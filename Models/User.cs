namespace devalut.Models;

using System.Text.RegularExpressions;

using devalut;
using devalut.Models.Enums;

public class User
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public Roles Rol { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public ICollection<Secret> Secrets { get; private set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    private User() { }

    public User(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EntityException("Nombre inválido.");

        name = name.Trim();

        if (name.Length is < 3 or > 100)
            throw new EntityException("El nombre debe tener entre 3 y 100 caracteres.");

        if (string.IsNullOrWhiteSpace(email))
            throw new EntityException("Correo inválido.");

        email = email.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(email,
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            throw new EntityException("Formato de correo inválido.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new EntityException("Hash inválido.");

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Rol = Roles.User;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new EntityException("Nombre inválido.");

        name = name.Trim();

        if (name.Length is < 3 or > 100)
            throw new EntityException("El nombre debe tener entre 3 y 100 caracteres.");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new EntityException("Hash inválido.");

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(Roles role)
    {
        Rol = role;
        UpdatedAt = DateTime.UtcNow;
    }
}