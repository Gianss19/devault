using System.Text.RegularExpressions;
using devault.Interfaces;

namespace devault.Services;

public class BcryptService : IHasherService
{
    private const string PasswordPattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{8,}$";

    public string GenerateHash(string Password)
    {
        if(!Regex.IsMatch(Password, PasswordPattern))
            throw new FormatException("Contraseña No segura.");

        return BCrypt.Net.BCrypt.HashPassword(Password);
    }

    public bool IsPasswordValid(string Password, string PasswordHash)
    {
        return BCrypt.Net.BCrypt.Verify(Password, PasswordHash);
    }
}
