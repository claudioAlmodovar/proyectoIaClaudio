using Backend.Models;

namespace Backend.Data;

public sealed class ConsultorioDbContext
{
    private readonly IReadOnlyDictionary<string, Usuario> _usuarios;

    public ConsultorioDbContext()
    {
        _usuarios = SeedUsuarios()
            .ToDictionary(
                usuario => usuario.NombreUsuario.Trim().ToLowerInvariant(),
                usuario => usuario,
                StringComparer.Ordinal);
    }

    public Usuario? FindUsuario(string normalizedUsuario)
    {
        if (string.IsNullOrWhiteSpace(normalizedUsuario))
        {
            return null;
        }

        _usuarios.TryGetValue(normalizedUsuario, out var usuario);
        return usuario;
    }

    private static IEnumerable<Usuario> SeedUsuarios()
    {
        yield return new Usuario
        {
            IdUsuarios = 1,
            NombreUsuario = "recepcion",
            Nombre = "Laura Sánchez",
            Contrasena = "recepcion123",
            Activo = true
        };

        yield return new Usuario
        {
            IdUsuarios = 2,
            NombreUsuario = "doctor1",
            Nombre = "Dr. Jorge Medina",
            Contrasena = "consulta2024",
            Activo = true
        };

        yield return new Usuario
        {
            IdUsuarios = 3,
            NombreUsuario = "admin",
            Nombre = "Administración",
            Contrasena = "admin2024",
            Activo = false
        };
    }
}
