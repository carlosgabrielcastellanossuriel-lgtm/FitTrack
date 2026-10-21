using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Permisos;

public class ServicioPermisos
{
    private readonly ContextoBD _db;

    public ServicioPermisos(ContextoBD db)
    {
        _db = db;
    }

    // ---------- CREAR SOLICITUD (RF-GP-01) ----------
    public async Task<Resultado> CrearAsync(Usuario solicitante, Permiso permiso)
    {
        // El permiso debe ser uno de los definidos en el enum.
        if (!Enum.IsDefined(permiso))
            return Resultado.Error("Permiso no válido.");

        SolicitudPermiso solicitud = new()
        {
            SolicitanteId = solicitante.Id,
            Permiso = permiso,
            Estado = EstadoSolicitud.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };

        _db.SolicitudesPermiso.Add(solicitud);
        await _db.SaveChangesAsync();

        return Resultado.Ok("Solicitud de permiso creada.");
    }

    // ---------- LISTAR PENDIENTES (RF-GP-02) ----------
    public async Task<List<SolicitudPermiso>> ListarPendientesAsync()
    {
        // AsNoTracking: solo se lee, EF no necesita vigilar estos objetos.
        return await _db.SolicitudesPermiso
            .AsNoTracking()
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .OrderBy(s => s.FechaCreacion)
            .ToListAsync();
    }

    // ---------- APROBAR (RF-GP-03, RF-GP-05, RF-GP-06, RF-GP-07) ----------
    public async Task<Resultado> AprobarAsync(int solicitudId, Usuario admin)
    {
        SolicitudPermiso? solicitud = await _db.SolicitudesPermiso.FindAsync(solicitudId);
        if (solicitud == null)
            return Resultado.Error("Solicitud no encontrada.");

        // No puedes resolver tu propia solicitud.
        if (solicitud.SolicitanteId == admin.Id)
            return Resultado.Error("No puedes resolver tu propia solicitud.");

        // Solo se permite la transición Pendiente -> Aprobada.
        if (!solicitud.CambiarEstado(EstadoSolicitud.Aprobada))
            return Resultado.Error("No se puede aprobar la solicitud en su estado actual.");

        solicitud.ResueltoPorId = admin.Id;
        solicitud.FechaResolucion = DateTime.UtcNow;

        // Aplica el permiso: agrega PermisoUsuario si el usuario no lo tiene ya.
        if (!await TieneAsync(solicitud.SolicitanteId, solicitud.Permiso))
        {
            _db.PermisosUsuario.Add(new PermisoUsuario
            {
                UsuarioId = solicitud.SolicitanteId,
                Permiso = solicitud.Permiso
            });
        }

        // Pasa la solicitud a Aplicada.
        solicitud.CambiarEstado(EstadoSolicitud.Aplicada);

        // Un solo guardado al final con todo.
        await _db.SaveChangesAsync();

        return Resultado.Ok("Permiso aprobado y aplicado.");
    }

    // ---------- RECHAZAR (RF-GP-04) ----------
    public async Task<Resultado> RechazarAsync(int solicitudId, Usuario admin, string motivo)
    {
        // El motivo es obligatorio.
        if (string.IsNullOrWhiteSpace(motivo))
            return Resultado.Error("Debes indicar un motivo para rechazar.");

        SolicitudPermiso? solicitud = await _db.SolicitudesPermiso.FindAsync(solicitudId);
        if (solicitud == null)
            return Resultado.Error("Solicitud no encontrada.");

        // No puedes rechazar tu propia solicitud.
        if (solicitud.SolicitanteId == admin.Id)
            return Resultado.Error("No puedes resolver tu propia solicitud.");

        // Solo se permite la transición Pendiente -> Rechazada.
        if (!solicitud.CambiarEstado(EstadoSolicitud.Rechazada))
            return Resultado.Error("No se puede rechazar la solicitud en su estado actual.");

        solicitud.ResueltoPorId = admin.Id;
        solicitud.FechaResolucion = DateTime.UtcNow;
        solicitud.MotivoRechazo = motivo;

        await _db.SaveChangesAsync();

        return Resultado.Ok("Solicitud rechazada.");
    }

    // ---------- TIENE PERMISO ----------
    public async Task<bool> TieneAsync(int usuarioId, Permiso permiso)
    {
        return await _db.PermisosUsuario
            .AnyAsync(p => p.UsuarioId == usuarioId && p.Permiso == permiso);
    }
}
