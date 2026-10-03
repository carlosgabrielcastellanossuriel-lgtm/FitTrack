namespace FitTrack.Core.Usuarios;

public class Usuario
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;          // siempre en minúsculas
    public string ContrasenaHash { get; set; } = string.Empty;  // NUNCA la contraseña real
    public bool Activo { get; set; } = false;                   // nace inactivo (RF-CA-15)
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int IntentosFallidos { get; set; } = 0;     // fallos seguidos (RF-CA-19)
    public DateTime? BloqueadoHasta { get; set; }      // si tiene fecha futura, está bloqueado}
}