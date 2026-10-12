using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Negocio.Perfiles;

public class ServicioPerfil
{
    // Rangos aceptados. Si algún día hay que cambiarlos, se cambian solo aquí.
    private const int EdadMinima = 15;
    private const int EdadMaxima = 100;
    private const double PesoMinimoLibras = 66;
    private const double PesoMaximoLibras = 660;
    private const double AlturaMinimaPulgadas = 39;
    private const double AlturaMaximaPulgadas = 98;

    private readonly ContextoBD _db;

    public ServicioPerfil(ContextoBD db)
    {
        _db = db;
    }

    // ---------- VER EL PERFIL ----------
    // Devuelve el perfil del usuario, o null si todavía no lo ha creado.
    public async Task<PerfilUsuario?> ObtenerAsync(int usuarioId)
    {
        // ContextoBD (en Core) no conoce PerfilUsuario, por eso no tiene un DbSet para él.
        // Set<PerfilUsuario>() le pide a EF esa tabla directamente.
        // AsNoTracking: solo se lee, EF no necesita vigilar este objeto.
        return await _db.Set<PerfilUsuario>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
    }

    // ---------- CREAR O ACTUALIZAR EL PERFIL ----------
    // Un usuario tiene un solo perfil: si ya existe se actualiza, si no se crea.
    public async Task<Resultado> GuardarAsync(int usuarioId, DatosPerfil datos)
    {
        // 1. Validaciones: si algo falla, se rechaza con un mensaje claro y no se guarda nada.
        string? error = Validar(datos);
        if (error != null)
            return Resultado.Error(error);

        // 2. Se calculan las metas con los datos recibidos.
        MetasCalculadas metas = CalculadoraMetas.Calcular(
            datos.Edad, datos.Peso, datos.Altura,
            datos.Sexo, datos.NivelActividad, datos.Objetivo);

        // 3. Se busca el perfil actual. Aquí SIN AsNoTracking, porque lo vamos a modificar.
        PerfilUsuario? perfil = await _db.Set<PerfilUsuario>()
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

        if (perfil == null)
        {
            // Primera vez: se crea el perfil nuevo.
            perfil = new PerfilUsuario();
            perfil.UsuarioId = usuarioId;
            _db.Set<PerfilUsuario>().Add(perfil);
        }

        // 4. Se copian los datos y las metas (sirve igual para perfil nuevo o existente).
        perfil.Edad = datos.Edad;
        perfil.Peso = datos.Peso;
        perfil.Altura = datos.Altura;
        perfil.Sexo = datos.Sexo;
        perfil.NivelActividad = datos.NivelActividad;
        perfil.Objetivo = datos.Objetivo;
        perfil.CaloriasMeta = metas.Calorias;
        perfil.ProteinaMeta = metas.ProteinaGramos;
        perfil.CarbohidratosMeta = metas.CarbohidratosGramos;
        perfil.GrasasMeta = metas.GrasasGramos;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Pasa si dos peticiones crean el perfil del mismo usuario en el mismo instante:
            // el índice único de la base de datos bloquea la segunda.
            return Resultado.Error("No se pudo guardar el perfil. Intenta de nuevo.");
        }

        return Resultado.Ok("Perfil guardado.");
    }

    // ---------- VALIDACIONES ----------
    // Devuelve el mensaje del primer error que encuentre, o null si todo está bien.
    private static string? Validar(DatosPerfil datos)
    {
        if (datos.Edad < EdadMinima || datos.Edad > EdadMaxima)
            return $"La edad debe estar entre {EdadMinima} y {EdadMaxima} años.";

        // Se escribe "no está dentro del rango" (con !) y no "está fuera":
        // así también se rechaza un valor que no sea un número válido (NaN).
        if (!(datos.Peso >= PesoMinimoLibras && datos.Peso <= PesoMaximoLibras))
            return $"El peso debe estar entre {PesoMinimoLibras} y {PesoMaximoLibras} libras.";

        if (!(datos.Altura >= AlturaMinimaPulgadas && datos.Altura <= AlturaMaximaPulgadas))
            return $"La altura debe estar entre {AlturaMinimaPulgadas} y {AlturaMaximaPulgadas} pulgadas.";

        // IsDefined evita que pasen números inventados como "7" en los enums.
        if (!Enum.IsDefined(datos.Sexo))
            return "Sexo no válido. Usa Masculino o Femenino.";

        if (!Enum.IsDefined(datos.NivelActividad))
            return "Nivel de actividad no válido. Usa Sedentario, Ligero, Moderado o Alto.";

        if (!Enum.IsDefined(datos.Objetivo))
            return "Objetivo no válido. Usa Deficit, Mantenimiento o Superavit.";

        return null;
    }
}
