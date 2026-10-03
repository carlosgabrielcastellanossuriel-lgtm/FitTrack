namespace FitTrack.Core.Correos;

public enum EstadoCorreo
{
    Pendiente = 0,
    Enviado = 1
}

public class CorreoEnCola
{
    public int Id { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Cuerpo { get; set; } = string.Empty;
    public EstadoCorreo Estado { get; set; } = EstadoCorreo.Pendiente;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;// hora universal
    public DateTime? FechaEnvio { get; set; }  
}