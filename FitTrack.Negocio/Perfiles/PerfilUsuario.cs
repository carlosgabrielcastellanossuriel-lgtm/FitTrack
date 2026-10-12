using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Negocio.Perfiles;

// Los datos del cuerpo y el objetivo de un usuario, junto con las metas que salen de ellos.
// Un usuario tiene como máximo un perfil: el índice único lo garantiza la base de datos.
[Index(nameof(UsuarioId), IsUnique = true)]
public class PerfilUsuario
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }    // de quién es el perfil (EF crea la relación con Usuarios)

    // Datos del usuario. Unidades del proyecto: libras y pulgadas.
    public int Edad { get; set; }
    public double Peso { get; set; }         // en libras
    public double Altura { get; set; }       // en pulgadas totales (5 pies 10 pulgadas = 70)
    public Sexo Sexo { get; set; }
    public NivelActividad NivelActividad { get; set; }
    public ObjetivoNutricional Objetivo { get; set; }

    // Metas diarias calculadas por CalculadoraMetas (las llena ServicioPerfil).
    public int CaloriasMeta { get; set; }
    public double ProteinaMeta { get; set; }         // en gramos
    public double CarbohidratosMeta { get; set; }    // en gramos
    public double GrasasMeta { get; set; }           // en gramos
}
