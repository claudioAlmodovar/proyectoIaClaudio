namespace Backend.Contracts;

public sealed record CreatePacienteRequest(
    string PrimerNombre,
    string? SegundoNombre,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string? Telefono,
    bool Activo);

public sealed record PacienteResponse(
    int Id,
    string PrimerNombre,
    string? SegundoNombre,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string? Telefono,
    bool Activo,
    DateTime FechaCreacion);
