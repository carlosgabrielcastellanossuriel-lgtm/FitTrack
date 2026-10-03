using System.Security.Cryptography;
using FitTrack.Core.Datos;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Usuarios;

public class ServicioAdministracion
{
    private readonly ContextoBD _db;
    private readonly ServicioContrasena _contrasena;   // se reutiliza lo de la Parte 5

    public ServicioAdministracion(ContextoBD db, ServicioContrasena contrasena)
    {
        _db = db;
        _contrasena = contrasena;
    }

    // ---------- PRIMER ADMINISTRADOR ----------
    // Se ejecuta al arrancar la Api. Si las dos variables de entorno existen y ese
    // correo todavía no está registrado, crea un Administrador ya activo.
    public async Task CrearAdministradorInicialAsync()
    {
        string? correo = Environment.GetEnvironmentVariable("FITTRACK_ADMIN_CORREO");
        string? clave = Environment.GetEnvironmentVariable("FITTRACK_ADMIN_CLAVE");

        // Sin variables no se hace nada (y ni siquiera se toca la base de datos).
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
            return;

        try
        {
            if (!Validaciones.CorreoValido(correo) || !Validaciones.ClaveValida(clave))
            {
                Console.WriteLine("Administrador inicial NO creado: revisa FITTRACK_ADMIN_CORREO y FITTRACK_ADMIN_CLAVE.");
                return;
            }

            string correoNormal = correo.Trim().ToLowerInvariant();
            bool yaExiste = await _db.Usuarios.AnyAsync(u => u.Correo == correoNormal);
            if (yaExiste)
                return;

            Usuario admin = new Usuario();
            admin.Correo = correoNormal;
            admin.ContrasenaHash = Claves.Hashear(clave);
            admin.Activo = true;                    // no necesita activar por correo
            admin.Rol = Rol.Administrador;

            _db.Usuarios.Add(admin);
            await _db.SaveChangesAsync();
            Console.WriteLine("Administrador inicial creado.");
        }
        catch (Exception ex)
        {
            // Típico: falta aplicar las migraciones. La Api arranca igual.
            Console.WriteLine($"No se pudo crear el administrador inicial: {ex.Message}");
        }
    }

    // ---------- LISTAR (RF-CA-21) ----------
    public async Task<List<Usuario>> ListarAsync()
    {
        // AsNoTracking: solo se lee, EF no necesita vigilar estos objetos.
        return await _db.Usuarios.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
    }

    // ---------- CAMBIAR ROL (RF-CA-08) ----------
    // "admin" es quien hace la petición (lo entrega el atributo de la Api).
    public async Task<Resultado> CambiarRolAsync(int usuarioId, Rol rolNuevo, Usuario admin)
    {
        // Evita que el único administrador se quite el rol y nadie pueda administrar.
        if (usuarioId == admin.Id)
            return Resultado.Error("No puedes cambiar tu propio rol.");

        Usuario? usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return Resultado.Error("Usuario no encontrado.");

        usuario.Rol = rolNuevo;
        await _db.SaveChangesAsync();

        return Resultado.Ok("Rol actualizado.");
    }

    // ---------- DESACTIVAR / REACTIVAR (RF-CA-20) ----------
    public async Task<Resultado> CambiarEstadoAsync(int usuarioId, bool activo, Usuario admin)
    {
        // Un Administrador no puede desactivarse a sí mismo.
        if (usuarioId == admin.Id && !activo)
            return Resultado.Error("No puedes desactivarte a ti mismo.");

        Usuario? usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return Resultado.Error("Usuario no encontrado.");

        usuario.Activo = activo;

        // Se cierran sus sesiones abiertas. Así sus tokens dejan de servir al desactivar,
        // y al reactivar no "reviven" sesiones viejas. Este método también guarda
        // el cambio de Activo (usa el mismo contexto de base de datos).
        await _contrasena.CerrarSesionesAsync(usuario.Id);

        return Resultado.Ok(activo ? "Usuario reactivado." : "Usuario desactivado.");
    }

    // ---------- FORZAR RESTABLECIMIENTO (RF-CA-13) ----------
    public async Task<Resultado> ForzarRestablecimientoAsync(int usuarioId)
    {
        Usuario? usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return Resultado.Error("Usuario no encontrado.");

        // La contraseña anterior deja de servir: se reemplaza por el hash de un texto
        // aleatorio que nadie conoce. El usuario define una nueva con el código.
        usuario.ContrasenaHash = Claves.Hashear(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));

        // Se cierran sus sesiones (esto también guarda el cambio de contraseña)...
        await _contrasena.CerrarSesionesAsync(usuario.Id);

        // ...y se le deja en la cola el correo con el código (el de la Parte 5).
        await _contrasena.EnviarCodigoAsync(usuario);

        return Resultado.Ok("Restablecimiento forzado. Se envió un código al correo del usuario.");
    }
}