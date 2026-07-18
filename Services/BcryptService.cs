using devault.Interfaces;
using BCrypt.Net;

namespace devault.Services;

public class BcryptService : IHasherService
{
    public string GenerateHash(string Password)
    {
        var HashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
        return HashedPassword;
    }

    public bool IsPasswordValid(string Password, string PasswordHash)
    {
        var isValid = BCrypt.Net.BCrypt.Verify(Password, PasswordHash);
        return isValid;
    }
}
