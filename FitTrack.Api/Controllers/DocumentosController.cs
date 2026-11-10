using FitTrack.Api.Seguridad;
using FitTrack.Core.Documentos;
using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Todas las operaciones exigen sesión y las reglas de permiso las decide el servicio (RD-06).
[RequiereSesion]
[ApiController]
[Route("api/documentos")]
public class DocumentosController : ControllerBase
{
    private readonly ServicioDocumentos _documentos;

    public DocumentosController(ServicioDocumentos documentos)
    {
        _documentos = documentos;
    }

    // El atributo ya validó la sesión y dejó aquí al usuario que hace la petición.
    private Usuario UsuarioActual => (Usuario)HttpContext.Items["Usuario"]!;

    // POST /api/documentos
    // El archivo llega como form-data en el campo "archivo".
    [HttpPost]
    public async Task<IActionResult> Subir(IFormFile? archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest(new { mensaje = "Debes adjuntar un archivo." });

        Resultado r = await _documentos.SubirAsync(
            UsuarioActual, archivo.FileName, archivo.ContentType, archivo.OpenReadStream(), archivo.Length);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // GET /api/documentos
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        List<Documento> documentos = await _documentos.ListarAsync(UsuarioActual);

        var lista = documentos.Select(d => new
        {
            id = d.Id,
            nombre = d.Nombre,
            tipo = d.Tipo,
            tamano = d.Tamano,
            fecha = d.Fecha,
            usuarioId = d.UsuarioId
        });

        return Ok(lista);
    }

    // GET /api/documentos/{id}/descargar
    [HttpGet("{id}/descargar")]
    public async Task<IActionResult> Descargar(int id)
    {
        Documento? documento = await _documentos.ObtenerParaDescargaAsync(UsuarioActual, id);

        if (documento == null)
            return NotFound(new { mensaje = "Documento no encontrado." });

        // El registro puede existir pero el archivo ya no estar en disco: se responde igual.
        string ruta = _documentos.RutaDelArchivo(documento);
        if (!System.IO.File.Exists(ruta))
            return NotFound(new { mensaje = "Documento no encontrado." });

        // Se devuelve con el nombre y el tipo originales, no con el GUID.
        return PhysicalFile(ruta, documento.Tipo, documento.Nombre);
    }

    // DELETE /api/documentos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        Resultado r = await _documentos.EliminarAsync(UsuarioActual, id);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }
}
