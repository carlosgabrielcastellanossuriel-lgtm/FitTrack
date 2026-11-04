using FitTrack.Negocio.Alimentos;

namespace FitTrack.Negocio.Resumenes;

// Una comida registrada: qué alimento, cuántos gramos y lo que aportó. Un ResumenDiario tiene muchas.
public class RegistroComida
{
    public int Id { get; set; }

    public int ResumenDiarioId { get; set; }
    public ResumenDiario? ResumenDiario { get; set; }    // de qué resumen del día es esta comida

    public int AlimentoId { get; set; }
    public Alimento? Alimento { get; set; }              // qué alimento se comió

    public double Gramos { get; set; }
    public double Calorias { get; set; }
    public double Proteinas { get; set; }
    public double Carbohidratos { get; set; }
    public double Grasas { get; set; }
}
