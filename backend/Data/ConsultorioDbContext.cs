using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class ConsultorioDbContext : DbContext
{
    public ConsultorioDbContext(DbContextOptions<ConsultorioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                IdUsuarios = 1,
                Usuario = "recepcion",
                Nombre = "Laura Sánchez",
                Contrasena = "recepcion123",
                Activo = true
            },
            new Usuario
            {
                IdUsuarios = 2,
                Usuario = "doctor1",
                Nombre = "Dr. Jorge Medina",
                Contrasena = "consulta2024",
                Activo = true
            },
            new Usuario
            {
                IdUsuarios = 3,
                Usuario = "admin",
                Nombre = "Administración",
                Contrasena = "admin2024",
                Activo = false
            });
    }
}
