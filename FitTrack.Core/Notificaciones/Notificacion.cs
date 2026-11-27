using FitTrack.Core.Usuarios;

namespace FitTrack.Core.Notificaciones;

public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
