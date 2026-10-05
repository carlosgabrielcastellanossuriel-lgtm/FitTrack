using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;
using FitTrack.Negocio.Resumenes;
using Microsoft.OpenApi;

// Le dice al contexto que ResumenDiario (del proyecto Negocio) también es una tabla.
ContextoBD.EntidadesNegocio.Add(typeof(ResumenDiario));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ContextoBD>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ServicioColaCorreo>();
builder.Services.AddScoped<ServicioRegistro>();
builder.Services.AddScoped<ServicioSesion>();
builder.Services.AddScoped<ServicioContrasena>();
builder.Services.AddScoped<ServicioAdministracion>();

builder.Services.AddControllers();

                    // SWAGGER

// Genera el documento que describe los endpoints.
// El "transformer" le agrega la opción de mandar un token Bearer,
// para poder probar en Swagger los endpoints que exigen sesión.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Declara que existe un esquema de seguridad llamado "Bearer".
        // Esto hace aparecer el botón "Authorize" en la página.
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
        // Swagger lo manda en cada petición.
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

    // La página de Swagger. Solo en modo Development.
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "FitTrack v1");
    });
}

// Manejador global de errores (RD-08).
// Envuelve toda petición: si algo lanza una excepción que nadie atrapó,
// el usuario recibe un mensaje genérico (500) y NUNCA la traza ni la consulta SQL.
// El detalle real solo se escribe en la consola del servidor, para quien administra.
app.Use(async (context, next) =>
{
    try
    {
        await next();   // continúa con el resto del proceso normal
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error no controlado en {Ruta}", context.Request.Path);

        // Si la respuesta ya empezó a enviarse no se puede cambiar.
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Ocurrió un error inesperado. Intenta de nuevo más tarde." });
        }
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();