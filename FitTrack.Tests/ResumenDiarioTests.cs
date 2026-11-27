using System.Runtime.CompilerServices;
using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Notificaciones;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Perfiles;
using FitTrack.Negocio.Resumenes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitTrack.Tests;

public class ResumenDiarioTests
{
    private static ServicioPermisos CrearServicioPermisos(ContextoBD db)
        => new ServicioPermisos(db, new ServicioNotificaciones(db, new ServicioColaCorreo(db)));

    private static ServicioResumen CrearServicioResumen(ContextoBD db, ServicioPermisos permisos)
        => new ServicioResumen(db, permisos, new ServicioNotificaciones(db, new ServicioColaCorreo(db)));

    // Registra ResumenDiario como tabla del contexto antes de que corran las pruebas.
    [ModuleInitializer]
    public static void RegistrarEntidadesNegocio() { ContextoBD.EntidadesNegocio.Add(typeof(ResumenDiario)); }

    // Crea un ContextoBD con base de datos en memoria y un nombre único por llamada.
    // Así cada prueba es independiente y no comparten datos.
    private static ContextoBD CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ContextoBD>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ContextoBD(options);
    }

    // Crea, guarda y devuelve un Usuario con el id y rol indicados.
    private static Usuario CrearUsuario(ContextoBD db, int id, Rol rol)
    {
        var usuario = new Usuario { Id = id, Correo = $"usuario{id}@test.com", Rol = rol };
        db.Usuarios.Add(usuario);
        db.SaveChanges();
        return usuario;
    }

    [Fact]
    public void Transicion_EnProgreso_A_DentroDeMeta_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.DentroDeMeta);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.DentroDeMeta, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_EnProgreso_A_Excedido_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Excedido);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_DentroDeMeta_A_Excedido_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.DentroDeMeta;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Excedido);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_DentroDeMeta_A_Cerrado_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.DentroDeMeta;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Cerrado);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_Excedido_A_Cerrado_SePermite()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Excedido;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.Cerrado);

        // Verificar
        Assert.True(cambio);
        Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_Cerrado_A_EnProgreso_SeRechazaYElEstadoNoCambia()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Cerrado;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.EnProgreso);

        // Verificar
        Assert.False(cambio);
        Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
    }

    [Fact]
    public void Transicion_Excedido_A_DentroDeMeta_SeRechazaYElEstadoNoCambia()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Excedido;

        // Actuar
        bool cambio = resumen.CambiarEstado(EstadoResumen.DentroDeMeta);

        // Verificar
        Assert.False(cambio);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void EstadoTerminal_Cerrado_NoTieneSalida()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.Cerrado;

        // Actuar
        // Se intenta cada estado posible como destino.
        foreach (EstadoResumen destino in Enum.GetValues<EstadoResumen>())
        {
            // Verificar: ninguna salida se permite y el estado no cambia.
            Assert.False(resumen.CambiarEstado(destino));
            Assert.Equal(EstadoResumen.Cerrado, resumen.EstadoActual);
        }
    }

    [Fact]
    public void RegistrarComida_ConTotalSobreLaMeta_QuedaExcedido()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        PerfilUsuario perfil = new PerfilUsuario();
        perfil.CaloriasMeta = 2000;
        perfil.ProteinaMeta = 150;
        perfil.CarbohidratosMeta = 200;
        perfil.GrasasMeta = 70;

        // Actuar
        bool registrada = resumen.RegistrarComida(2500, 100, 150, 50, perfil);

        // Verificar
        Assert.True(registrada);
        Assert.Equal(EstadoResumen.Excedido, resumen.EstadoActual);
    }

    [Fact]
    public void RegistrarComida_DentroDeLaMeta_QuedaDentroDeMeta()
    {
        // Preparar
        ResumenDiario resumen = new ResumenDiario();
        resumen.EstadoActual = EstadoResumen.EnProgreso;

        PerfilUsuario perfil = new PerfilUsuario();
        perfil.CaloriasMeta = 2000;
        perfil.ProteinaMeta = 150;
        perfil.CarbohidratosMeta = 200;
        perfil.GrasasMeta = 70;

        // Actuar
        bool registrada = resumen.RegistrarComida(500, 30, 50, 20, perfil);

        // Verificar
        Assert.True(registrada);
        Assert.Equal(EstadoResumen.DentroDeMeta, resumen.EstadoActual);
    }

    [Fact]
    public async Task Historial_SinPermisoAprobado_SeRechaza()
    {
        // Preparar
        ContextoBD db = CrearContexto();
        Usuario admin = CrearUsuario(db, 1, Rol.Administrador);
        Usuario estandar = CrearUsuario(db, 2, Rol.Estandar);

        ServicioPermisos permisos = CrearServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);   // queda pendiente, sin aprobar

        ServicioResumen resumen = CrearServicioResumen(db, permisos);

        // Actuar
        List<ResumenDiario>? historial = await resumen.HistorialAsync(estandar);

        // Verificar
        Assert.Null(historial);
    }

    [Fact]
    public async Task Historial_ConPermisoAprobadoPorAdmin_SePermite()
    {
        // Preparar
        ContextoBD db = CrearContexto();
        Usuario admin = CrearUsuario(db, 1, Rol.Administrador);
        Usuario estandar = CrearUsuario(db, 2, Rol.Estandar);

        ServicioPermisos permisos = CrearServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);

        int idSolicitud = db.SolicitudesPermiso.First().Id;
        await permisos.AprobarAsync(idSolicitud, admin);

        ServicioResumen resumen = CrearServicioResumen(db, permisos);

        // Actuar
        List<ResumenDiario>? historial = await resumen.HistorialAsync(estandar);

        // Verificar
        Assert.NotNull(historial);
    }

    [Fact]
    public async Task Promedio_SinPermisoAprobado_DevuelveNull()
    {
        // Preparar
        ContextoBD db = CrearContexto();
        Usuario estandar = CrearUsuario(db, 1, Rol.Estandar);

        ServicioPermisos permisos = CrearServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);   // queda pendiente, sin aprobar

        ServicioResumen resumen = CrearServicioResumen(db, permisos);

        // Actuar
        PromedioPeriodo? promedio = await resumen.PromedioAsync(estandar, 7);

        // Verificar
        Assert.Null(promedio);
    }

    [Fact]
    public async Task Promedio_ConPermisoYDosResumenesEnRango_PromediaCalorias()
    {
        // Preparar
        ContextoBD db = CrearContexto();
        Usuario admin = CrearUsuario(db, 1, Rol.Administrador);
        Usuario estandar = CrearUsuario(db, 2, Rol.Estandar);

        ServicioPermisos permisos = CrearServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);

        int idSolicitud = db.SolicitudesPermiso.First().Id;
        await permisos.AprobarAsync(idSolicitud, admin);

        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

        ResumenDiario dia1 = new ResumenDiario();
        dia1.UsuarioId = estandar.Id;
        dia1.Fecha = hoy;
        dia1.CaloriasAcumuladas = 2000;
        db.Set<ResumenDiario>().Add(dia1);

        ResumenDiario dia2 = new ResumenDiario();
        dia2.UsuarioId = estandar.Id;
        dia2.Fecha = hoy.AddDays(-1);
        dia2.CaloriasAcumuladas = 1000;
        db.Set<ResumenDiario>().Add(dia2);

        await db.SaveChangesAsync();

        ServicioResumen resumen = CrearServicioResumen(db, permisos);

        // Actuar
        PromedioPeriodo? promedio = await resumen.PromedioAsync(estandar, 7);

        // Verificar
        Assert.NotNull(promedio);
        Assert.Equal(2, promedio.DiasRegistrados);
        Assert.Equal(1500, promedio.Calorias);
    }

    [Fact]
    public async Task Promedio_ResumenFueraDelRango_NoCuenta()
    {
        // Preparar
        ContextoBD db = CrearContexto();
        Usuario admin = CrearUsuario(db, 1, Rol.Administrador);
        Usuario estandar = CrearUsuario(db, 2, Rol.Estandar);

        ServicioPermisos permisos = CrearServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);

        int idSolicitud = db.SolicitudesPermiso.First().Id;
        await permisos.AprobarAsync(idSolicitud, admin);

        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

        // Dentro del rango de 7 días.
        ResumenDiario reciente = new ResumenDiario();
        reciente.UsuarioId = estandar.Id;
        reciente.Fecha = hoy;
        reciente.CaloriasAcumuladas = 1000;
        db.Set<ResumenDiario>().Add(reciente);

        // Más viejo que el rango (hace 10 días).
        ResumenDiario viejo = new ResumenDiario();
        viejo.UsuarioId = estandar.Id;
        viejo.Fecha = hoy.AddDays(-10);
        viejo.CaloriasAcumuladas = 5000;
        db.Set<ResumenDiario>().Add(viejo);

        await db.SaveChangesAsync();

        ServicioResumen resumen = CrearServicioResumen(db, permisos);

        // Actuar
        PromedioPeriodo? promedio = await resumen.PromedioAsync(estandar, 7);

        // Verificar
        Assert.NotNull(promedio);
        Assert.Equal(1, promedio.DiasRegistrados);
        Assert.Equal(1000, promedio.Calorias);
    }

    [Fact]
    public async Task Promedio_SinResumenes_DevuelveCeros()
    {
        // Preparar
        ContextoBD db = CrearContexto();
        Usuario admin = CrearUsuario(db, 1, Rol.Administrador);
        Usuario estandar = CrearUsuario(db, 2, Rol.Estandar);

        ServicioPermisos permisos = CrearServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);

        int idSolicitud = db.SolicitudesPermiso.First().Id;
        await permisos.AprobarAsync(idSolicitud, admin);

        ServicioResumen resumen = CrearServicioResumen(db, permisos);

        // Actuar
        PromedioPeriodo? promedio = await resumen.PromedioAsync(estandar, 7);

        // Verificar
        Assert.NotNull(promedio);
        Assert.Equal(0, promedio.DiasRegistrados);
        Assert.Equal(0, promedio.Calorias);
        Assert.Equal(0, promedio.Proteinas);
        Assert.Equal(0, promedio.Carbohidratos);
        Assert.Equal(0, promedio.Grasas);
    }
}
