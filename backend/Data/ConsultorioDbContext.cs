using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class ConsultorioDbContext(DbContextOptions<ConsultorioDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Consulta> Consultas => Set<Consulta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasIndex(m => m.Cedula).IsUnique();
            entity.HasIndex(m => m.Email).IsUnique();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Correo).IsUnique();

            entity.HasOne(u => u.Medico)
                  .WithMany(m => m.Usuarios)
                  .HasForeignKey(u => u.MedicoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Paciente>();

        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.HasOne(c => c.Medico)
                  .WithMany(m => m.Consultas)
                  .HasForeignKey(c => c.MedicoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Paciente)
                  .WithMany(p => p.Consultas)
                  .HasForeignKey(c => c.PacienteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
