namespace Backend.Contracts;

public sealed record CreateUsuarioRequest(string Correo, string Password, string NombreCompleto, int? MedicoId, bool Activo);

public sealed record UsuarioResponse(int Id, string Correo, string NombreCompleto, int? MedicoId, bool Activo, DateTime FechaCreacion);
