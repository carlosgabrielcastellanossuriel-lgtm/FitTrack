using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Perfiles;

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

    // Registra una comida: suma sus valores y recalcula el estado del día.
    // Devuelve false (sin cambiar nada) si el día ya está cerrado.
    public bool RegistrarComida(double calorias, double proteinas, double carbohidratos, double grasas, PerfilUsuario perfil)
    {
        // 1. Un día cerrado no acepta más comidas.
        if (TransicionesResumen.EsTerminal(EstadoActual))
            return false;

        // 2. Se suman los valores de la comida a los acumulados del día.
        CaloriasAcumuladas += calorias;
        ProteinasAcumuladas += proteinas;
        CarbohidratosAcumulados += carbohidratos;
        GrasasAcumuladas += grasas;

        // 3. ¿Algún acumulado supera su meta del perfil?
        bool excede = CaloriasAcumuladas > perfil.CaloriasMeta || ProteinasAcumuladas > perfil.ProteinaMeta || CarbohidratosAcumulados > perfil.CarbohidratosMeta || GrasasAcumuladas > perfil.GrasasMeta;

        // 4. Estado que corresponde según el resultado anterior.
        EstadoResumen nuevo = excede ? EstadoResumen.Excedido : EstadoResumen.DentroDeMeta;

        // 5. Solo se cambia si es distinto (CambiarEstado valida la transición).
        if (nuevo != EstadoActual)
            CambiarEstado(nuevo);

        // 6. La comida quedó registrada.
        return true;
    }
}