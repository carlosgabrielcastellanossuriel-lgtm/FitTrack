using FitTrack.Api.Seguridad;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Alimentos;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Solo el Administrador puede administrar los alimentos.
[RequiereRol(Rol.Administrador)]
[ApiController]
[Route("api/alimentos")]
public class AlimentosController : ControllerBase
{
    private readonly ServicioAlimento _alimentos;

    public AlimentosController(ServicioAlimento alimentos)
    {
        _alimentos = alimentos;
    }

    // POST /api/alimentos
    [HttpPost]
    public async Task<IActionResult> Crear(AlimentoDto dto)
    {
        Resultado r = await _alimentos.CrearAsync(
            dto.Nombre, dto.CaloriasPor100g, dto.ProteinaPor100g,
            dto.CarbohidratosPor100g, dto.GrasasPor100g);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }
}

// El "molde" del JSON que llega en el cuerpo de la petición.
public class AlimentoDto
{
    public string Nombre { get; set; } = string.Empty;
    public double CaloriasPor100g { get; set; }
    public double ProteinaPor100g { get; set; }
    public double CarbohidratosPor100g { get; set; }
    public double GrasasPor100g { get; set; }
}
