using FitTrack.Core.Usuarios;

namespace FitTrack.Negocio.Resumenes;

// Un resumen por usuario y por día: acumula lo consumido y guarda el estado del día.
public class ResumenDiario
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }    // de quién es el resumen (EF crea la relación con Usuarios)

    public DateOnly Fecha { get; set; }
    public double CaloriasAcumuladas { get; set; } = 0;
    public double ProteinasAcumuladas { get; set; } = 0;
    public double CarbohidratosAcumulados { get; set; } = 0;
    public double GrasasAcumuladas { get; set; } = 0;

    // El atributo de estado. Todo resumen nuevo nace En Progreso.
    public EstadoResumen EstadoActual { get; set; } = EstadoResumen.EnProgreso;

    // ÚNICA puerta para cambiar el estado: consulta las reglas de TransicionesResumen.
    // Devuelve false (y no cambia nada) si la transición no está permitida.
    public bool CambiarEstado(EstadoResumen nuevo)
    {
        if (!TransicionesResumen.EsPermitida(EstadoActual, nuevo))
            return false;

        EstadoActual = nuevo;
        return true;
    }
}