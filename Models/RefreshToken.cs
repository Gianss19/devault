using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace devalut.Models;

public class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string token, TimeSpan lifetime)
    {
        if (userId == Guid.Empty)
            throw new EntityException("Usuario inválido.");

        if (string.IsNullOrWhiteSpace(token))
            throw new EntityException("Token inválido.");

        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.Add(lifetime);
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt != null;

    public bool IsActive => !IsExpired && !IsRevoked;

    public void Revoke()
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
    }
}
