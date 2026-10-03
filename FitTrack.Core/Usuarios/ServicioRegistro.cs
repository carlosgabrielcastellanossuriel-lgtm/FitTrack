using System.Security.Cryptography;
using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Usuarios;

public class ServicioRegistro
{
    private readonly ContextoBD _db;
    private readonly ServicioColaCorreo _cola;   // para dejar el correo en la cola (NO lo envía)

    public ServicioRegistro(ContextoBD db, ServicioColaCorreo cola)
    {
        _db = db;
        _cola = cola;
    }

    // ---------- REGISTRO (RF-CA-01, 02, 14, 15) ----------
    public async Task<Resultado> RegistrarAsync(string correo, string clave)
    {
        // 1. Validaciones: si algo falla, se rechaza con un mensaje controlado (RD-07).
        if (!Validaciones.CorreoValido(correo))
            return Resultado.Error("El correo no tiene un formato válido.");

        if (!Validaciones.ClaveValida(clave))
            return Resultado.Error("La contraseña debe tener al menos 8 caracteres, con letras y números.");

        // 2. Se guarda siempre en minúsculas para que "A@x.com" y "a@x.com" cuenten como el mismo.
        string correoNormal = correo.Trim().ToLowerInvariant();

        // 3. Correo único (RF-CA-01).
        bool yaExiste = await _db.Usuarios.AnyAsync(u => u.Correo == correoNormal);
        if (yaExiste)
            return Resultado.Error("Ese correo ya está registrado.");

        // 4. Se crea el usuario: inactivo y con la contraseña hasheada.
        Usuario usuario = new Usuario();
        usuario.Correo = correoNormal;
        usuario.ContrasenaHash = Claves.Hashear(clave);
        usuario.Activo = false;

        _db.Usuarios.Add(usuario);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Pasa si dos personas se registran con el mismo correo en el mismo instante:
            // el índice único de la base de datos lo bloquea.
            return Resultado.Error("Ese correo ya está registrado.");
        }

        // 5. Se crea el token y se deja el correo en la cola.
        await CrearTokenYEncolarCorreoAsync(usuario);

        return Resultado.Ok("Cuenta creada. Revisa tu correo para activarla.");
    }

    // ---------- ACTIVACIÓN (RF-CA-16) ----------
    public async Task<Resultado> ActivarAsync(string token)
    {
        // Include trae también al usuario dueño del token (sin él, t.Usuario vendría vacío).
        TokenActivacion? t = await _db.TokensActivacion
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Token == token);

        // Un solo mensaje para todos los fallos: no existe, ya usado o vencido.
        if (t == null || t.Usuario == null || t.Usado || t.FechaVencimiento < DateTime.UtcNow)
            return Resultado.Error("El enlace no es válido o ya venció.");

        t.Usado = true;              // el cupón se gasta
        t.Usuario.Activo = true;     // la cuenta se activa
        await _db.SaveChangesAsync();

        return Resultado.Ok("Cuenta activada. Ya puedes iniciar sesión.");
    }

    // ---------- REENVÍO (RF-CA-17) ----------
    // No devuelve nada a propósito: la Api siempre responde igual, exista o no el correo.
    public async Task ReenviarActivacionAsync(string correo)
    {
        string correoNormal = correo.Trim().ToLowerInvariant();

        Usuario? usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoNormal);

        // Si no existe o ya está activo, no se hace nada (sin avisar a quien pregunta).
        if (usuario == null || usuario.Activo)
            return;

        await CrearTokenYEncolarCorreoAsync(usuario);
    }

    // ---------- Pieza compartida por registro y reenvío ----------
    private async Task CrearTokenYEncolarCorreoAsync(Usuario usuario)
    {
        // Se anulan los tokens anteriores sin usar: el reenvío invalida el enlace viejo (RF-CA-17).
        List<TokenActivacion> anteriores = await _db.TokensActivacion
            .Where(t => t.UsuarioId == usuario.Id && !t.Usado)
            .ToListAsync();
        foreach (TokenActivacion anterior in anteriores)
        {
            anterior.Usado = true;
        }

        TokenActivacion nuevo = new TokenActivacion();
        nuevo.UsuarioId = usuario.Id;
        // 32 bytes aleatorios seguros, convertidos a texto (64 caracteres).
        // RandomNumberGenerator es el generador pensado para seguridad; Random común es predecible.
        nuevo.Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        nuevo.FechaVencimiento = DateTime.UtcNow.AddHours(24);   // vence en 24 horas
        _db.TokensActivacion.Add(nuevo);
        await _db.SaveChangesAsync();

        // La dirección base se lee de una variable de entorno; si no existe, usa la local.
        string urlBase = Environment.GetEnvironmentVariable("FITTRACK_URL_BASE") ?? "http://localhost:5262";
        string enlace = $"{urlBase}/api/auth/activar?token={nuevo.Token}";

        // Solo se deja en la cola (RF-NOT-08). El Enviador lo manda después.
        await _cola.EncolarAsync(
            usuario.Correo,
            "Activa tu cuenta de FitTrack",
            $"Hola. Abre este enlace para activar tu cuenta (vence en 24 horas):\n\n{enlace}");
    }
}