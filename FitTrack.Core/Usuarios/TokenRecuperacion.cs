namespace FitTrack.Core.Usuarios;

// Igual que TokenActivacion: un código que sirve una vez y vence.
public class TokenRecuperacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Codigo { get; set; } = string.Empty;   // el código que llega por correo
    public DateTime FechaVencimiento { get; set; }
    public bool Usado { get; set; } = false;             // true = ya no sirve
}