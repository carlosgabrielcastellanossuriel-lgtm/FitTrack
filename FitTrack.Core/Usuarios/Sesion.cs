namespace FitTrack.Core.Usuarios;

// Cada inicio de sesión crea una fila aquí. El token es la "tarjeta" que el cliente
// manda en cada petición para demostrar que ya inició sesión.
public class Sesion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaVencimiento { get; set; }
    public bool Cerrada { get; set; } = false;      // true = el token ya no sirve (RF-CA-18)
}