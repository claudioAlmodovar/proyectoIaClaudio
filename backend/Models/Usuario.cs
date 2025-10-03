using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("correo")]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [Column("password")]
    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column("nombre_completo")]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Column("id_medico")]
    public int? MedicoId { get; set; }

    [Column("activo")]
    public bool Activo { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    public Medico? Medico { get; set; }
}
