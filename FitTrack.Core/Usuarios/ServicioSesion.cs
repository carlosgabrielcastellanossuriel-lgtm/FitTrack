using System.Security.Cryptography;
using FitTrack.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Usuarios;

public class ServicioSesion
{
    private const int MaxIntentos = 5;           // fallos seguidos permitidos
    private const int MinutosBloqueo = 15;       // duración del bloqueo
    private const int HorasSesion = 8;           // cuánto dura una sesión

    private readonly ContextoBD _db;

    public ServicioSesion(ContextoBD db)
    {
        _db = db;
    }

    // ---------- LOGIN (RF-CA-03, RF-CA-19, RF-CA-15) ----------
    public async Task<Resultado> IniciarSesionAsync(string correo, string clave)
    {
        // Mismo mensaje para "correo no existe" y "contraseña mala" (RF-CA-03).
        string mensajeGenerico = "Correo o contraseña incorrectos.";

        string correoNormal = correo.Trim().ToLowerInvariant();
        Usuario? usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoNormal);

        if (usuario == null)
            return Resultado.Error(mensajeGenerico);

        // Si está bloqueado, se rechaza SIN mirar la contraseña:
        // así el sexto intento falla aunque la clave sea correcta (RF-CA-19).
        if (usuario.BloqueadoHasta != null && usuario.BloqueadoHasta > DateTime.UtcNow)
            return Resultado.Error("Cuenta bloqueada temporalmente. Intenta de nuevo más tarde.");

        // Contraseña incorrecta: se suma un fallo.
        if (!Claves.Verificar(clave, usuario.ContrasenaHash))
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= MaxIntentos)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
                usuario.IntentosFallidos = 0;   // al terminar el bloqueo se empieza de cero
            }

            await _db.SaveChangesAsync();
            return Resultado.Error(mensajeGenerico);
        }

        // Contraseña correcta pero cuenta sin activar (RF-CA-15).
        // Se avisa solo aquí, para no revelar si un correo existe a quien no sabe la clave.
        if (!usuario.Activo)
            return Resultado.Error("La cuenta no está activa. Revisa tu correo para activarla.");

        // Login correcto: el contador vuelve a cero.
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;

        // Se crea la sesión con un token aleatorio seguro.
        Sesion sesion = new Sesion();
        sesion.UsuarioId = usuario.Id;
        sesion.Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        sesion.FechaVencimiento = DateTime.UtcNow.AddHours(HorasSesion);
        _db.Sesiones.Add(sesion);
        await _db.SaveChangesAsync();

        return Resultado.Ok("Sesión iniciada.", sesion.Token);
    }

    // ---------- COMPROBAR UN TOKEN (RF-CA-07) ----------
    // Devuelve el usuario dueño del token, o null si el token no sirve.
    public async Task<Usuario?> ObtenerUsuarioAsync(string token)
    {
        Sesion? sesion = await _db.Sesiones
            .Include(s => s.Usuario)
            .FirstOrDefaultAsync(s => s.Token == token);

        if (sesion == null || sesion.Usuario == null)
            return null;
        if (sesion.Cerrada || sesion.FechaVencimiento < DateTime.UtcNow)
            return null;
        if (!sesion.Usuario.Activo)    // un usuario desactivado pierde sus sesiones (Parte 6)
            return null;

        return sesion.Usuario;
    }

    // ---------- LOGOUT (RF-CA-18) ----------
    public async Task CerrarSesionAsync(string token)
    {
        Sesion? sesion = await _db.Sesiones.FirstOrDefaultAsync(s => s.Token == token);
        if (sesion == null)
            return;

        sesion.Cerrada = true;
        await _db.SaveChangesAsync();
    }
}