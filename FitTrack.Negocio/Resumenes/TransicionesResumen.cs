namespace FitTrack.Negocio.Resumenes;

// ÚNICO lugar donde se declaran las transiciones (RD-04).
// Si un cambio de estado no está aquí, no se puede hacer.
public static class TransicionesResumen
{
    // Estado terminal (RF-NEG-05): al llegar aquí, no se sale nunca.
    public static readonly EstadoResumen EstadoTerminal = EstadoResumen.Cerrado;

    // Transiciones PERMITIDAS: para cada estado, a cuáles puede pasar.
    private static readonly Dictionary<EstadoResumen, EstadoResumen[]> Permitidas = new()
    {
        { EstadoResumen.EnProgreso,   new[] { EstadoResumen.DentroDeMeta, EstadoResumen.Excedido } },
        { EstadoResumen.DentroDeMeta, new[] { EstadoResumen.Excedido, EstadoResumen.Cerrado } },
        { EstadoResumen.Excedido,     new[] { EstadoResumen.Cerrado } },
        { EstadoResumen.Cerrado,      new EstadoResumen[] { } }    // terminal: no sale a ningún lado
    };

    // Transiciones PROHIBIDAS de forma explícita (RF-NEG-04).
    // Ya quedarían rechazadas por no estar en la lista de arriba;
    // se declaran aquí para que la regla sea visible y se revise primero.
    private static readonly (EstadoResumen Desde, EstadoResumen Hacia)[] Prohibidas =
    {
        (EstadoResumen.Cerrado,  EstadoResumen.EnProgreso),     // un día cerrado no se reabre
        (EstadoResumen.Excedido, EstadoResumen.DentroDeMeta)    // un día excedido no vuelve atrás
    };

    public static bool EsPermitida(EstadoResumen desde, EstadoResumen hacia)
    {
        // 1. Si está en la lista de prohibidas, se rechaza.
        if (Prohibidas.Contains((desde, hacia)))
            return false;

        // 2. Si no, solo se acepta lo que esté en la lista de permitidas.
        return Permitidas[desde].Contains(hacia);
    }

    public static bool EsTerminal(EstadoResumen estado)
    {
        return estado == EstadoTerminal;
    }
}