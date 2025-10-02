namespace Backend.Models;

public class Usuario
{
    public int IdUsuarios { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
