using FitTrack.Api.Seguridad;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Requiere sesión válida en todas las operaciones de este controlador.
[RequiereSesion]
[ApiController]
[Route("api/permisos")]
public class PermisosController : ControllerBase
{
    private readonly ServicioPermisos _permisos;

    public PermisosController(ServicioPermisos permisos)
    {
        _permisos = permisos;
    }

    // El atributo ya validó la sesión y dejó aquí al usuario que hace la petición.
    private Usuario UsuarioActual => (Usuario)HttpContext.Items["Usuario"]!;

    // POST /api/permisos   cuerpo: { "permiso": "ExportarDatos" }   (RF-GP-01)
    [HttpPost]
    public async Task<IActionResult> Crear(CrearPermisoDto dto)
    {
        // Convierte el texto al enum. IsDefined evita valores inventados.
        if (!Enum.TryParse<Permiso>(dto.Permiso, true, out Permiso permiso) || !Enum.IsDefined(permiso))
            return BadRequest(new { mensaje = "Permiso no válido. Usa ExportarDatos o VerEstadisticas." });

        Resultado r = await _permisos.CrearAsync(UsuarioActual, permiso);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // GET /api/permisos/pendientes   (RF-GP-02)
    [HttpGet("pendientes")]
    public async Task<IActionResult> Pendientes()
    {
        List<SolicitudPermiso> solicitudes = await _permisos.ListarPendientesAsync(UsuarioActual);

        var lista = solicitudes.Select(s => new
        {
            s.Id,
            s.SolicitanteId,
            Permiso = s.Permiso.ToString(),
            Estado = s.Estado.ToString(),
            s.FechaCreacion
        });

        return Ok(lista);
    }

    // POST /api/permisos/{id}/aprobar   solo Administrador   (RF-GP-03, 05, 06, 07)
    [HttpPost("{id}/aprobar")]
    [RequiereRol(Rol.Administrador)]
    public async Task<IActionResult> Aprobar(int id)
    {
        Resultado r = await _permisos.AprobarAsync(id, UsuarioActual);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // POST /api/permisos/{id}/rechazar   solo Administrador   cuerpo: { "motivo": "..." }   (RF-GP-04)
    [HttpPost("{id}/rechazar")]
    [RequiereRol(Rol.Administrador)]
    public async Task<IActionResult> Rechazar(int id, RechazarPermisoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo))
            return BadRequest(new { mensaje = "Debes indicar un motivo para rechazar." });

        Resultado r = await _permisos.RechazarAsync(id, UsuarioActual, dto.Motivo);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // GET /api/permisos/accion-protegida   requiere sesión + permiso ExportarDatos
    [HttpGet("accion-protegida")]
    public async Task<IActionResult> AccionProtegida()
    {
        bool tiene = await _permisos.TieneAsync(UsuarioActual.Id, Permiso.ExportarDatos);

        if (!tiene)
            return StatusCode(403, new { mensaje = "No tienes permiso." });

        return Ok(new { mensaje = "Acción ejecutada." });
    }
}

// DTOs (moldes del JSON que llega en el cuerpo).
public class CrearPermisoDto
{
    public string Permiso { get; set; } = string.Empty;  // ExportarDatos | VerEstadisticas
}

public class RechazarPermisoDto
{
    public string Motivo { get; set; } = string.Empty;
}
