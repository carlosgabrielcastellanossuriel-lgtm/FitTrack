using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;

namespace FitTrack.Negocio.Alimentos;

public class ServicioAlimento
{
    private readonly ContextoBD _db;

    public ServicioAlimento(ContextoBD db)
    {
        _db = db;
    }

    // Crea un alimento nuevo después de validar sus datos.
    public async Task<Resultado> CrearAsync(string nombre, double calorias, double proteina, double carbohidratos, double grasas)
    {
        // 1. Validaciones: si algo falla, se rechaza con un mensaje claro y no se guarda nada.
        if (string.IsNullOrWhiteSpace(nombre))
            return Resultado.Error("El nombre del alimento no puede estar vacío.");

        // Se escribe "no es válido" (con !) y no "es negativo":
        // así también se rechaza un valor que no sea un número válido (NaN).
        if (!(calorias >= 0))
            return Resultado.Error("Las calorías por 100 g no pueden ser negativas ni inválidas.");

        if (!(proteina >= 0))
            return Resultado.Error("La proteína por 100 g no puede ser negativa ni inválida.");

        if (!(carbohidratos >= 0))
            return Resultado.Error("Los carbohidratos por 100 g no pueden ser negativos ni inválidos.");

        if (!(grasas >= 0))
            return Resultado.Error("Las grasas por 100 g no pueden ser negativas ni inválidas.");

        // 2. Se guarda el alimento nuevo.
        Alimento alimento = new Alimento();
        alimento.Nombre = nombre;
        alimento.CaloriasPor100g = calorias;
        alimento.ProteinaPor100g = proteina;
        alimento.CarbohidratosPor100g = carbohidratos;
        alimento.GrasasPor100g = grasas;

        _db.Set<Alimento>().Add(alimento);
        await _db.SaveChangesAsync();

        return Resultado.Ok("Alimento guardado.");
    }
}
