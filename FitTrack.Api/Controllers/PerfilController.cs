using FitTrack.Api.Seguridad;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Perfiles;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Esta única línea exige sesión válida en TODAS las operaciones de este controlador.
// Cada usuario solo ve y edita SU propio perfil: el usuario sale de la sesión, no del JSON.
[RequiereSesion]
[ApiController]
[Route("api/perfil")]
public class PerfilController : ControllerBase
{
    private readonly ServicioPerfil _perfil;

    public PerfilController(ServicioPerfil perfil)
    {
        _perfil = perfil;
    }

    // El atributo ya validó la sesión y dejó aquí al usuario que hace la petición.
    private Usuario UsuarioActual => (Usuario)HttpContext.Items["Usuario"]!;

    // GET /api/perfil
    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        PerfilUsuario? perfil = await _perfil.ObtenerAsync(UsuarioActual.Id);

        if (perfil == null)
            return NotFound(new { mensaje = "Aún no has creado tu perfil." });

        return Ok(ArmarRespuesta(perfil));
    }

    // PUT /api/perfil   (crea el perfil la primera vez y lo actualiza después)
    [HttpPut]
    public async Task<IActionResult> Guardar(PerfilDto dto)
    {
        // Los enums llegan como texto ("Masculino") y se convierten aquí.
        // IsDefined evita que pasen números inventados como "7".
        if (!Enum.TryParse<Sexo>(dto.Sexo, true, out Sexo sexo) || !Enum.IsDefined(sexo))
            return BadRequest(new { mensaje = "Sexo no válido. Usa Masculino o Femenino." });

        if (!Enum.TryParse<NivelActividad>(dto.NivelActividad, true, out NivelActividad actividad) || !Enum.IsDefined(actividad))
            return BadRequest(new { mensaje = "Nivel de actividad no válido. Usa Sedentario, Ligero, Moderado o Alto." });

        if (!Enum.TryParse<ObjetivoNutricional>(dto.Objetivo, true, out ObjetivoNutricional objetivo) || !Enum.IsDefined(objetivo))
            return BadRequest(new { mensaje = "Objetivo no válido. Usa Deficit, Mantenimiento o Superavit." });

        DatosPerfil datos = new DatosPerfil();
        datos.Edad = dto.Edad;
        datos.Peso = dto.Peso;
        datos.Altura = dto.Altura;
        datos.Sexo = sexo;
        datos.NivelActividad = actividad;
        datos.Objetivo = objetivo;

        // El servicio valida los rangos, calcula las metas y guarda.
        Resultado r = await _perfil.GuardarAsync(UsuarioActual.Id, datos);

        if (!r.Exito)
            return BadRequest(new { mensaje = r.Mensaje });

        // Se devuelve el perfil ya guardado, con las metas calculadas.
        PerfilUsuario? perfil = await _perfil.ObtenerAsync(UsuarioActual.Id);
        return Ok(ArmarRespuesta(perfil!));
    }

    // Arma lo que se le devuelve al cliente. No se incluyen Id ni UsuarioId.
    private static object ArmarRespuesta(PerfilUsuario p)
    {
        return new
        {
            p.Edad,
            PesoLibras = p.Peso,
            AlturaPulgadas = p.Altura,
            Sexo = p.Sexo.ToString(),
            NivelActividad = p.NivelActividad.ToString(),
            Objetivo = p.Objetivo.ToString(),
            Metas = new
            {
                Calorias = p.CaloriasMeta,
                ProteinaGramos = p.ProteinaMeta,
                CarbohidratosGramos = p.CarbohidratosMeta,
                GrasasGramos = p.GrasasMeta
            }
        };
    }
}

// El "molde" del JSON que llega en el cuerpo de la petición.
public class PerfilDto
{
    public int Edad { get; set; }
    public double Peso { get; set; }                                    // en libras
    public double Altura { get; set; }                                  // en pulgadas totales
    public string Sexo { get; set; } = string.Empty;                    // Masculino | Femenino
    public string NivelActividad { get; set; } = string.Empty;          // Sedentario | Ligero | Moderado | Alto
    public string Objetivo { get; set; } = string.Empty;                // Deficit | Mantenimiento | Superavit
}
