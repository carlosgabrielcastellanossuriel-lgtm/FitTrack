using System.Runtime.CompilerServices;
using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Notificaciones;
using FitTrack.Core.Permisos;
using FitTrack.Core.Reportes;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Alimentos;
using FitTrack.Negocio.Perfiles;
using FitTrack.Negocio.Resumenes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitTrack.Tests;

public class ReportesTests
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

    private static ServicioResumen CrearServicioResumen(ContextoBD db)
        => new ServicioResumen(
            db,
            new ServicioPermisos(db, CrearServicioNotificaciones(db)),
            CrearServicioNotificaciones(db));

    private static Usuario CrearUsuario(ContextoBD db, int id, Rol rol)
    {
        var usuario = new Usuario
        {
            Id = id,
            Correo = $"usuario{id}@test.com",
            Rol = rol,
            Activo = true
        };
        db.Usuarios.Add(usuario);
        db.SaveChanges();
        return usuario;
    }

    private static void CrearSolicitud(ContextoBD db, int solicitanteId, EstadoSolicitud estado, DateTime fecha)
    {
        db.SolicitudesPermiso.Add(new SolicitudPermiso
        {
            SolicitanteId = solicitanteId,
            Permiso = Permiso.ExportarDatos,
            Estado = estado,
            FechaCreacion = fecha
        });
    }

    private static void CrearResumen(ContextoBD db, int usuarioId, DateOnly fecha, EstadoResumen estado, double calorias)
    {
        db.Set<ResumenDiario>().Add(new ResumenDiario
        {
            UsuarioId = usuarioId,
            Fecha = fecha,
            EstadoActual = estado,
            CaloriasAcumuladas = calorias
        });
    }

    [Fact]
    public async Task RF_REP_04_EstandarCuentaSoloSusSolicitudes_YAdministradorCuentaTodas()
    {
        using var db = CrearContexto();
        var admin = CrearUsuario(db, 1, Rol.Administrador);
        var estandarA = CrearUsuario(db, 2, Rol.Estandar);
        var estandarB = CrearUsuario(db, 3, Rol.Estandar);

        DateTime fecha = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
        CrearSolicitud(db, estandarA.Id, EstadoSolicitud.Pendiente, fecha);
        CrearSolicitud(db, estandarA.Id, EstadoSolicitud.Pendiente, fecha);
        CrearSolicitud(db, estandarA.Id, EstadoSolicitud.Aprobada, fecha);
        CrearSolicitud(db, estandarB.Id, EstadoSolicitud.Pendiente, fecha);
        await db.SaveChangesAsync();

        var servicio = new ServicioReportes(db);

        var propias = await servicio.SolicitudesPorEstadoAsync(estandarA, null, null);
        var todas = await servicio.SolicitudesPorEstadoAsync(admin, null, null);

        Assert.Equal(2, propias.Single(f => f.Estado == EstadoSolicitud.Pendiente).Total);
        Assert.Equal(1, propias.Single(f => f.Estado == EstadoSolicitud.Aprobada).Total);
        Assert.Equal(3, propias.Sum(f => f.Total));

        Assert.Equal(3, todas.Single(f => f.Estado == EstadoSolicitud.Pendiente).Total);
        Assert.Equal(1, todas.Single(f => f.Estado == EstadoSolicitud.Aprobada).Total);
        Assert.Equal(4, todas.Sum(f => f.Total));
    }

    [Fact]
    public async Task RF_REP_05_RangosDistintos_DanResultadosDistintos()
    {
        using var db = CrearContexto();
        var admin = CrearUsuario(db, 1, Rol.Administrador);

        CrearSolicitud(db, admin.Id, EstadoSolicitud.Pendiente, new DateTime(2026, 11, 5, 10, 0, 0, DateTimeKind.Utc));
        CrearSolicitud(db, admin.Id, EstadoSolicitud.Pendiente, new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc));
        // El día "hasta" entra completo, incluso si la solicitud se creó casi a medianoche.
        CrearSolicitud(db, admin.Id, EstadoSolicitud.Aprobada, new DateTime(2026, 11, 15, 23, 30, 0, DateTimeKind.Utc));
        CrearSolicitud(db, admin.Id, EstadoSolicitud.Rechazada, new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();

        var servicio = new ServicioReportes(db);

        var primeraQuincena = await servicio.SolicitudesPorEstadoAsync(
            admin, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 15));
        var segundaQuincena = await servicio.SolicitudesPorEstadoAsync(
            admin, new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 30));

        Assert.Equal(3, primeraQuincena.Sum(f => f.Total));
        Assert.Equal(1, segundaQuincena.Sum(f => f.Total));
        Assert.NotEqual(primeraQuincena.Sum(f => f.Total), segundaQuincena.Sum(f => f.Total));
    }

    // H-01 (RF-REP-05): el día "hasta" se incluye completo y el límite máximo
    // de DateOnly no debe desbordar AddDays(1).
    [Fact]
    public async Task H_01_HastaFechaMaxima_NoLanzaYIncluyeElDia()
    {
        // Preparar
        using var db = CrearContexto();
        var admin = CrearUsuario(db, 1, Rol.Administrador);
        CrearSolicitud(db, admin.Id, EstadoSolicitud.Pendiente, new DateTime(9999, 12, 31, 12, 0, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();
        var servicio = new ServicioReportes(db);

        // Actuar
        var filas = await servicio.SolicitudesPorEstadoAsync(admin, null, DateOnly.MaxValue);

        // Verificar
        Assert.Single(filas);
        Assert.Equal(1, filas[0].Total);
    }

    [Fact]
    public async Task RF_REP_02_Y_03_DiasPorEstado_AgrupaYDevuelveMenosFilasQueResumenes()
    {
        using var db = CrearContexto();
        var estandar = CrearUsuario(db, 1, Rol.Estandar);

        CrearResumen(db, estandar.Id, new DateOnly(2026, 11, 1), EstadoResumen.DentroDeMeta, 1000);
        CrearResumen(db, estandar.Id, new DateOnly(2026, 11, 2), EstadoResumen.DentroDeMeta, 2000);
        CrearResumen(db, estandar.Id, new DateOnly(2026, 11, 3), EstadoResumen.DentroDeMeta, 3000);
        CrearResumen(db, estandar.Id, new DateOnly(2026, 11, 4), EstadoResumen.Excedido, 4000);
        await db.SaveChangesAsync();

        var servicio = CrearServicioResumen(db);

        var filas = await servicio.DiasPorEstadoAsync(estandar, null, null);

        Assert.Equal(2, filas.Count);
        Assert.True(filas.Count < 4);

        var dentroDeMeta = filas.Single(f => f.Estado == EstadoResumen.DentroDeMeta);
        Assert.Equal(3, dentroDeMeta.Dias);
        Assert.Equal(2000, dentroDeMeta.PromedioCalorias);

        var excedido = filas.Single(f => f.Estado == EstadoResumen.Excedido);
        Assert.Equal(1, excedido.Dias);
        Assert.Equal(4000, excedido.PromedioCalorias);
    }
}
