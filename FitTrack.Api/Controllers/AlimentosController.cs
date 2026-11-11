using FitTrack.Api.Seguridad;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Alimentos;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Api.Controllers;

// Todas las operaciones exigen sesión; el rol de Administrador se exige solo donde hace falta.
[RequiereSesion]
[ApiController]
[Route("api/alimentos")]
public class AlimentosController : ControllerBase
{
    private readonly ServicioAlimento _alimentos;
    private readonly ServicioUsda _usda;

    public AlimentosController(ServicioAlimento alimentos, ServicioUsda usda)
    {
        _alimentos = alimentos;
        _usda = usda;
    }

    // El atributo ya validó la sesión y dejó aquí al usuario que hace la petición.
    private Usuario UsuarioActual => (Usuario)HttpContext.Items["Usuario"]!;

    // GET /api/alimentos
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        List<Alimento> alimentos = await _alimentos.ListarAsync(UsuarioActual.Id);

        var lista = alimentos.Select(a => new
        {
            id = a.Id,
            nombre = a.Nombre,
            caloriasPor100g = a.CaloriasPor100g,
            proteinaPor100g = a.ProteinaPor100g,
            carbohidratosPor100g = a.CarbohidratosPor100g,
            grasasPor100g = a.GrasasPor100g
        });

        return Ok(lista);
    }

    // POST /api/alimentos
    // Crear un alimento básico (lo ven todos) es cosa solo del Administrador.
    [RequiereRol(Rol.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Crear(AlimentoDto dto)
    {
        Resultado r = await _alimentos.CrearAsync(
            dto.Nombre, dto.CaloriasPor100g, dto.ProteinaPor100g,
            dto.CarbohidratosPor100g, dto.GrasasPor100g);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // POST /api/alimentos/personalizado
    // Cualquier usuario con sesión crea un alimento propio: solo él lo verá.
    [HttpPost("personalizado")]
    public async Task<IActionResult> CrearPersonalizado(AlimentoDto dto)
    {
        Resultado r = await _alimentos.CrearAsync(
            dto.Nombre, dto.CaloriasPor100g, dto.ProteinaPor100g,
            dto.CarbohidratosPor100g, dto.GrasasPor100g,
            UsuarioActual.Id);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // POST /api/alimentos/importar
    // Busca el alimento en USDA y lo guarda como alimento personalizado del usuario.
    [HttpPost("importar")]
    public async Task<IActionResult> Importar(ImportarAlimentoDto dto)
    {
        Resultado r = await _usda.ImportarAsync(UsuarioActual, dto.Nombre);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // PUT /api/alimentos/{id}
    // Sin [RequiereRol]: la clase ya exige sesión y la regla de rol vive en el servicio.
    // Un Estándar solo modifica los suyos; un Administrador solo los básicos.
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, AlimentoDto dto)
    {
        Resultado r = await _alimentos.EditarAsync(
            UsuarioActual, id, dto.Nombre, dto.CaloriasPor100g, dto.ProteinaPor100g,
            dto.CarbohidratosPor100g, dto.GrasasPor100g);

        return r.Exito ? Ok(new { mensaje = r.Mensaje }) : BadRequest(new { mensaje = r.Mensaje });
    }

    // DELETE /api/alimentos/{id}
    // Misma regla de permiso que el PUT: la decide el servicio.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Borrar(int id)
    {
        Resultado r = await _alimentos.BorrarAsync(UsuarioActual, id);

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

// El "molde" del JSON para importar un alimento desde USDA.
public class ImportarAlimentoDto
{
    public string Nombre { get; set; } = string.Empty;
}
