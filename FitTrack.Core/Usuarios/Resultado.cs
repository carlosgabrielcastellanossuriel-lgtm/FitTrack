namespace FitTrack.Core.Usuarios;

// Un "paquete" simple para que el servicio le diga a la Api si salió bien y con qué mensaje.
public class Resultado
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    public static Resultado Ok(string mensaje)
    {
        return new Resultado { Exito = true, Mensaje = mensaje };
    }

    public static Resultado Error(string mensaje)
    {
        return new Resultado { Exito = false, Mensaje = mensaje };
    }
}