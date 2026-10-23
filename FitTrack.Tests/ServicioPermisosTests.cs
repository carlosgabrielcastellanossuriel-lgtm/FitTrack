using System.Linq;
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

    // Crea, guarda y devuelve un Usuario con el id y rol indicados.
    private static Usuario CrearUsuario(ContextoBD db, int id, Rol rol)
    {
        var usuario = new Usuario { Id = id, Correo = $"usuario{id}@test.com", Rol = rol };
        db.Usuarios.Add(usuario);
        db.SaveChanges();
        return usuario;
    }

    // Crea, guarda y devuelve una SolicitudPermiso para el solicitante indicado.
    private static SolicitudPermiso CrearSolicitud(ContextoBD db, int solicitanteId, EstadoSolicitud estado)
    {
        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitanteId,
            Permiso = Permiso.ExportarDatos,
            Estado = estado,
            FechaCreacion = DateTime.UtcNow
        };
        db.SolicitudesPermiso.Add(solicitud);
        db.SaveChanges();
        return solicitud;
    }

    // RF_GP_03_Y_05: aprobar una pendiente la deja Aplicada, registra aprobador y fecha,
    // y aplica el permiso al solicitante.
    [Fact]
    public async Task RF_GP_03_Y_05_Aprobar_SolicitudPendiente_QuedaAplicadaYRegistraAprobadorYFecha()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = CrearUsuario(db, 1, Rol.Administrador);
        var solicitante = CrearUsuario(db, 2, Rol.Estandar);
        var solicitud = CrearSolicitud(db, solicitante.Id, EstadoSolicitud.Pendiente);

        // Actuar
        var resultado = await servicio.AprobarAsync(solicitud.Id, admin);

        // Verificar
        Assert.True(resultado.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Aplicada, guardada!.Estado);
        Assert.Equal(admin.Id, guardada.ResueltoPorId);
        Assert.NotNull(guardada.FechaResolucion);
        Assert.True(await servicio.TieneAsync(solicitante.Id, Permiso.ExportarDatos));
    }

    // RF_GP_06: intentar aprobar una ya rechazada falla y el estado sigue siendo Rechazada.
    [Fact]
    public async Task RF_GP_06_Aprobar_SolicitudRechazada_SeRechazaYElEstadoSigueRechazada()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = CrearUsuario(db, 1, Rol.Administrador);
        var solicitante = CrearUsuario(db, 2, Rol.Estandar);
        var solicitud = CrearSolicitud(db, solicitante.Id, EstadoSolicitud.Rechazada);

        // Actuar
        var resultado = await servicio.AprobarAsync(solicitud.Id, admin);

        // Verificar
        Assert.False(resultado.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Rechazada, guardada!.Estado);
    }

    // RF_GP_04: rechazar sin motivo (null, vacío o solo espacios) no se completa y sigue siendo Pendiente.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RF_GP_04_Rechazar_SinMotivo_NoSeCompletaYSigueSiendoPendiente(string? motivo)
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = CrearUsuario(db, 1, Rol.Administrador);
        var solicitante = CrearUsuario(db, 2, Rol.Estandar);
        var solicitud = CrearSolicitud(db, solicitante.Id, EstadoSolicitud.Pendiente);

        // Actuar
        var resultado = await servicio.RechazarAsync(solicitud.Id, admin, motivo!);

        // Verificar
        Assert.False(resultado.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Pendiente, guardada!.Estado);
    }

    // RF_GP_04: rechazar con motivo deja Rechazada, guarda el motivo y registra quién lo resolvió.
    [Fact]
    public async Task RF_GP_04_Rechazar_ConMotivo_QuedaRechazadaConMotivoYResueltoPor()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = CrearUsuario(db, 1, Rol.Administrador);
        var solicitante = CrearUsuario(db, 2, Rol.Estandar);
        var solicitud = CrearSolicitud(db, solicitante.Id, EstadoSolicitud.Pendiente);
        string motivo = "No cumple los requisitos";

        // Actuar
        var resultado = await servicio.RechazarAsync(solicitud.Id, admin, motivo);

        // Verificar
        Assert.True(resultado.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Rechazada, guardada!.Estado);
        Assert.Equal(motivo, guardada.MotivoRechazo);
        Assert.Equal(admin.Id, guardada.ResueltoPorId);
    }

    // RF_GP_01: CrearAsync deja una solicitud en estado Pendiente.
    [Fact]
    public async Task RF_GP_01_CrearAsync_DejaSolicitudPendiente()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);
        var solicitante = CrearUsuario(db, 1, Rol.Estandar);

        // Actuar
        var resultado = await servicio.CrearAsync(solicitante, Permiso.ExportarDatos);

        // Verificar
        Assert.True(resultado.Exito);
        var solicitud = await db.SolicitudesPermiso.FirstOrDefaultAsync();
        Assert.NotNull(solicitud);
        Assert.Equal(EstadoSolicitud.Pendiente, solicitud!.Estado);
        Assert.Equal(solicitante.Id, solicitud.SolicitanteId);
    }

    // RF_GP_02: un Estándar con una pendiente y otro Estándar con otra;
    // ListarPendientesAsync(estándar A) devuelve solo la de A.
    [Fact]
    public async Task RF_GP_02_Estandar_VeSoloSusPropiaSolicitudes()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var estandarA = CrearUsuario(db, 1, Rol.Estandar);
        var estandarB = CrearUsuario(db, 2, Rol.Estandar);
        var solicitudA = CrearSolicitud(db, estandarA.Id, EstadoSolicitud.Pendiente);
        var solicitudB = CrearSolicitud(db, estandarB.Id, EstadoSolicitud.Pendiente);

        // Actuar
        var listaA = await servicio.ListarPendientesAsync(estandarA);

        // Verificar
        Assert.Single(listaA);
        Assert.Equal(solicitudA.Id, listaA[0].Id);
    }

    // RF_GP_02: un Administrador ve las pendientes de todos y no ve las que no son Pendiente.
    [Fact]
    public async Task RF_GP_02_Administrador_VeTodasLasPendientesYNoLasOtras()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = CrearUsuario(db, 1, Rol.Administrador);
        var solicitanteA = CrearUsuario(db, 2, Rol.Estandar);
        var solicitanteB = CrearUsuario(db, 3, Rol.Estandar);

        var pendienteA = CrearSolicitud(db, solicitanteA.Id, EstadoSolicitud.Pendiente);
        var pendienteB = CrearSolicitud(db, solicitanteB.Id, EstadoSolicitud.Pendiente);
        var aprobada = CrearSolicitud(db, solicitanteA.Id, EstadoSolicitud.Aplicada);
        var rechazada = CrearSolicitud(db, solicitanteB.Id, EstadoSolicitud.Rechazada);

        // Actuar
        var lista = await servicio.ListarPendientesAsync(admin);

        // Verificar
        Assert.Equal(2, lista.Count);
        Assert.Contains(lista, s => s.Id == pendienteA.Id);
        Assert.Contains(lista, s => s.Id == pendienteB.Id);
        Assert.DoesNotContain(lista, s => s.Id == aprobada.Id);
        Assert.DoesNotContain(lista, s => s.Id == rechazada.Id);
    }

    // RF_GP_06: aprobar dos veces la misma solicitud; la segunda falla y el estado sigue Aplicada.
    [Fact]
    public async Task RF_GP_06_Aprobar_DosVecesLaMisma_LaSegundaFallaYSigueAplicada()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var admin = CrearUsuario(db, 1, Rol.Administrador);
        var solicitante = CrearUsuario(db, 2, Rol.Estandar);
        var solicitud = CrearSolicitud(db, solicitante.Id, EstadoSolicitud.Pendiente);

        // Actuar - primera aprobación (Pendiente -> Aprobada -> Aplicada dentro del servicio)
        var primera = await servicio.AprobarAsync(solicitud.Id, admin);
        // Actuar - segunda aprobación sobre la misma solicitud (ya Aplicada)
        var segunda = await servicio.AprobarAsync(solicitud.Id, admin);

        // Verificar
        Assert.True(primera.Exito);
        Assert.False(segunda.Exito);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Aplicada, guardada!.Estado);
    }

    // RF_GP_06: CambiarEstado(Aplicada) estando Rechazada devuelve false y el estado sigue Rechazada.
    [Fact]
    public async Task RF_GP_06_CambiarEstado_AplicadaDesdeRechazada_DevuelveFalseYSigueRechazada()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var solicitante = CrearUsuario(db, 1, Rol.Estandar);
        var solicitud = CrearSolicitud(db, solicitante.Id, EstadoSolicitud.Rechazada);

        // Actuar
        bool resultado = solicitud.CambiarEstado(EstadoSolicitud.Aplicada);

        // Verificar
        Assert.False(resultado);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Rechazada, guardada!.Estado);
    }

    // RF_GP_06: CambiarEstado(Pendiente) estando Aprobada devuelve false y el estado sigue Aprobada.
    [Fact]
    public async Task RF_GP_06_CambiarEstado_PendienteDesdeAprobada_DevuelveFalseYSigueAprobada()
    {
        // Preparar
        using var db = CrearContexto();
        var servicio = new ServicioPermisos(db);

        var solicitante = CrearUsuario(db, 1, Rol.Estandar);
        var solicitud = CrearSolicitud(db, solicitante.Id, EstadoSolicitud.Aprobada);

        // Actuar
        bool resultado = solicitud.CambiarEstado(EstadoSolicitud.Pendiente);

        // Verificar
        Assert.False(resultado);
        var guardada = await db.SolicitudesPermiso.FindAsync(solicitud.Id);
        Assert.Equal(EstadoSolicitud.Aprobada, guardada!.Estado);
    }

    // RF_GP_06: TransicionesSolicitud.EsPermitida solo es true para
    // Pendiente->Aprobada, Pendiente->Rechazada y Aprobada->Aplicada.
    [Fact]
    public void RF_GP_06_TransicionesSolicitud_SoloLasPermitidasSonVerdaderas()
    {
        // Todos los estados definidos en el enum.
        var estados = Enum.GetValues<EstadoSolicitud>();

        // Pares que DEBEN ser permitidos según el negocio.
        var permitidas = new (EstadoSolicitud Desde, EstadoSolicitud Hacia)[]
        {
            (EstadoSolicitud.Pendiente, EstadoSolicitud.Aprobada),
            (EstadoSolicitud.Pendiente, EstadoSolicitud.Rechazada),
            (EstadoSolicitud.Aprobada, EstadoSolicitud.Aplicada),
        };

        // Actuar / Verificar: recorre todos los pares (desde, hacia).
        foreach (var desde in estados)
        {
            foreach (var hacia in estados)
            {
                bool real = TransicionesSolicitud.EsPermitida(desde, hacia);
                bool esperado = permitidas.Any(p => p.Desde == desde && p.Hacia == hacia);
                Assert.True(esperado == real,
                    $"Transición {desde} -> {hacia}: esperado {esperado}, real {real}");
            }
        }
    }
}
