using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Backend.Options;
using Backend.Services.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public TokenResult GenerateToken(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var expiration = DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new("name", usuario.NombreCompleto)
        };

        if (usuario.MedicoId.HasValue)
        {
            claims.Add(new Claim("medicoId", usuario.MedicoId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.WriteToken(token);

        return new TokenResult(jwt, expiration);
    }
}
