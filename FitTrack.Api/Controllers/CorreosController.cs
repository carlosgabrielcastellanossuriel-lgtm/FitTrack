using FitTrack.Api.Seguridad;
using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Api.Controllers;

[RequiereRol(Rol.Administrador)]
[ApiController]
[Route("api/correos")]
public class CorreosController : ControllerBase
{
    private readonly ContextoBD _db;

    public CorreosController(ContextoBD db)
    {
        _db = db;
    }

    // GET /api/correos   (RF-NOT-11)
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _db.CorreosEnCola
            .AsNoTracking()
            .OrderByDescending(c => c.FechaCreacion)
            .ThenByDescending(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.Destinatario,
                c.Asunto,
                Estado = c.Estado.ToString(),
                c.Intentos,
                c.FechaCreacion,
                c.FechaEnvio,
                c.FechaUltimoIntento,
                c.UltimoError
            })
            .ToListAsync();

        return Ok(lista);
    }
}
