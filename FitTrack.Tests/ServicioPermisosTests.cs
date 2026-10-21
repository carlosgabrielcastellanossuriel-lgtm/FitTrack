using FitTrack.Core.Datos;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitTrack.Tests;

public class ServicioPermisosTests
{
    // Crea un ContextoBD con base de datos en memoria y un nombre único por llamada.
    // Así cada prueba es independiente y no comparten datos.
    private static ContextoBD CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ContextoBD>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ContextoBD(options);
    }

    [Fact]
    public async Task Aprobar_SolicitudPendiente_QuedaAplicadaYRegistraAprobadorYFecha()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = new Usuario { Id = 1, Correo = "admin@test.com", Rol = Rol.Administrador };
        var solicitante = new Usuario { Id = 2, Correo = "solicitante@test.com", Rol = Rol.Estandar };
        db.Usuarios.Add(admin);
        db.Usuarios.Add(solicitante);
        await db.SaveChangesAsync();

        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitante.Id,
            Permiso = Permiso.ExportarDatos,
            Estado = EstadoSolicitud.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        // Actuar
        var resultado = await servicio.AprobarAsync(solicitud.Id, admin);

        // Verificar
        Assert.True(resultado.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Aplicada, guardada!.Estado);
        Assert.Equal(admin.Id, guardada.ResueltoPorId);
        Assert.NotNull(guardada.FechaResolucion);
    }

    [Fact]
    public async Task Aprobar_SolicitudRechazada_SeRechazaYElEstadoSigueRechazada()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = new Usuario { Id = 1, Correo = "admin@test.com", Rol = Rol.Administrador };
        var solicitante = new Usuario { Id = 2, Correo = "solicitante@test.com", Rol = Rol.Estandar };
        db.Usuarios.Add(admin);
        db.Usuarios.Add(solicitante);
        await db.SaveChangesAsync();

        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitante.Id,
            Permiso = Permiso.ExportarDatos,
            Estado = EstadoSolicitud.Rechazada,
            FechaCreacion = DateTime.UtcNow,
            MotivoRechazo = "No cumple requisitos"
        };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        // Actuar
        var resultado = await servicio.AprobarAsync(solicitud.Id, admin);

        // Verificar
        Assert.False(resultado.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Rechazada, guardada!.Estado);
    }

    [Fact]
    public async Task Rechazar_SinMotivo_NoSeCompletaYSigueSiendoPendiente()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = new Usuario { Id = 1, Correo = "admin@test.com", Rol = Rol.Administrador };
        var solicitante = new Usuario { Id = 2, Correo = "solicitante@test.com", Rol = Rol.Estandar };
        db.Usuarios.Add(admin);
        db.Usuarios.Add(solicitante);
        await db.SaveChangesAsync();

        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitante.Id,
            Permiso = Permiso.ExportarDatos,
            Estado = EstadoSolicitud.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };
        db.SolicitudesPermiso.Add(solicitud);
        await db.SaveChangesAsync();

        // Actuar
        var resultado = await servicio.RechazarAsync(solicitud.Id, admin, "");

        // Verificar
        Assert.False(resultado.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Pendiente, guardada!.Estado);
    }
}
