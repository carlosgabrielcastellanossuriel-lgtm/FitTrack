using FitTrack.Core.Correos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FitTrack.Core.Datos;

public class ContextoBD : DbContext
{

    public ContextoBD(DbContextOptions<ContextoBD> opciones) : base(opciones)
    {
        
    }

    public DbSet<CorreoEnCola> CorreosEnCola => Set<CorreoEnCola>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) // con este metodo configuramos las propiedades de la entidad CorreoEnCola.
    {
        modelBuilder.Entity<CorreoEnCola>(e => // le decimos que los campos destinatario y asunto tienen un tamaño maximo de 320 y 200 respectivamente. No le ponemos limite al cuerpo para que se cree como un varchar(max)
        {
            e.Property(c => c.Destinatario).HasMaxLength(320);
            e.Property(c => c.Asunto).HasMaxLength(200);
        });
    }
}
