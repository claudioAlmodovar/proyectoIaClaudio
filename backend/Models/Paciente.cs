using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("pacientes")]
public class Paciente
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

    [Column("telefono")]
    [MaxLength(20)]
    public string? Telefono { get; set; }

    [Column("activo")]
    public bool Activo { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}
