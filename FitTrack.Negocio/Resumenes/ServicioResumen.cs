using FitTrack.Core.Datos;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Alimentos;
using FitTrack.Negocio.Perfiles;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Negocio.Resumenes;

public class ServicioResumen
{
    private readonly ContextoBD _db;
    private readonly ServicioPermisos _permisos;

    public ServicioResumen(ContextoBD db, ServicioPermisos permisos)
    {
        _db = db;
        _permisos = permisos;
    }

    // Registra una comida de un alimento (en gramos) en el resumen del día.
    public async Task<Resultado> RegistrarComidaAsync(int usuarioId, int alimentoId, double gramos)
    {
        // 1. Los gramos deben ser un número positivo y válido.
        if (!(gramos > 0))
            return Resultado.Error("Los gramos deben ser mayores que cero.");

        // 2. El alimento debe existir y ser visible para el usuario.
        // Un usuario no puede usar alimentos privados de otro: solo los básicos (sin creador) o los suyos.
        Alimento? alimento = await _db.Set<Alimento>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == alimentoId && (a.UsuarioCreadorId == null || a.UsuarioCreadorId == usuarioId));

        if (alimento == null)
            return Resultado.Error("Alimento no encontrado.");

        // 3. El usuario debe tener perfil (de ahí salen sus metas).
        PerfilUsuario? perfil = await _db.Set<PerfilUsuario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

        if (perfil == null)
            return Resultado.Error("Primero crea tu perfil.");

        // 4. Se busca el resumen de hoy; si no existe, se crea.
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

        ResumenDiario? resumen = await _db.Set<ResumenDiario>()
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.Fecha == hoy);

        if (resumen == null)
        {
            resumen = new ResumenDiario();
            resumen.UsuarioId = usuarioId;
            resumen.Fecha = hoy;
            _db.Set<ResumenDiario>().Add(resumen);
        }

        // 5. Se calculan las calorías y macros de la porción: valor por 100 g * gramos / 100.
        double calorias = alimento.CaloriasPor100g * gramos / 100;
        double proteinas = alimento.ProteinaPor100g * gramos / 100;
        double carbohidratos = alimento.CarbohidratosPor100g * gramos / 100;
        double grasas = alimento.GrasasPor100g * gramos / 100;

        // 6. El resumen suma la comida y recalcula su estado.
        if (!resumen.RegistrarComida(calorias, proteinas, carbohidratos, grasas, perfil))
            return Resultado.Error("El día está cerrado.");

        // 7. Se guarda el detalle de la comida. Se usa la propiedad ResumenDiario (no el Id)
        // para que funcione aunque el resumen se acabe de crear y aún no tenga Id.
        RegistroComida registro = new RegistroComida();
        registro.ResumenDiario = resumen;
        registro.AlimentoId = alimento.Id;
        registro.Gramos = gramos;
        registro.Calorias = calorias;
        registro.Proteinas = proteinas;
        registro.Carbohidratos = carbohidratos;
        registro.Grasas = grasas;
        _db.Set<RegistroComida>().Add(registro);

        // 8. Se guarda todo junto: el registro y el resumen van en el mismo SaveChangesAsync,
        // así nunca queda una cosa sin la otra.
        await _db.SaveChangesAsync();

        return Resultado.Ok($"Comida registrada. Estado del día: {resumen.EstadoActual}.");
    }

    // Cierra el resumen del día de hoy. Un día ya cerrado no se puede cerrar otra vez.
    public async Task<Resultado> CerrarDiaAsync(int usuarioId)
    {
        // 1. Se busca el resumen de hoy; si no hay comidas registradas, no existe.
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

        ResumenDiario? resumen = await _db.Set<ResumenDiario>()
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.Fecha == hoy);

        if (resumen == null)
            return Resultado.Error("Hoy no has registrado comidas.");

        // 2. El cambio de estado lo decide la entidad (con las reglas de TransicionesResumen).
        if (!resumen.CambiarEstado(EstadoResumen.Cerrado))
            return Resultado.Error("El día no se puede cerrar en su estado actual.");

        // 3. Se guarda el nuevo estado.
        await _db.SaveChangesAsync();

        return Resultado.Ok("Día cerrado.");
    }

    // Devuelve el historial de resúmenes del usuario. Solo con el permiso VerEstadisticas.
    public async Task<List<ResumenDiario>?> HistorialAsync(Usuario usuario)
    {
        if (!await _permisos.TieneAsync(usuario.Id, Permiso.VerEstadisticas)) return null;

        // AsNoTracking: solo se lee, EF no necesita vigilar estos objetos.
        return await _db.Set<ResumenDiario>()
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuario.Id)
            .OrderBy(r => r.Fecha)
            .ToListAsync();
    }

    // Devuelve el resumen de hoy, el perfil y las comidas registradas, todo junto.
    // Si el usuario no tiene resumen de hoy o no tiene perfil, devuelve null.
    public async Task<DatosDia?> ObtenerHoyAsync(int usuarioId)
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

        // 1. El resumen del día de hoy (solo lectura).
        ResumenDiario? resumen = await _db.Set<ResumenDiario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.Fecha == hoy);

        if (resumen == null)
            return null;

        // 2. El perfil del usuario (de ahí salen sus metas).
        PerfilUsuario? perfil = await _db.Set<PerfilUsuario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

        if (perfil == null)
            return null;

        // 3. Las comidas de ese resumen, con su alimento, en el orden en que se registraron.
        List<RegistroComida> comidas = await _db.Set<RegistroComida>()
            .AsNoTracking()
            .Include(c => c.Alimento)
            .Where(c => c.ResumenDiarioId == resumen.Id)
            .OrderBy(c => c.Id)
            .ToListAsync();

        DatosDia datos = new DatosDia();
        datos.Resumen = resumen;
        datos.Perfil = perfil;
        datos.Comidas = comidas;

        return datos;
    }
}

// Solo un paquete para devolver los tres datos juntos.
public class DatosDia
{
    public ResumenDiario Resumen { get; set; } = new ResumenDiario();
    public PerfilUsuario Perfil { get; set; } = new PerfilUsuario();
    public List<RegistroComida> Comidas { get; set; } = new List<RegistroComida>();
}
