using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;
using FitTrack.Api.Seguridad;

namespace FitTrack.Api.Controllers;

// [ApiController] activa reglas automáticas: si falta un campo del JSON,
// ASP.NET responde 400 solo, sin que se caiga nada.
[ApiController]
[Route("api/auth")]   // todas las rutas de aquí empiezan con /api/auth
public class AutenticacionController : ControllerBase
{
    private readonly ServicioRegistro _registro;
    private readonly ServicioSesion _sesion;

    public AutenticacionController(ServicioRegistro registro, ServicioSesion sesion)
    {
        _registro = registro;
        _sesion = sesion;
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

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> IniciarSesion(LoginDto dto)
    {
        Resultado r = await _sesion.IniciarSesionAsync(dto.Correo, dto.Clave);

        if (!r.Exito)
            return Unauthorized(new { mensaje = r.Mensaje });   // 401

        return Ok(new { mensaje = r.Mensaje, token = r.Token });
    }

    // GET /api/auth/yo   -> quién soy (RF-CA-07)
    [RequiereSesion]
    [HttpGet("yo")]
    public IActionResult QuienSoy()
    {
        // El atributo ya validó la sesión y dejó el usuario aquí.
        Usuario usuario = (Usuario)HttpContext.Items["Usuario"]!;

        // Solo se devuelven datos seguros: nunca el hash. (El rol se agrega en la Parte 6.)
        return Ok(new { usuario.Id, usuario.Correo, usuario.Activo });
    }

    // POST /api/auth/logout   (RF-CA-18)
    [RequiereSesion]
    [HttpPost("logout")]
    public async Task<IActionResult> CerrarSesion()
    {
        string token = (string)HttpContext.Items["Token"]!;
        await _sesion.CerrarSesionAsync(token);
        return Ok(new { mensaje = "Sesión cerrada." });
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

public class LoginDto
{
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
}