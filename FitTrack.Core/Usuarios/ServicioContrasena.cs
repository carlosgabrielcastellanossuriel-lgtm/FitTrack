using System.Security.Cryptography;
using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Usuarios;

public class ServicioContrasena
{
    private readonly ContextoBD _db;
    private readonly ServicioColaCorreo _cola;   // para dejar el correo en la cola (NO lo envía)

    public ServicioContrasena(ContextoBD db, ServicioColaCorreo cola)
    {
        _db = db;
        _cola = cola;
    }

    // ---------- PEDIR RECUPERACIÓN (RF-CA-09, RF-CA-10) ----------
    // No devuelve nada a propósito: la Api siempre responde igual, exista o no el correo.
    public async Task SolicitarRecuperacionAsync(string correo)
    {
        string correoNormal = correo.Trim().ToLowerInvariant();
        Usuario? usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoNormal);

        // Si no existe, no se hace nada (sin avisar a quien pregunta).
        if (usuario == null)
            return;

        await EnviarCodigoAsync(usuario);
    }

    // ---------- CREAR EL CÓDIGO Y ENCOLAR EL CORREO ----------
    // Es "public" porque en la Parte 6 el restablecimiento forzado del Administrador la reutiliza.
    public async Task EnviarCodigoAsync(Usuario usuario)
    {
        // Se anulan los códigos anteriores sin usar: solo vale el último.
        List<TokenRecuperacion> anteriores = await _db.TokensRecuperacion
            .Where(t => t.UsuarioId == usuario.Id && !t.Usado)
            .ToListAsync();
        foreach (TokenRecuperacion anterior in anteriores)
        {
            anterior.Usado = true;
        }

        TokenRecuperacion nuevo = new TokenRecuperacion();
        nuevo.UsuarioId = usuario.Id;
        // 32 bytes aleatorios seguros convertidos a texto (igual que el token de activación).
        nuevo.Codigo = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        nuevo.FechaVencimiento = DateTime.UtcNow.AddHours(1);   // vence en 1 hora
        _db.TokensRecuperacion.Add(nuevo);
        await _db.SaveChangesAsync();

        string urlBase = Environment.GetEnvironmentVariable("FITTRACK_URL_BASE") ?? "http://localhost:5262";

        // Solo se deja en la cola (RF-NOT-08). El Enviador lo manda después.
        await _cola.EncolarAsync(
            usuario.Correo,
            "Recupera tu contraseña de FitTrack",
            $"Tu código de recuperación (vence en 1 hora, sirve una sola vez):\n\n{nuevo.Codigo}\n\n" +
            $"Envíalo junto con tu nueva contraseña a {urlBase}/api/contrasena/restablecer");
    }

    // ---------- DEFINIR CONTRASEÑA NUEVA CON EL CÓDIGO (RF-CA-11, RF-CA-12, RF-CA-14) ----------
    public async Task<Resultado> RestablecerAsync(string codigo, string claveNueva)
    {
        // Primero la política: si la clave es débil se rechaza SIN gastar el código.
        if (!Validaciones.ClaveValida(claveNueva))
            return Resultado.Error("La contraseña debe tener al menos 8 caracteres, con letras y números.");

        TokenRecuperacion? t = await _db.TokensRecuperacion
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Codigo == codigo);

        // Un solo mensaje para todos los fallos: no existe, ya usado o vencido.
        if (t == null || t.Usuario == null || t.Usado || t.FechaVencimiento < DateTime.UtcNow)
            return Resultado.Error("El código no es válido o ya venció.");

        t.Usado = true;                                          // el código se gasta
        t.Usuario.ContrasenaHash = Claves.Hashear(claveNueva);   // la anterior deja de servir
        await CerrarSesionesAsync(t.Usuario.Id);                 // RF-CA-12 (también guarda los cambios)

        return Resultado.Ok("Contraseña actualizada. Inicia sesión con la nueva.");
    }

    // ---------- CAMBIAR LA PROPIA CONTRASEÑA CON SESIÓN (RF-CA-22) ----------
    public async Task<Resultado> CambiarAsync(Usuario usuario, string claveActual, string claveNueva)
    {
        // Si la actual no coincide, se rechaza y nada cambia.
        if (!Claves.Verificar(claveActual, usuario.ContrasenaHash))
            return Resultado.Error("La contraseña actual es incorrecta.");

        if (!Validaciones.ClaveValida(claveNueva))
            return Resultado.Error("La contraseña debe tener al menos 8 caracteres, con letras y números.");

        usuario.ContrasenaHash = Claves.Hashear(claveNueva);
        await CerrarSesionesAsync(usuario.Id);   // incluye la sesión actual: hay que volver a iniciar

        return Resultado.Ok("Contraseña cambiada. Inicia sesión de nuevo.");
    }

    // ---------- CERRAR TODAS LAS SESIONES DE UN USUARIO (RF-CA-12) ----------
    // Es "public" porque en la Parte 6 también se usa al desactivar a un usuario.
    // Guarda todos los cambios pendientes del contexto, incluidos los que hizo quien la llamó.
    public async Task CerrarSesionesAsync(int usuarioId)
    {
        List<Sesion> abiertas = await _db.Sesiones
            .Where(s => s.UsuarioId == usuarioId && !s.Cerrada)
            .ToListAsync();

        foreach (Sesion s in abiertas)
        {
            s.Cerrada = true;   // el token de esa sesión deja de servir
        }

        await _db.SaveChangesAsync();
    }
}