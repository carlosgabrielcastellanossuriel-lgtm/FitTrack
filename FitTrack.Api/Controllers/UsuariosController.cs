using FitTrack.Api.Seguridad;
using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Esta única línea es la exigencia de rol de TODAS las operaciones de este controlador
// (RF-CA-05). Si más adelante se agrega otra operación aquí, queda protegida sola.
[RequiereRol(Rol.Administrador)]
[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly ServicioAdministracion _admin;

    public UsuariosController(ServicioAdministracion admin)
    {
        _admin = admin;
    }

    // El atributo ya validó la sesión y dejó aquí al Administrador que hace la petición.
    private Usuario Administrador => (Usuario)HttpContext.Items["Usuario"]!;

    // GET /api/usuarios   (RF-CA-21)
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        List<Usuario> usuarios = await _admin.ListarAsync();

        // Solo se copian campos seguros: nunca el hash ni tokens.
        var lista = usuarios.Select(u => new
        {
            u.Id,
            u.Correo,
            Rol = u.Rol.ToString(),
            u.Activo
        });

        return Ok(lista);
    }

    // PUT /api/usuarios/5/rol   cuerpo: { "rol": "Administrador" }   (RF-CA-08)
    [HttpPut("{id}/rol")]
    public async Task<IActionResult> CambiarRol(int id, CambiarRolDto dto)
    {
        // IsDefined evita que pasen números inventados como "7".
        if (!Enum.TryParse<Rol>(dto.Rol, true, out Rol rol) || !Enum.IsDefined(rol))
            return BadRequest(new { mensaje = "Rol no válido. Usa Estandar o Administrador." });

        Resultado r = await _admin.CambiarRolAsync(id, rol, Administrador);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // POST /api/usuarios/5/desactivar   (RF-CA-20)
    [HttpPost("{id}/desactivar")]
    public async Task<IActionResult> Desactivar(int id)
    {
        Resultado r = await _admin.CambiarEstadoAsync(id, false, Administrador);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // POST /api/usuarios/5/reactivar   (RF-CA-20)
    [HttpPost("{id}/reactivar")]
    public async Task<IActionResult> Reactivar(int id)
    {
        Resultado r = await _admin.CambiarEstadoAsync(id, true, Administrador);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // POST /api/usuarios/5/forzar-restablecimiento   (RF-CA-13)
    [HttpPost("{id}/forzar-restablecimiento")]
    public async Task<IActionResult> ForzarRestablecimiento(int id)
    {
        Resultado r = await _admin.ForzarRestablecimientoAsync(id);
        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }
}

public class CambiarRolDto
{
    public string Rol { get; set; } = string.Empty;
}