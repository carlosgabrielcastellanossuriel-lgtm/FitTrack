using FitTrack.Api.Seguridad;
using FitTrack.Core.Reportes;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Resumenes;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Requiere sesión válida en todas las operaciones de este controlador.
[RequiereSesion]
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly ServicioReportes _reportes;
    private readonly ServicioResumen _resumen;

    public ReportesController(ServicioReportes reportes, ServicioResumen resumen)
    {
        _reportes = reportes;
        _resumen = resumen;
    }

    // El atributo ya validó la sesión y dejó aquí al usuario que hace la petición.
    private Usuario UsuarioActual => (Usuario)HttpContext.Items["Usuario"]!;

    // GET /api/reportes/solicitudes-permiso?desde=2026-11-01&hasta=2026-11-30   (RF-REP-01)
    [HttpGet("solicitudes-permiso")]
    public async Task<IActionResult> SolicitudesPorEstado(DateOnly? desde, DateOnly? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            return BadRequest(new { mensaje = "La fecha desde no puede ser mayor que hasta." });

        List<SolicitudesPorEstado> filas = await _reportes.SolicitudesPorEstadoAsync(UsuarioActual, desde, hasta);

        var lista = filas.Select(f => new
        {
            estado = f.Estado.ToString(),
            total = f.Total
        });

        return Ok(lista);
    }

    // GET /api/reportes/dias-por-estado?desde=2026-11-01&hasta=2026-11-30   (RF-REP-02)
    [HttpGet("dias-por-estado")]
    public async Task<IActionResult> DiasPorEstado(DateOnly? desde, DateOnly? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            return BadRequest(new { mensaje = "La fecha desde no puede ser mayor que hasta." });

        List<DiasPorEstado> filas = await _resumen.DiasPorEstadoAsync(UsuarioActual, desde, hasta);

        var lista = filas.Select(f => new
        {
            estado = f.Estado.ToString(),
            dias = f.Dias,
            promedioCalorias = f.PromedioCalorias
        });

        return Ok(lista);
    }
}
