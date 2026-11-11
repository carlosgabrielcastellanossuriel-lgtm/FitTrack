using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Alimentos;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitTrack.Tests;

public class UsdaTests
{
    // Registra Alimento como tabla del contexto antes de que corran las pruebas.
    [ModuleInitializer]
    public static void RegistrarEntidadesNegocio() { ContextoBD.EntidadesNegocio.Add(typeof(Alimento)); }

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

    // Handler falso: devuelve la respuesta indicada o lanza HttpRequestException.
    // Nunca se llama a internet.
    private sealed class HandlerFalso : HttpMessageHandler
    {
        private readonly HttpResponseMessage? _respuesta;
        private readonly bool _lanzarError;

        public HandlerFalso(HttpStatusCode estado, string json)
        {
            _respuesta = new HttpResponseMessage(estado)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        public HandlerFalso()
        {
            _lanzarError = true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_lanzarError)
                throw new HttpRequestException("La API está caída.");

            return Task.FromResult(_respuesta!);
        }
    }

    // Crea el servicio con el handler y la clave indicados.
    private static ServicioUsda CrearServicio(ContextoBD db, HttpMessageHandler handler, string clave)
    {
        HttpClient http = new HttpClient(handler);
        ServicioAlimento alimentos = new ServicioAlimento(db);
        return new ServicioUsda(http, alimentos) { Clave = clave };
    }

    // RF_API_01: sin clave configurada no se consulta nada y no se guarda ningún alimento.
    [Fact]
    public async Task RF_API_01_SinClave_FallaYNoCreaAlimento()
    {
        // Preparar
        using var db = CrearContexto();
        var usuario = CrearUsuario(db, 1, Rol.Estandar);
        var servicio = CrearServicio(db, new HandlerFalso(), "");

        // Actuar
        var resultado = await servicio.ImportarAsync(usuario, "banana");

        // Verificar
        Assert.False(resultado.Exito);
        Assert.Equal("La integración con USDA no está configurada: falta la clave FITTRACK_USDA_CLAVE.", resultado.Mensaje);
        Assert.False(await db.Set<Alimento>().AnyAsync());
    }

    // RF_API_02: si la API lanza un error de red, se devuelve un mensaje controlado sin excepción.
    [Fact]
    public async Task RF_API_02_ApiCaida_FallaConMensajeControlado()
    {
        // Preparar
        using var db = CrearContexto();
        var usuario = CrearUsuario(db, 1, Rol.Estandar);
        var servicio = CrearServicio(db, new HandlerFalso(), "clave-de-prueba");

        // Actuar
        var resultado = await servicio.ImportarAsync(usuario, "banana");

        // Verificar
        Assert.False(resultado.Exito);
        Assert.Equal("No se pudo consultar USDA. Intenta de nuevo más tarde.", resultado.Mensaje);
        Assert.False(await db.Set<Alimento>().AnyAsync());
    }

    // RF_API_03: una respuesta 403 indica que la clave no es válida.
    [Fact]
    public async Task RF_API_03_ClaveRechazada_MuestraMensajeDeClaveInvalida()
    {
        // Preparar
        using var db = CrearContexto();
        var usuario = CrearUsuario(db, 1, Rol.Estandar);
        var servicio = CrearServicio(db, new HandlerFalso(HttpStatusCode.Forbidden, "{}"), "clave-mala");

        // Actuar
        var resultado = await servicio.ImportarAsync(usuario, "banana");

        // Verificar
        Assert.False(resultado.Exito);
        Assert.Equal("La clave de USDA no es válida.", resultado.Mensaje);
        Assert.False(await db.Set<Alimento>().AnyAsync());
    }

    // RF_API_04: si USDA no devuelve alimentos, se avisa y no se guarda nada.
    [Fact]
    public async Task RF_API_04_SinResultados_FallaConMensajeDeNoEncontrado()
    {
        // Preparar
        using var db = CrearContexto();
        var usuario = CrearUsuario(db, 1, Rol.Estandar);
        var servicio = CrearServicio(db, new HandlerFalso(HttpStatusCode.OK, "{ \"foods\": [] }"), "clave-de-prueba");

        // Actuar
        var resultado = await servicio.ImportarAsync(usuario, "banana");

        // Verificar
        Assert.False(resultado.Exito);
        Assert.Equal("No se encontró ese alimento en USDA.", resultado.Mensaje);
        Assert.False(await db.Set<Alimento>().AnyAsync());
    }

    // RF_API_05: con una respuesta válida se crea el alimento personalizado con sus valores por 100 g.
    [Fact]
    public async Task RF_API_05_RespuestaValida_CreaAlimentoConValoresYUsuario()
    {
        // Preparar
        using var db = CrearContexto();
        var usuario = CrearUsuario(db, 7, Rol.Estandar);

        string json = """
        {
          "foods": [
            {
              "description": "Banana, raw",
              "foodNutrients": [
                { "nutrientNumber": "208", "value": 89 },
                { "nutrientNumber": "203", "value": 1.09 },
                { "nutrientNumber": "205", "value": 22.84 },
                { "nutrientNumber": "204", "value": 0.33 }
              ]
            }
          ]
        }
        """;

        var servicio = CrearServicio(db, new HandlerFalso(HttpStatusCode.OK, json), "clave-de-prueba");

        // Actuar
        var resultado = await servicio.ImportarAsync(usuario, "banana");

        // Verificar
        Assert.True(resultado.Exito);
        var alimento = await db.Set<Alimento>().SingleAsync();
        Assert.Equal("Banana, raw", alimento.Nombre);
        Assert.Equal(89, alimento.CaloriasPor100g);
        Assert.Equal(1.09, alimento.ProteinaPor100g);
        Assert.Equal(22.84, alimento.CarbohidratosPor100g);
        Assert.Equal(0.33, alimento.GrasasPor100g);
        Assert.Equal(usuario.Id, alimento.UsuarioCreadorId);
    }
}
