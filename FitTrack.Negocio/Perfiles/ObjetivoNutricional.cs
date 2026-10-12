namespace FitTrack.Negocio.Perfiles;

// Lo que la persona quiere lograr. Decide si se restan, se suman o se mantienen las calorías.
public enum ObjetivoNutricional
{
    Deficit = 0,        // bajar de peso (comer menos de lo que se gasta)
    Mantenimiento = 1,  // mantener el peso actual
    Superavit = 2       // subir de peso (comer más de lo que se gasta)
}
