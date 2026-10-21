using FitTrack.Core.Usuarios;

namespace FitTrack.Core.Permisos;

// Solicitud de un permiso hecha por un usuario. Sigue el ciclo de estados de TransicionesSolicitud.
public class SolicitudPermiso
{
    public int Id { get; set; }

    public int SolicitanteId { get; set; }
    public Usuario? Solicitante { get; set; }   // quién pidió el permiso (EF crea la relación con Usuarios)

    public Permiso Permiso { get; set; }         // permiso que se solicita

    // Estado de la solicitud. Toda solicitud nueva nace Pendiente.
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int? ResueltoPorId { get; set; }      // administrador que aprobó o rechazó
    public DateTime? FechaResolucion { get; set; } // cuándo se resolvió
    public string? MotivoRechazo { get; set; }   // por qué se rechazó, si aplica

    // ÚNICA puerta para cambiar el estado: consulta TransicionesSolicitud.
    // Devuelve false (y no cambia nada) si la transición no está permitida.
    public bool CambiarEstado(EstadoSolicitud nuevo)
    {
        if (!TransicionesSolicitud.EsPermitida(Estado, nuevo))
            return false;

        Estado = nuevo;
        return true;
    }
}
