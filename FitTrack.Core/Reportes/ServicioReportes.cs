using FitTrack.Core.Datos;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Reportes;

// Reportes del Core: cuentan solicitudes de permiso agrupadas por estado.
// La agrupación la hace la base de datos con GroupBy; nunca se trae la lista completa.
public class ServicioReportes
{
    private readonly ContextoBD _db;

    public ServicioReportes(ContextoBD db)
    {
        _db = db;
    }

    // Devuelve una fila por estado con el total de solicitudes.
    // El Administrador ve todas; cualquier otro usuario solo ve las suyas.
    public async Task<List<SolicitudesPorEstado>> SolicitudesPorEstadoAsync(Usuario consultante, DateOnly? desde, DateOnly? hasta)
    {
        // La regla vive en el servicio, no solo en el controlador (RD-02).
        if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            throw new ArgumentException("La fecha desde no puede ser mayor que hasta.");

        IQueryable<SolicitudPermiso> consulta = _db.SolicitudesPermiso.AsNoTracking();

        // El día "desde" entra desde las 00:00 y el día "hasta" entra completo.
        if (desde.HasValue)
        {
            DateTime inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(s => s.FechaCreacion >= inicio);
        }

        if (hasta.HasValue)
        {
            // Se usa el último instante del día en vez de AddDays(1): así
            // hasta = 9999-12-31 (DateOnly.MaxValue) no desborda el DateTime.
            DateTime fin = hasta.Value.ToDateTime(TimeOnly.MaxValue);
            consulta = consulta.Where(s => s.FechaCreacion <= fin);
        }

        if (consultante.Rol != Rol.Administrador)
            consulta = consulta.Where(s => s.SolicitanteId == consultante.Id);

        return await consulta
            .GroupBy(s => s.Estado)
            .Select(g => new SolicitudesPorEstado
            {
                Estado = g.Key,
                Total = g.Count()
            })
            .ToListAsync();
    }
}

// Una fila del reporte: un estado y cuántas solicitudes tiene.
public class SolicitudesPorEstado
{
    public EstadoSolicitud Estado { get; set; }
    public int Total { get; set; }
}
