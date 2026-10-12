namespace FitTrack.Negocio.Perfiles;

// El "paquete" con las metas diarias que devuelve la calculadora.
public class MetasCalculadas
{
    public int Calorias { get; set; }
    public double ProteinaGramos { get; set; }
    public double CarbohidratosGramos { get; set; }
    public double GrasasGramos { get; set; }
}

// Calcula las metas de calorías y macros a partir de los datos del usuario.
// Es lógica pura: no toca la base de datos, solo recibe números y devuelve números.
// "static" = no hace falta crear un objeto, se usa directo: CalculadoraMetas.Calcular(...)
public static class CalculadoraMetas
{
    // ---------- Constantes: si hay que ajustar un número, se cambia solo aquí ----------

    // Fórmula Mifflin-St Jeor en libras y pulgadas.
    private const double CoeficientePeso = 4.536;      // por cada libra
    private const double CoeficienteAltura = 15.875;   // por cada pulgada
    private const double CoeficienteEdad = 5;          // se resta por cada año
    private const double AjusteHombre = 5;
    private const double AjusteMujer = -161;

    // Calorías que se restan o suman según el objetivo.
    private const int AjusteDeficit = -500;
    private const int AjusteSuperavit = 300;
    private const int CaloriasMinimas = 1200;          // por seguridad, nunca se baja de aquí

    // Reparto de macros.
    private const double ProteinaGramosPorLibra = 1;   // 1 g de proteína por libra de peso
    private const double PorcentajeGrasas = 0.25;        // 25% de las calorías vienen de grasas

    // Cuántas calorías tiene cada gramo de cada macro.
    private const int CaloriasPorGramoProteina = 4;
    private const int CaloriasPorGramoCarbohidrato = 4;
    private const int CaloriasPorGramoGrasa = 9;

    public static MetasCalculadas Calcular( int edad, double pesoLibras, double alturaPulgadas, Sexo sexo, NivelActividad actividad, ObjetivoNutricional objetivo)
    {
        // 1. Metabolismo basal (TMB): las calorías que el cuerpo gasta en reposo.
        double tmb = (CoeficientePeso * pesoLibras)
                   + (CoeficienteAltura * alturaPulgadas)
                   - (CoeficienteEdad * edad);

        tmb += (sexo == Sexo.Masculino) ? AjusteHombre : AjusteMujer;

        // 2. Calorías de mantenimiento: el TMB multiplicado por qué tan activa es la persona.
        double calorias = tmb * FactorActividad(actividad);

        // 3. Se ajusta según el objetivo y se pone el mínimo de seguridad.
        calorias += AjusteSegunObjetivo(objetivo);
        if (calorias < CaloriasMinimas)
            calorias = CaloriasMinimas;

        // 4. Macros en gramos.
        // Proteína: fija según el peso.
        double proteina = pesoLibras * ProteinaGramosPorLibra;

        // Grasas: un porcentaje de las calorías, pasado a gramos.
        double grasas = (calorias * PorcentajeGrasas) / CaloriasPorGramoGrasa;

        // Carbohidratos: lo que sobra de calorías después de proteína y grasas.
        double caloriasRestantes = calorias
                                 - (proteina * CaloriasPorGramoProteina)
                                 - (grasas * CaloriasPorGramoGrasa);
        double carbohidratos = caloriasRestantes / CaloriasPorGramoCarbohidrato;
        if (carbohidratos < 0)
            carbohidratos = 0;

        // 5. Se redondea al final: calorías a entero y macros a 1 decimal.
        MetasCalculadas metas = new MetasCalculadas();
        metas.Calorias = (int)Math.Round(calorias);
        metas.ProteinaGramos = Math.Round(proteina, 1);
        metas.CarbohidratosGramos = Math.Round(carbohidratos, 1);
        metas.GrasasGramos = Math.Round(grasas, 1);
        return metas;
    }

    // Cuántas veces el TMB se gasta realmente en el día según la actividad.
    private static double FactorActividad(NivelActividad actividad)
    {
        switch (actividad)
        {
            case NivelActividad.Sedentario: return 1.2;
            case NivelActividad.Ligero: return 1.375;
            case NivelActividad.Moderado: return 1.55;
            case NivelActividad.Alto: return 1.725;
            default: return 1.2;
        }
    }

    // Calorías que se suman o restan según el objetivo.
    private static int AjusteSegunObjetivo(ObjetivoNutricional objetivo)
    {
        switch (objetivo)
        {
            case ObjetivoNutricional.Deficit: return AjusteDeficit;
            case ObjetivoNutricional.Superavit: return AjusteSuperavit;
            default: return 0;   // Mantenimiento
        }
    }
}
