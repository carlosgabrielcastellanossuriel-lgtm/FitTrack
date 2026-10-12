namespace FitTrack.Negocio.Perfiles;

// Los datos que el usuario manda para crear o actualizar su perfil.
// Las metas NO van aquí: las calcula el sistema (CalculadoraMetas).
public class DatosPerfil
{
    public int Edad { get; set; }
    public double Peso { get; set; }       // en libras
    public double Altura { get; set; }     // en pulgadas totales (5 pies 10 pulgadas = 70)
    public Sexo Sexo { get; set; }
    public NivelActividad NivelActividad { get; set; }
    public ObjetivoNutricional Objetivo { get; set; }
}
