namespace FitTrack.Core.Usuarios;

// Los dos roles del sistema (RF-CA-04). Cada usuario tiene exactamente uno.
// Se guardan como número en la base: Estandar = 0, Administrador = 1.
public enum Rol
{
    Estandar = 0,
    Administrador = 1
}