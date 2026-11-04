namespace FitTrack.Negocio.Alimentos;

// Un alimento con sus valores nutricionales por cada 100 gramos.
public class Alimento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public double CaloriasPor100g { get; set; }
    public double ProteinaPor100g { get; set; }
    public double CarbohidratosPor100g { get; set; }
    public double GrasasPor100g { get; set; }
    // Si es null, es un alimento básico (lo ven todos). Si tiene un valor, es el Id del usuario que lo creó y solo él lo ve.
    public int? UsuarioCreadorId { get; set; }
}
