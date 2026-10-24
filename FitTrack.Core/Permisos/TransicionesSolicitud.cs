namespace FitTrack.Core.Permisos;

// ÚNICO lugar donde se declaran las transiciones de una solicitud.
// Si una transición no está aquí, no se puede hacer.
public static class TransicionesSolicitud
{
    // Transiciones PERMITIDAS: desde cada estado, a cuáles puede pasar.
    private static readonly Dictionary<EstadoSolicitud, EstadoSolicitud[]> Permitidas = new()
    {
        { EstadoSolicitud.Pendiente,   new[] { EstadoSolicitud.Aprobada, EstadoSolicitud.Rechazada } },
        { EstadoSolicitud.Aprobada,    new[] { EstadoSolicitud.Aplicada, EstadoSolicitud.Pendiente } },
        { EstadoSolicitud.Rechazada,   new EstadoSolicitud[] { } }, // terminal: no sale a ningún lado
        { EstadoSolicitud.Aplicada,    new EstadoSolicitud[] { } }  // terminal: ya se aplicó
    };

    public static bool EsPermitida(EstadoSolicitud desde, EstadoSolicitud hacia)
    {
        // Solo se acepta lo que esté en la lista de permitidas; todo lo demás es falso.
        return Permitidas[desde].Contains(hacia);
    }
}
