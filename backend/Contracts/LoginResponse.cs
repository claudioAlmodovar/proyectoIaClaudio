namespace Backend.Contracts;

public sealed record LoginResponse(string Token, DateTime Expiracion, UsuarioSummary Usuario);

public sealed record UsuarioSummary(int Id, string Correo, string NombreCompleto, int? MedicoId);
