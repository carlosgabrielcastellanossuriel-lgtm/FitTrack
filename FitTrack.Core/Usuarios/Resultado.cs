namespace FitTrack.Core.Usuarios;

// Un "paquete" simple para que el servicio le diga a la Api si salió bien y con qué mensaje.
public class Resultado
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    // El token es opcional: solo el login lo usa.
    public static Resultado Ok(string mensaje, string token = "")
    {
        return new Resultado { Exito = true, Mensaje = mensaje, Token = token };
    }

    public static Resultado Error(string mensaje)
    {
        return new Resultado { Exito = false, Mensaje = mensaje };
    }
}