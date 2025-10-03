using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("consultas")]
public class Consulta
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_medico")]
    public int MedicoId { get; set; }

    [Column("id_paciente")]
    public int PacienteId { get; set; }

    [Column("fecha_consulta")]
    public DateTime FechaConsulta { get; set; }

    [Column("sintomas")]
    [MaxLength(500)]
    public string? Sintomas { get; set; }

    [Column("recomendaciones")]
    [MaxLength(500)]
    public string? Recomendaciones { get; set; }

    [Column("diagnostico")]
    [MaxLength(500)]
    public string? Diagnostico { get; set; }

    public Medico? Medico { get; set; }

    public Paciente? Paciente { get; set; }
}
