using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;
using FitTrack.Negocio.Resumenes;
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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();

// Crea el primer Administrador si existen las variables de entorno (ver README).
using (IServiceScope scope = app.Services.CreateScope())
{
    ServicioAdministracion administracion = scope.ServiceProvider.GetRequiredService<ServicioAdministracion>();
    await administracion.CrearAdministradorInicialAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
} 

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();