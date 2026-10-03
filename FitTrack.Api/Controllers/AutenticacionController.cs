using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// [ApiController] activa reglas automáticas: si falta un campo del JSON,
// ASP.NET responde 400 solo, sin que se caiga nada.
[ApiController]
[Route("api/auth")]   // todas las rutas de aquí empiezan con /api/auth
public class AutenticacionController : ControllerBase
{
    private readonly ServicioRegistro _registro;

    public AutenticacionController(ServicioRegistro registro)
    {
        _registro = registro;
    }

    // POST /api/auth/registro
    [HttpPost("registro")]
    public async Task<IActionResult> Registrar(RegistroDto dto)
    {
        Resultado r = await _registro.RegistrarAsync(dto.Correo, dto.Clave);

        if (!r.Exito)
            return BadRequest(new { mensaje = r.Mensaje });   // 400

        return Ok(new { mensaje = r.Mensaje });               // 200
    }

    // GET /api/auth/activar?token=XXXX   (es GET para que funcione abriendo el enlace en el navegador)
    [HttpGet("activar")]
    public async Task<IActionResult> Activar(string token)
    {
        Resultado r = await _registro.ActivarAsync(token);

        if (!r.Exito)
            return BadRequest(new { mensaje = r.Mensaje });

        return Ok(new { mensaje = r.Mensaje });
    }

    // POST /api/auth/reenviar-activacion
    [HttpPost("reenviar-activacion")]
    public async Task<IActionResult> ReenviarActivacion(ReenvioDto dto)
    {
        // Un correo mal formado sí se rechaza (RD-07); eso no revela qué correos existen.
        if (!Validaciones.CorreoValido(dto.Correo))
            return BadRequest(new { mensaje = "El correo no tiene un formato válido." });

        await _registro.ReenviarActivacionAsync(dto.Correo);

        // SIEMPRE la misma respuesta, exista el correo o no (RF-CA-17).
        return Ok(new { mensaje = "Si el correo está registrado y sin activar, enviaremos un nuevo enlace." });
    }
}

// Los "moldes" del JSON que llega en el cuerpo de la petición.
public class RegistroDto
{
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
}

public class ReenvioDto
{
    public string Correo { get; set; } = string.Empty;
}