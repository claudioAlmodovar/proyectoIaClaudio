using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Backend.Services;

public sealed class PasswordService : IPasswordService
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrEmpty(providedPassword))
        {
            return false;
        }

        var parts = hashedPassword.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var storedHashBytes = Convert.FromBase64String(parts[1]);

            var computedHash = KeyDerivation.Pbkdf2(providedPassword, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
