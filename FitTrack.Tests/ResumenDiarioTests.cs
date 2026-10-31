using System.Runtime.CompilerServices;
using FitTrack.Core.Datos;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Perfiles;
using FitTrack.Negocio.Resumenes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitTrack.Tests;

public class ResumenDiarioTests
{
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

        ServicioPermisos permisos = new ServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);   // queda pendiente, sin aprobar

        ServicioResumen resumen = new ServicioResumen(db, permisos);

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

        ServicioPermisos permisos = new ServicioPermisos(db);
        await permisos.CrearAsync(estandar, Permiso.VerEstadisticas);

        int idSolicitud = db.SolicitudesPermiso.First().Id;
        await permisos.AprobarAsync(idSolicitud, admin);

        ServicioResumen resumen = new ServicioResumen(db, permisos);

        // Actuar
        List<ResumenDiario>? historial = await resumen.HistorialAsync(estandar);

        // Verificar
        Assert.NotNull(historial);
    }
}
