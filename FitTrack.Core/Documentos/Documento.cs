using FitTrack.Core.Usuarios;

namespace FitTrack.Core.Documentos;

// Archivo que sube un usuario. En la base de datos solo se guardan los datos del archivo;
// el contenido real se guarda en disco con el nombre de NombreGuardado.
public class Documento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;          // nombre original del archivo (el que ve el usuario)
    public string Tipo { get; set; } = string.Empty;            // content type (por ejemplo "application/pdf")
    public long Tamano { get; set; }                            // tamaño en bytes
    public DateTime Fecha { get; set; } = DateTime.UtcNow;      // hora universal

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }                       // dueño del documento (EF crea la relación con Usuarios)

    public string NombreGuardado { get; set; } = string.Empty;  // nombre real en disco: un GUID
    public bool Eliminado { get; set; } = false;                // true = borrado lógico, el archivo ya no se muestra
}
