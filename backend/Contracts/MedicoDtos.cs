namespace Backend.Contracts;

public sealed record CreateMedicoRequest(
    string PrimerNombre,
    string? SegundoNombre,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string Cedula,
    string? Telefono,
    string? Especialidad,
    string? Email,
    bool Activo);

public sealed record MedicoResponse(
    int Id,
    string PrimerNombre,
    string? SegundoNombre,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string Cedula,
    string? Telefono,
    string? Especialidad,
    string? Email,
    bool Activo,
    DateTime FechaCreacion);
