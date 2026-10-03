using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ContextoBD>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ServicioColaCorreo>();
builder.Services.AddScoped<ServicioRegistro>();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
} 

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();