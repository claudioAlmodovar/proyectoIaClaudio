using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("medicos")]
public class Medico
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("primer_nombre")]
    [MaxLength(100)]
    public string PrimerNombre { get; set; } = string.Empty;

    [Column("segundo_nombre")]
    [MaxLength(100)]
    public string? SegundoNombre { get; set; }

    [Required]
    [Column("apellido_paterno")]
    [MaxLength(100)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Column("apellido_materno")]
    [MaxLength(100)]
    public string? ApellidoMaterno { get; set; }

    [Required]
    [Column("cedula")]
    [MaxLength(50)]
    public string Cedula { get; set; } = string.Empty;

    [Column("telefono")]
    [MaxLength(20)]
    public string? Telefono { get; set; }

    [Column("especialidad")]
    [MaxLength(150)]
    public string? Especialidad { get; set; }

    [Column("email")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Column("activo")]
    public bool Activo { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}
