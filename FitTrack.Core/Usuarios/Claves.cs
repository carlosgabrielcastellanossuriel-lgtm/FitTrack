namespace FitTrack.Core.Usuarios;

// "static" = no hace falta crear un objeto, se usa directo: Claves.Hashear(...)
public static class Claves
{
    // BCrypt genera una SAL aleatoria y la guarda dentro del propio hash.
    // Por eso dos usuarios con la misma contraseña obtienen hashes distintos (RF-CA-02).
    public static string Hashear(string clave)
    {
        return BCrypt.Net.BCrypt.HashPassword(clave);
    }

    // Se usará en el login: no se "descifra" nada,
    // BCrypt hashea lo escrito con la misma sal y compara.
    public static bool Verificar(string clave, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(clave, hash);
    }
}