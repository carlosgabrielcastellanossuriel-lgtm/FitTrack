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

    // GET /api/resumen/hoy
    [HttpGet("hoy")]
    public async Task<IActionResult> Hoy()
    {
        DatosDia? datos = await _resumen.ObtenerHoyAsync(UsuarioActual.Id);

        if (datos == null)
            return NotFound(new { mensaje = "Hoy no has registrado comidas." });

        // Lo que queda de cada meta. Si sale negativo significa que se pasó de la meta.
        var restante = new
        {
            calorias = Math.Round(datos.Perfil.CaloriasMeta - datos.Resumen.CaloriasAcumuladas, 1),
            proteinas = Math.Round(datos.Perfil.ProteinaMeta - datos.Resumen.ProteinasAcumuladas, 1),
            carbohidratos = Math.Round(datos.Perfil.CarbohidratosMeta - datos.Resumen.CarbohidratosAcumulados, 1),
            grasas = Math.Round(datos.Perfil.GrasasMeta - datos.Resumen.GrasasAcumuladas, 1)
        };

        // Solo se copian los campos que se muestran al cliente.
        var comidas = datos.Comidas.Select(c => new
        {
            alimento = c.Alimento != null ? c.Alimento.Nombre : "",
            gramos = c.Gramos,
            calorias = c.Calorias,
            proteinas = c.Proteinas,
            carbohidratos = c.Carbohidratos,
            grasas = c.Grasas
        });

        return Ok(new
        {
            fecha = datos.Resumen.Fecha,
            estado = datos.Resumen.EstadoActual.ToString(),
            consumido = new
            {
                calorias = datos.Resumen.CaloriasAcumuladas,
                proteinas = datos.Resumen.ProteinasAcumuladas,
                carbohidratos = datos.Resumen.CarbohidratosAcumulados,
                grasas = datos.Resumen.GrasasAcumuladas
            },
            metas = new
            {
                calorias = datos.Perfil.CaloriasMeta,
                proteinas = datos.Perfil.ProteinaMeta,
                carbohidratos = datos.Perfil.CarbohidratosMeta,
                grasas = datos.Perfil.GrasasMeta
            },
            restante,
            comidas
        });
    }

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
