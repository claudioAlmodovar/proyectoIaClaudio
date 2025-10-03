using System.Threading;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public sealed class ConsultorioDbContext : DbContext
{
    public ConsultorioDbContext(DbContextOptions<ConsultorioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public Task<Usuario?> FindUsuarioAsync(string normalizedUsuario, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedUsuario))
        {
            return Task.FromResult<Usuario?>(null);
        }

        var normalized = normalizedUsuario.Trim().ToLowerInvariant();

        return Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.NombreUsuario.ToLower() == normalized, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(usuario => usuario.IdUsuarios);

            entity.Property(usuario => usuario.IdUsuarios)
                  .HasColumnName("idUsuarios");

            entity.Property(usuario => usuario.NombreUsuario)
                  .HasColumnName("nombreUsuario")
                  .HasMaxLength(100);

            entity.Property(usuario => usuario.Nombre)
                  .HasColumnName("nombre")
                  .HasMaxLength(150);

            entity.Property(usuario => usuario.Contrasena)
                  .HasColumnName("contrasena")
                  .HasMaxLength(255);

            entity.Property(usuario => usuario.Activo)
                  .HasColumnName("activo");
        });
    }
}
