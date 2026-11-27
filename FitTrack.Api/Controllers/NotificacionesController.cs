using FitTrack.Api.Seguridad;
using FitTrack.Core.Notificaciones;
using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

[RequiereSesion]
[ApiController]
[Route("api/notificaciones")]
public class NotificacionesController : ControllerBase
{
    private readonly ServicioNotificaciones _notificaciones;

    public NotificacionesController(ServicioNotificaciones notificaciones)
    {
        _notificaciones = notificaciones;
    }

    private Usuario UsuarioActual => (Usuario)HttpContext.Items["Usuario"]!;

    // GET /api/notificaciones?soloNoLeidas=false   (RF-NOT-04)
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool soloNoLeidas = false)
    {
        List<Notificacion> notificaciones = await _notificaciones.ListarAsync(UsuarioActual.Id, soloNoLeidas);

        var lista = notificaciones.Select(n => new
        {
            n.Id,
            n.Titulo,
            n.Mensaje,
            n.Leida,
            n.FechaCreacion
        });

        return Ok(lista);
    }

    // POST /api/notificaciones/{id}/leer   (RF-NOT-05)
    [HttpPost("{id}/leer")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        Resultado r = await _notificaciones.MarcarLeidaAsync(UsuarioActual.Id, id);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // GET /api/notificaciones/preferencias   (RF-NOT-07)
    [HttpGet("preferencias")]
    public async Task<IActionResult> ObtenerPreferencias()
    {
        bool correo = await _notificaciones.ObtenerPreferenciaAsync(UsuarioActual.Id);
        return Ok(new { correo });
    }

    // PUT /api/notificaciones/preferencias   (RF-NOT-07)
    [HttpPut("preferencias")]
    public async Task<IActionResult> CambiarPreferencias(PreferenciasNotificacionDto dto)
    {
        Resultado r = await _notificaciones.CambiarPreferenciaAsync(UsuarioActual.Id, dto.Correo);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }
}

public class PreferenciasNotificacionDto
{
    public bool Correo { get; set; }
}
