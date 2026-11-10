using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Documentos;

public class ServicioDocumentos
{
    private readonly ContextoBD _db;

    // Carpeta donde se guardan los archivos. Sale de una variable de entorno para poder
    // apuntar a otra ruta (los tests la cambian). Si no existe la variable, usa "documentos".
    public string Carpeta { get; set; } = Environment.GetEnvironmentVariable("FITTRACK_RUTA_DOCUMENTOS") ?? "documentos";

    // Tamaño máximo permitido para un archivo: 10 MB (RD-07).
    public const long TamanoMaximo = 10 * 1024 * 1024;

    public ServicioDocumentos(ContextoBD db)
    {
        _db = db;
    }

    // ---------- SUBIR (RD-07) ----------
    public async Task<Resultado> SubirAsync(Usuario usuario, string nombre, string tipo, Stream contenido, long tamano)
    {
        // Solo el nombre del archivo, sin rutas: evita que alguien mande "../../algo".
        string nombreLimpio = Path.GetFileName(nombre);

        if (string.IsNullOrWhiteSpace(nombreLimpio))
            return Resultado.Error("El nombre del archivo es obligatorio.");

        if (tamano == 0)
            return Resultado.Error("El archivo está vacío.");

        if (tamano > TamanoMaximo)
            return Resultado.Error("El archivo supera el tamaño máximo de 10 MB.");

        // El nombre en disco es un GUID para que dos archivos con el mismo nombre no choquen.
        string nombreGuardado = Guid.NewGuid().ToString();

        // Crea la carpeta si todavía no existe.
        Directory.CreateDirectory(Carpeta);

        // Copia el contenido a disco.
        string ruta = Path.Combine(Carpeta, nombreGuardado);
        using (FileStream archivo = File.Create(ruta))
        {
            await contenido.CopyToAsync(archivo);
        }

        // Y después guarda el registro en la base.
        Documento documento = new()
        {
            Nombre = nombreLimpio,
            Tipo = tipo,
            Tamano = tamano,
            Fecha = DateTime.UtcNow,
            UsuarioId = usuario.Id,
            NombreGuardado = nombreGuardado
        };

        _db.Documentos.Add(documento);
        await _db.SaveChangesAsync();

        return Resultado.Ok("Documento subido.");
    }

    // ---------- LISTAR ----------
    public async Task<List<Documento>> ListarAsync(Usuario usuario)
    {
        // AsNoTracking: solo se lee, EF no necesita vigilar estos objetos.
        IQueryable<Documento> consulta = _db.Documentos
            .AsNoTracking()
            .Where(d => !d.Eliminado);

        // El Administrador ve todos; los demás solo los suyos.
        if (usuario.Rol != Rol.Administrador)
            consulta = consulta.Where(d => d.UsuarioId == usuario.Id);

        return await consulta
            .OrderBy(d => d.Fecha)
            .ToListAsync();
    }

    // ---------- DESCARGAR ----------
    public async Task<Documento?> ObtenerParaDescargaAsync(Usuario usuario, int id)
    {
        Documento? documento = await _db.Documentos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && !d.Eliminado);

        // No existe, está eliminado o es de otro usuario y quien pide no es Administrador.
        if (documento == null)
            return null;

        if (documento.UsuarioId != usuario.Id && usuario.Rol != Rol.Administrador)
            return null;

        return documento;
    }

    // ---------- RUTA EN DISCO ----------
    public string RutaDelArchivo(Documento documento)
    {
        return Path.Combine(Carpeta, documento.NombreGuardado);
    }

    // ---------- ELIMINAR (borrado lógico) ----------
    public async Task<Resultado> EliminarAsync(Usuario usuario, int id)
    {
        // Esta consulta sí vigila el objeto porque hay que modificarlo.
        Documento? documento = await _db.Documentos
            .FirstOrDefaultAsync(d => d.Id == id && !d.Eliminado);

        // Mismo mensaje si no existe o es ajeno: no se revela que el documento existe.
        if (documento == null)
            return Resultado.Error("Documento no encontrado.");

        if (documento.UsuarioId != usuario.Id && usuario.Rol != Rol.Administrador)
            return Resultado.Error("Documento no encontrado.");

        // Borrado lógico: el registro y el archivo siguen en disco, solo se marca.
        documento.Eliminado = true;
        await _db.SaveChangesAsync();

        return Resultado.Ok("Documento eliminado.");
    }
}
