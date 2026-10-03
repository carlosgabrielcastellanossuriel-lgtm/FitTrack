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
    }
}