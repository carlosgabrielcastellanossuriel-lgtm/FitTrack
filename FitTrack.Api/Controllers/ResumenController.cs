using FitTrack.Api.Seguridad;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Resumenes;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Esta única línea exige sesión válida en TODAS las operaciones de este controlador.
// Cada usuario registra comidas solo en SU propio resumen: el usuario sale de la sesión.
[RequiereSesion]
[ApiController]
[Route("api/resumen")]
public class ResumenController : ControllerBase
{
    private readonly ServicioResumen _resumen;

    public ResumenController(ServicioResumen resumen)
    {
        _resumen = resumen;
    }

    // El atributo ya validó la sesión y dejó aquí al usuario que hace la petición.
    private Usuario UsuarioActual => (Usuario)HttpContext.Items["Usuario"]!;

    // POST /api/resumen/comida
    [HttpPost("comida")]
    public async Task<IActionResult> RegistrarComida(ComidaDto dto)
    {
        Resultado r = await _resumen.RegistrarComidaAsync(UsuarioActual.Id, dto.AlimentoId, dto.Gramos);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // POST /api/resumen/cerrar
    [HttpPost("cerrar")]
    public async Task<IActionResult> CerrarDia()
    {
        Resultado r = await _resumen.CerrarDiaAsync(UsuarioActual.Id);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // GET /api/resumen/historial   (requiere el permiso VerEstadisticas)
    [HttpGet("historial")]
    public async Task<IActionResult> Historial()
    {
        List<ResumenDiario>? resumenes = await _resumen.HistorialAsync(UsuarioActual);

        if (resumenes == null)
            return StatusCode(403, new { mensaje = "No tienes permiso." });

        // Solo se copian los campos que se muestran al cliente.
        var lista = resumenes.Select(r => new
        {
            fecha = r.Fecha,
            calorias = r.CaloriasAcumuladas,
            proteinas = r.ProteinasAcumuladas,
            carbohidratos = r.CarbohidratosAcumulados,
            grasas = r.GrasasAcumuladas,
            estado = r.EstadoActual.ToString()
        });

        return Ok(lista);
    }
}

// El "molde" del JSON que llega en el cuerpo de la petición.
public class ComidaDto
{
    public int AlimentoId { get; set; }
    public double Gramos { get; set; }
}
