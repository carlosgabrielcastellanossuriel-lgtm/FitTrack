using System.Runtime.CompilerServices;
using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Notificaciones;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Alimentos;
using FitTrack.Negocio.Perfiles;
using FitTrack.Negocio.Resumenes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitTrack.Tests;

public class NotificacionesTests
{
    [ModuleInitializer]
    internal static void RegistrarEntidadesNegocio()
    {
        ContextoBD.EntidadesNegocio.Add(typeof(Alimento));
        ContextoBD.EntidadesNegocio.Add(typeof(PerfilUsuario));
        ContextoBD.EntidadesNegocio.Add(typeof(ResumenDiario));
        ContextoBD.EntidadesNegocio.Add(typeof(RegistroComida));
    }

    private static ContextoBD CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ContextoBD>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ContextoBD(options);
    }

    private static ServicioNotificaciones CrearServicioNotificaciones(ContextoBD db)
        => new ServicioNotificaciones(db, new ServicioColaCorreo(db));

    private static ServicioPermisos CrearServicioPermisos(ContextoBD db)
        => new ServicioPermisos(db, CrearServicioNotificaciones(db));

    private static ServicioResumen CrearServicioResumen(ContextoBD db)
        => new ServicioResumen(db, CrearServicioPermisos(db), CrearServicioNotificaciones(db));

    private static Usuario CrearUsuario(ContextoBD db, int id, Rol rol, bool notificarPorCorreo = true, bool activo = true)
    {
        var usuario = new Usuario
        {
            Id = id,
            Correo = $"usuario{id}@test.com",
            Rol = rol,
            Activo = activo,
            NotificarPorCorreo = notificarPorCorreo
        };
        db.Usuarios.Add(usuario);
        db.SaveChanges();
        return usuario;
    }

    [Fact]
    public async Task RF_NOT_01_CrearSolicitud_NotificaSoloAdministradoresActivos()
    {
        using var db = CrearContexto();
        var admin1 = CrearUsuario(db, 1, Rol.Administrador, activo: true);
        var admin2 = CrearUsuario(db, 2, Rol.Administrador, activo: true);
        var adminInactivo = CrearUsuario(db, 3, Rol.Administrador, activo: true);
        adminInactivo.Activo = false;
        await db.SaveChangesAsync();

        var estandar = CrearUsuario(db, 4, Rol.Estandar, activo: true);
        var permisos = CrearServicioPermisos(db);

        var resultado = await permisos.CrearAsync(estandar, Permiso.ExportarDatos);

        Assert.True(resultado.Exito);
        Assert.Single(await db.Notificaciones.Where(n => n.UsuarioId == admin1.Id).ToListAsync());
        Assert.Single(await db.Notificaciones.Where(n => n.UsuarioId == admin2.Id).ToListAsync());
        Assert.Empty(await db.Notificaciones.Where(n => n.UsuarioId == adminInactivo.Id).ToListAsync());
        Assert.Empty(await db.Notificaciones.Where(n => n.UsuarioId == estandar.Id).ToListAsync());
    }

    [Fact]
    public async Task RF_NOT_05_MarcarLeida_DeOtroUsuario_FallaYSigueSinLeer()
    {
        using var db = CrearContexto();
        var usuarioA = CrearUsuario(db, 1, Rol.Estandar, activo: true);
        var usuarioB = CrearUsuario(db, 2, Rol.Estandar, activo: true);
        var servicio = CrearServicioNotificaciones(db);

        await servicio.NotificarAsync(usuarioA.Id, "Aviso", "Mensaje de prueba");
        var notificacion = await db.Notificaciones.SingleAsync();

        var intentoAjeno = await servicio.MarcarLeidaAsync(usuarioB.Id, notificacion.Id);

        Assert.False(intentoAjeno.Exito);
        Assert.Equal("Notificación no encontrada.", intentoAjeno.Mensaje);
        Assert.False((await db.Notificaciones.FindAsync(notificacion.Id))!.Leida);

        var intentoPropio = await servicio.MarcarLeidaAsync(usuarioA.Id, notificacion.Id);
        Assert.True(intentoPropio.Exito);
        Assert.True((await db.Notificaciones.FindAsync(notificacion.Id))!.Leida);
    }

    [Fact]
    public async Task RF_NOT_06_Y_07_PreferenciaCorreo_ControlaEncoladoDeCorreo()
    {
        using var db = CrearContexto();
        var sinCorreo = CrearUsuario(db, 1, Rol.Estandar, notificarPorCorreo: false, activo: true);
        var conCorreo = CrearUsuario(db, 2, Rol.Estandar, notificarPorCorreo: true, activo: true);
        var servicio = CrearServicioNotificaciones(db);

        await servicio.NotificarAsync(sinCorreo.Id, "Sin correo", "Solo notificación interna");
        Assert.Single(await db.Notificaciones.Where(n => n.UsuarioId == sinCorreo.Id).ToListAsync());
        Assert.Empty(await db.CorreosEnCola.ToListAsync());

        await servicio.NotificarAsync(conCorreo.Id, "Con correo", "Notificación interna y correo");
        Assert.Single(await db.Notificaciones.Where(n => n.UsuarioId == conCorreo.Id).ToListAsync());
        var correo = await db.CorreosEnCola.SingleAsync();
        Assert.Equal(conCorreo.Correo, correo.Destinatario);
        Assert.Equal(EstadoCorreo.Pendiente, correo.Estado);
    }

    [Fact]
    public async Task RF_NOT_03_DiaExcedido_NotificaUnaSolaVezPorDia()
    {
        using var db = CrearContexto();
        var usuario = CrearUsuario(db, 1, Rol.Estandar, activo: true);

        db.Set<PerfilUsuario>().Add(new PerfilUsuario
        {
            UsuarioId = usuario.Id,
            CaloriasMeta = 2000,
            ProteinaMeta = 150,
            CarbohidratosMeta = 200,
            GrasasMeta = 70
        });

        var alimento = new Alimento
        {
            Nombre = "Pizza",
            CaloriasPor100g = 300,
            ProteinaPor100g = 12,
            CarbohidratosPor100g = 35,
            GrasasPor100g = 12
        };
        db.Set<Alimento>().Add(alimento);
        await db.SaveChangesAsync();

        var resumen = CrearServicioResumen(db);

        var primera = await resumen.RegistrarComidaAsync(usuario.Id, alimento.Id, 800);
        var segunda = await resumen.RegistrarComidaAsync(usuario.Id, alimento.Id, 100);

        Assert.True(primera.Exito);
        Assert.True(segunda.Exito);
        var notificaciones = await db.Notificaciones.Where(n => n.UsuarioId == usuario.Id).ToListAsync();
        Assert.Single(notificaciones);
        Assert.Equal("Día excedido", notificaciones[0].Titulo);
    }
}
