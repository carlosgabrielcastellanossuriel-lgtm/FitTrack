using FitTrack.Core.Correos;
using FitTrack.Core.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Core.Datos;

public class ContextoBD : DbContext
{
    public ContextoBD(DbContextOptions<ContextoBD> options) : base(options)
    {
    }

    public DbSet<CorreoEnCola> CorreosEnCola => Set<CorreoEnCola>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TokenActivacion> TokensActivacion => Set<TokenActivacion>();
    public DbSet<Sesion> Sesiones => Set<Sesion>();

    public DbSet<TokenRecuperacion> TokensRecuperacion => Set<TokenRecuperacion>();

    // Entidades de otros proyectos (como Negocio) que Core no puede ver directamente.
    // La Api las agrega al arrancar y aquí se registran como tablas.
    public static List<Type> EntidadesNegocio { get; } = new List<Type>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CorreoEnCola>(e =>
        {
            e.Property(c => c.Destinatario).HasMaxLength(320);
            e.Property(c => c.Asunto).HasMaxLength(200);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Correo).HasMaxLength(320);
            e.Property(u => u.ContrasenaHash).HasMaxLength(100);

            // Índice único: la BASE DE DATOS rechaza dos usuarios con el mismo correo (RF-CA-01).
            // Es una segunda barrera además de la revisión que hace el código.
            e.HasIndex(u => u.Correo).IsUnique();
        });

        modelBuilder.Entity<TokenActivacion>(e =>
        {
            e.Property(t => t.Token).HasMaxLength(100);
            e.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<TokenRecuperacion>(e =>
        {
            e.Property(t => t.Codigo).HasMaxLength(100);
            e.HasIndex(t => t.Codigo).IsUnique();
        });

        foreach (Type tipo in EntidadesNegocio)
        {
            modelBuilder.Entity(tipo);   // "este tipo también es una tabla"
        }

    }
}