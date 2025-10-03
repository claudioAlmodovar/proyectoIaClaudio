using Backend.Models;
using Backend.Services.Models;

namespace Backend.Services;

public interface IJwtTokenService
{
    TokenResult GenerateToken(Usuario usuario);
}
