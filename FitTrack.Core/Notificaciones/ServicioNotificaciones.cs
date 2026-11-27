using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Notificaciones;

/// <summary>
/// Gestiona las notificaciones internas de los usuarios y el encolado opcional de correos.
/// </summary>
public class ServicioNotificaciones
{
    private readonly ContextoBD _db;
    private readonly ServicioColaCorreo _cola;

    public ServicioNotificaciones(ContextoBD db, ServicioColaCorreo cola)
    {
        _db = db;
        _cola = cola;
    }

    public async Task NotificarAsync(int usuarioId, string titulo, string mensaje)
    {
        Usuario? usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
            return;

        _db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = usuarioId,
            Titulo = titulo,
            Mensaje = mensaje,
            Leida = false,
            FechaCreacion = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        if (usuario.NotificarPorCorreo)
        {
            string asunto = titulo.Length > 200 ? titulo[..200] : titulo;
            await _cola.EncolarAsync(usuario.Correo, asunto, mensaje);
        }
    }

    public async Task NotificarAdministradoresAsync(string titulo, string mensaje)
    {
        List<int> adminIds = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == Rol.Administrador && u.Activo)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (int adminId in adminIds)
        {
            await NotificarAsync(adminId, titulo, mensaje);
        }
    }

    public async Task<List<Notificacion>> ListarAsync(int usuarioId, bool soloNoLeidas)
    {
        IQueryable<Notificacion> consulta = _db.Notificaciones
            .AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId);

        if (soloNoLeidas)
            consulta = consulta.Where(n => !n.Leida);

        return await consulta
            .OrderByDescending(n => n.FechaCreacion)
            .ThenByDescending(n => n.Id)
            .ToListAsync();
    }

    public async Task<Resultado> MarcarLeidaAsync(int usuarioId, int notificacionId)
    {
        Notificacion? notificacion = await _db.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);

        if (notificacion == null)
            return Resultado.Error("Notificación no encontrada.");

        notificacion.Leida = true;
        await _db.SaveChangesAsync();

        return Resultado.Ok("Notificación marcada como leída.");
    }

    public async Task<bool> ObtenerPreferenciaAsync(int usuarioId)
    {
        Usuario? usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        return usuario?.NotificarPorCorreo ?? false;
    }

    public async Task<Resultado> CambiarPreferenciaAsync(int usuarioId, bool correo)
    {
        Usuario? usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return Resultado.Error("Usuario no encontrado.");

        usuario.NotificarPorCorreo = correo;
        await _db.SaveChangesAsync();

        return Resultado.Ok("Preferencias actualizadas.");
    }
}
