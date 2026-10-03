using FitTrack.Api.Seguridad;
using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

[ApiController]
[Route("api/contrasena")]
public class ContrasenasController : ControllerBase
{
    private readonly ServicioContrasena _contrasena;

    public ContrasenasController(ServicioContrasena contrasena)
    {
        _contrasena = contrasena;
    }

    // POST /api/contrasena/recuperar
    [HttpPost("recuperar")]
    public async Task<IActionResult> Recuperar(RecuperarDto dto)
    {
        // Un correo mal formado sí se rechaza (RD-07); eso no revela qué correos existen.
        if (!Validaciones.CorreoValido(dto.Correo))
            return BadRequest(new { mensaje = "El correo no tiene un formato válido." });

        await _contrasena.SolicitarRecuperacionAsync(dto.Correo);

        // SIEMPRE la misma respuesta, exista el correo o no (RF-CA-09).
        return Ok(new { mensaje = "Si el correo está registrado, enviaremos un código de recuperación." });
    }

    // POST /api/contrasena/restablecer
    [HttpPost("restablecer")]
    public async Task<IActionResult> Restablecer(RestablecerDto dto)
    {
        Resultado r = await _contrasena.RestablecerAsync(dto.Codigo, dto.ClaveNueva);

        if (!r.Exito)
            return BadRequest(new { mensaje = r.Mensaje });

        return Ok(new { mensaje = r.Mensaje });
    }

    // POST /api/contrasena/cambiar   (requiere sesión)
    [RequiereSesion]
    [HttpPost("cambiar")]
    public async Task<IActionResult> Cambiar(CambiarDto dto)
    {
        // El atributo ya validó la sesión y dejó aquí al usuario.
        Usuario usuario = (Usuario)HttpContext.Items["Usuario"]!;

        Resultado r = await _contrasena.CambiarAsync(usuario, dto.ClaveActual, dto.ClaveNueva);

        if (!r.Exito)
            return BadRequest(new { mensaje = r.Mensaje });

        return Ok(new { mensaje = r.Mensaje });
    }
}

public class RecuperarDto
{
    public string Correo { get; set; } = string.Empty;
}

public class RestablecerDto
{
    public string Codigo { get; set; } = string.Empty;
    public string ClaveNueva { get; set; } = string.Empty;
}

public class CambiarDto
{
    public string ClaveActual { get; set; } = string.Empty;
    public string ClaveNueva { get; set; } = string.Empty;
}