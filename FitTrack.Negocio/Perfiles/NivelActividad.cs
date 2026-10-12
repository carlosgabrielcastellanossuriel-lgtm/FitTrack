namespace FitTrack.Negocio.Perfiles;

// Qué tan activa es la persona en su semana normal.
// Mientras más activa, más calorías gasta al día.
public enum NivelActividad
{
    Sedentario = 0,   // casi no hace ejercicio (trabajo de escritorio)
    Ligero = 1,       // ejercicio suave 1 a 3 días por semana
    Moderado = 2,     // ejercicio 3 a 5 días por semana
    Alto = 3          // ejercicio intenso 6 a 7 días por semana
}
