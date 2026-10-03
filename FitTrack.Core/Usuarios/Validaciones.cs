using System.Net.Mail;

namespace FitTrack.Core.Usuarios;

public static class Validaciones
{
    public static bool CorreoValido(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo) || correo.Length > 320)
            return false;

        try
        {
            // MailAddress lanza FormatException si el correo está mal formado.
            MailAddress direccion = new MailAddress(correo.Trim());

            // Dos revisiones extra: que sea SOLO la dirección (no "Nombre <a@b.com>")
            // y que el dominio tenga un punto (rechaza "juan@gmail").
            return direccion.Address == correo.Trim() && direccion.Host.Contains('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }

    // Política (RF-CA-14): mínimo 8 caracteres, con letras y números.
    public static bool ClaveValida(string clave)
    {
        if (string.IsNullOrEmpty(clave) || clave.Length < 8)
            return false;

        bool tieneLetra = clave.Any(char.IsLetter);   // ¿hay al menos una letra?
        bool tieneNumero = clave.Any(char.IsDigit);   // ¿hay al menos un número?
        return tieneLetra && tieneNumero;
    }
}