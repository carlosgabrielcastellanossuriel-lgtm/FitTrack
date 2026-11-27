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
        IQueryable<SolicitudPermiso> consulta = _db.SolicitudesPermiso.AsNoTracking();

        // El día "desde" entra desde las 00:00 y el día "hasta" entra completo.
        if (desde.HasValue)
        {
            DateTime inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(s => s.FechaCreacion >= inicio);
        }

        if (hasta.HasValue)
        {
            DateTime fin = hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(s => s.FechaCreacion < fin);
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
