namespace FitTrack.Core.Usuarios;

// Es el "cupón" del enlace de activación: sirve una vez y vence.
public class TokenActivacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }                  // de quién es este token
    public Usuario? Usuario { get; set; }               // permite que EF cree la relación entre tablas
    public string Token { get; set; } = string.Empty;   // el texto largo que va en el enlace
    public DateTime FechaVencimiento { get; set; }
    public bool Usado { get; set; } = false;            // true = ya no sirve
}