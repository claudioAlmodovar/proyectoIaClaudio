namespace Backend.Contracts;

public sealed record CreateConsultaRequest(
    int MedicoId,
    int PacienteId,
    DateTime FechaConsulta,
    string? Sintomas,
    string? Recomendaciones,
    string? Diagnostico);

public sealed record ConsultaResponse(
    int Id,
    int MedicoId,
    int PacienteId,
    DateTime FechaConsulta,
    string? Sintomas,
    string? Recomendaciones,
    string? Diagnostico);
