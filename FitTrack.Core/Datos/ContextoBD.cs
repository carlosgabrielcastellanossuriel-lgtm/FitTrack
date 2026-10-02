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

    //Aqui iran las tablas de la base de datos

}
