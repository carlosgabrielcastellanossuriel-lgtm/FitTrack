using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Permisos;
using FitTrack.Core.Usuarios;
using FitTrack.Negocio.Alimentos;
using FitTrack.Negocio.Perfiles;
using FitTrack.Negocio.Resumenes;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

// Le dice al contexto que ResumenDiario (del proyecto Negocio) tambi�n es una tabla.
ContextoBD.EntidadesNegocio.Add(typeof(ResumenDiario));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ContextoBD>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ServicioColaCorreo>();
builder.Services.AddScoped<ServicioRegistro>();
builder.Services.AddScoped<ServicioSesion>();
builder.Services.AddScoped<ServicioContrasena>();
builder.Services.AddScoped<ServicioAdministracion>();
builder.Services.AddScoped<ServicioPermisos>();
ContextoBD.EntidadesNegocio.Add(typeof(PerfilUsuario));// Le dice a EF que PerfilUsuario (del proyecto Negocio) tambi�n es una tabla.
ContextoBD.EntidadesNegocio.Add(typeof(Alimento));// Le dice a EF que Alimento (del proyecto Negocio) también es una tabla.
builder.Services.AddScoped<ServicioPerfil>();
builder.Services.AddScoped<ServicioAlimento>();
builder.Services.AddScoped<ServicioResumen>();

builder.Services.AddControllers();

                // SWAGGER

// Hace que los n�meros se describan solo como n�meros (no "n�mero o texto").
// As� Swagger UI reconoce bien los campos de la ruta, como el {id}.
builder.Services.ConfigureHttpJsonOptions(opciones =>
{
    opciones.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

// Genera el documento que describe los endpoints.
// El "transformer" le agrega la opci�n de mandar un token Bearer,
// para poder probar en Swagger los endpoints que exigen sesi�n.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Declara que existe un esquema de seguridad llamado "Bearer".
        // Esto hace aparecer el bot�n "Authorize" en la p�gina.
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            }
        };

        // Lo aplica a todos los endpoints: si guardas el token una vez,
        // Swagger lo manda en cada petici�n.
        foreach (var operacion in document.Paths.Values.SelectMany(ruta => ruta.Operations!))
        {
            operacion.Value.Security ??= [];
            operacion.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }

        return Task.CompletedTask;
    });
});
var app = builder.Build();

// Crea el primer Administrador si existen las variables de entorno.
using (IServiceScope scope = app.Services.CreateScope())
{
    ServicioAdministracion administracion = scope.ServiceProvider.GetRequiredService<ServicioAdministracion>();
    await administracion.CrearAdministradorInicialAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // La p�gina de Swagger. Solo en modo Development.
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "FitTrack v1");
    });
}

// Manejador global de errores (RD-08).
// Envuelve toda petici�n: si algo lanza una excepci�n que nadie atrap�,
// el usuario recibe un mensaje gen�rico (500) y NUNCA la traza ni la consulta SQL.
// El detalle real solo se escribe en la consola del servidor, para quien administra.
app.Use(async (context, next) =>
{
    try
    {
        await next();   // contin�a con el resto del proceso normal
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error no controlado en {Ruta}", context.Request.Path);

        // Si la respuesta ya empez� a enviarse no se puede cambiar.
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Ocurri� un error inesperado. Intenta de nuevo m�s tarde." });
        }
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();