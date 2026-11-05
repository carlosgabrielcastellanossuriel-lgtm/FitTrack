using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Resumenes;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Negocio.Alimentos;

public class ServicioAlimento
{
    private readonly ContextoBD _db;

    public ServicioAlimento(ContextoBD db)
    {
        _db = db;
    }

    // Crea un alimento nuevo después de validar sus datos.
    public async Task<Resultado> CrearAsync(string nombre, double calorias, double proteina, double carbohidratos, double grasas, int? usuarioCreadorId = null)
    {
        // 1. Validaciones: si algo falla, se rechaza con un mensaje claro y no se guarda nada.
        string? error = Validar(nombre, calorias, proteina, carbohidratos, grasas);
        if (error != null)
            return Resultado.Error(error);

        // 2. Se guarda el alimento nuevo.
        Alimento alimento = new Alimento();
        alimento.Nombre = nombre;
        alimento.CaloriasPor100g = calorias;
        alimento.ProteinaPor100g = proteina;
        alimento.CarbohidratosPor100g = carbohidratos;
        alimento.GrasasPor100g = grasas;
        alimento.UsuarioCreadorId = usuarioCreadorId;

        _db.Set<Alimento>().Add(alimento);
        await _db.SaveChangesAsync();

        return Resultado.Ok("Alimento guardado.");
    }

    // Edita un alimento existente. Cada uno solo puede tocar los que le corresponden.
    public async Task<Resultado> EditarAsync(Usuario usuario, int alimentoId, string nombre, double calorias, double proteina, double carbohidratos, double grasas)
    {
        // 1. Los datos nuevos deben ser válidos.
        string? error = Validar(nombre, calorias, proteina, carbohidratos, grasas);
        if (error != null)
            return Resultado.Error(error);

        // 2. Se busca el alimento. Aquí SIN AsNoTracking, porque lo vamos a modificar.
        Alimento? alimento = await _db.Set<Alimento>().FindAsync(alimentoId);

        // 3. Las mismas reglas de permiso que al borrar.
        Resultado? permiso = VerificarPermiso(usuario, alimento);
        if (permiso != null)
            return permiso;

        // 4. Se copian los valores nuevos y se guarda.
        alimento!.Nombre = nombre;
        alimento.CaloriasPor100g = calorias;
        alimento.ProteinaPor100g = proteina;
        alimento.CarbohidratosPor100g = carbohidratos;
        alimento.GrasasPor100g = grasas;

        await _db.SaveChangesAsync();

        return Resultado.Ok("Alimento actualizado.");
    }

    // Borra un alimento existente. Solo si el usuario tiene permiso y nadie lo ha usado en una comida.
    public async Task<Resultado> BorrarAsync(Usuario usuario, int alimentoId)
    {
        // 1. Se busca el alimento. Aquí SIN AsNoTracking, porque lo vamos a borrar.
        Alimento? alimento = await _db.Set<Alimento>().FindAsync(alimentoId);

        // 2. Las mismas reglas de permiso que al editar.
        Resultado? permiso = VerificarPermiso(usuario, alimento);
        if (permiso != null)
            return permiso;

        // 3. Si ya se usó en alguna comida registrada, no se borra:
        // borrarlo arrastraría el historial de comidas (la llave foránea lo impediría).
        bool usado = await _db.Set<RegistroComida>().AnyAsync(c => c.AlimentoId == alimentoId);
        if (usado)
            return Resultado.Error("No se puede borrar: el alimento ya se usó en comidas registradas.");

        // 4. Se elimina y se guarda.
        _db.Set<Alimento>().Remove(alimento!);
        await _db.SaveChangesAsync();

        return Resultado.Ok("Alimento borrado.");
    }

    // Devuelve los alimentos básicos (sin creador) y los que creó el usuario indicado, ordenados por nombre.
    public async Task<List<Alimento>> ListarAsync(int usuarioId)
    {
        return await _db.Set<Alimento>()
            .AsNoTracking()
            .Where(a => a.UsuarioCreadorId == null || a.UsuarioCreadorId == usuarioId)
            .OrderBy(a => a.Nombre)
            .ToListAsync();
    }

    // Comprueba si el usuario puede modificar o borrar este alimento.
    // Devuelve el error a mostrar, o null si tiene permiso.
    private static Resultado? VerificarPermiso(Usuario usuario, Alimento? alimento)
    {
        // Si no existe, o es de OTRO usuario, se responde igual: así no se revela que existe.
        if (alimento == null || (alimento.UsuarioCreadorId != null && alimento.UsuarioCreadorId != usuario.Id))
            return Resultado.Error("Alimento no encontrado.");

        // Los alimentos básicos (sin creador) solo los toca un Administrador.
        if (alimento.UsuarioCreadorId == null && usuario.Rol != Rol.Administrador)
            return Resultado.Error("Solo un Administrador puede modificar alimentos básicos.");

        return null;
    }

    // Devuelve el mensaje del primer error, o null si todos los datos son válidos.
    private static string? Validar(string nombre, double calorias, double proteina, double carbohidratos, double grasas)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre del alimento no puede estar vacío.";

        // Se escribe "no es válido" (con !) y no "es negativo":
        // así también se rechaza un valor que no sea un número válido (NaN).
        if (!(calorias >= 0))
            return "Las calorías por 100 g no pueden ser negativas ni inválidas.";

        if (!(proteina >= 0))
            return "La proteína por 100 g no puede ser negativa ni inválida.";

        if (!(carbohidratos >= 0))
            return "Los carbohidratos por 100 g no pueden ser negativos ni inválidos.";

        if (!(grasas >= 0))
            return "Las grasas por 100 g no pueden ser negativas ni inválidas.";

        // Todo bien.
        return null;
    }
}
