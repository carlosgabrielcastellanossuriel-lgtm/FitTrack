namespace FitTrack.Core.Correos;

public enum EstadoCorreo
{
    Pendiente = 0,
    Enviado = 1,
    Fallido = 2
}

public class CorreoEnCola
{
    public const int MaxIntentos = 3;

    public int Id { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Cuerpo { get; set; } = string.Empty;
    public EstadoCorreo Estado { get; set; } = EstadoCorreo.Pendiente;
    public int Intentos { get; set; } = 0;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;// hora universal
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaUltimoIntento { get; set; }
    public string? UltimoError { get; set; }
}