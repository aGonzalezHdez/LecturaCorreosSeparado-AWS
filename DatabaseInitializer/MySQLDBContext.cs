using DatabaseInitializer.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInitializer;

public class MySQLDBContext :  DbContext
{
    public DbSet<Consultas> Consultas { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Comentarios> Comentarios { get; set; }
    public DbSet<Ejecutivo> Ejecutivos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            "Server=db-lecturacorreosaws.c4bkgguee5hw.us-east-1.rds.amazonaws.com;Database=DB-LecturaCorreosAWS;User=admin;Password=13wolfiinXP_;Port=3306;",
            new MySqlServerVersion(new Version(8, 0, 3)),  // 💡 Ajusta la versión de MySQL según la que usas en RDS
            options =>
            {
                options.EnableRetryOnFailure(  // 💡 Manejo de errores transitorios en AWS RDS
                    maxRetryCount: 5,          // 🔄 Intenta reconectar hasta 5 veces
                    maxRetryDelay: TimeSpan.FromSeconds(10),  // ⏳ Espera 10 segundos entre reintentos
                    errorNumbersToAdd: null
                );

                options.CommandTimeout(60);  // ⏳ Establece un tiempo límite de 60 segundos para consultas

                options.MigrationsAssembly("LecturaCorreosAWS");  // 📌 Asegura que las migraciones se generen en el ensamblado correcto
            }
        );

    }
    
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        // Datos iniciales para Cliente
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nombre = "POR ASIGNAR" },
            new Cliente { Id = 2, Nombre = "CLIENTE" },
            new Cliente { Id = 3, Nombre = "COM. EXT" },
            new Cliente { Id = 4, Nombre = "COMERCIALIZADORA" },
            new Cliente { Id = 5, Nombre = "COTIZACIONES PARA COMERCIAL" },
            new Cliente { Id = 6, Nombre = "DHL" },
            new Cliente { Id = 7, Nombre = "Ei CARGA" },
            new Cliente { Id = 8, Nombre = "IOM DE MEXICO" },
            new Cliente { Id = 9, Nombre = "MSL" }
        );

        // Datos iniciales para Comentarios
        modelBuilder.Entity<Comentarios>().HasData(
            new Comentarios { Id = 1, Descripcion = "SIN COMENTARIOS" },
            new Comentarios { Id = 2, Descripcion = "APOYO A OTRA ÁREA" },
            new Comentarios { Id = 3, Descripcion = "BÚSQUEDA DE INFORMACIÓN PARA CLASIFICAR" },
            new Comentarios { Id = 4, Descripcion = "CATÁLOGO DE PRODUCTOS" },
            new Comentarios { Id = 5, Descripcion = "DINÁMICAS" },
            new Comentarios { Id = 6, Descripcion = "FALLA DEL SISTEMA" },
            new Comentarios { Id = 7, Descripcion = "FALTA DE INFORMACIÓN DEL CLIENTE" },
            new Comentarios { Id = 8, Descripcion = "MANTENIMIENTO EQUIPO" },
            new Comentarios { Id = 9, Descripcion = "MISCELÁNEA" },
            new Comentarios { Id = 10, Descripcion = "REUNIÓN" },
            new Comentarios { Id = 11, Descripcion = "VACACIONES / INCAPACIDAD / VACANTE" }
        );

        // Datos iniciales para Ejecutivo
        modelBuilder.Entity<Ejecutivo>().HasData(
            new Ejecutivo { Id = 1, Nombre = "POR ASIGNAR", IdEmpleado = "000000" },
            new Ejecutivo { Id = 2, Nombre = "ALEJANDRA", IdEmpleado = "000000" },
            new Ejecutivo { Id = 3, Nombre = "ROSARIO", IdEmpleado = "000000" }
        );

    }
    
}