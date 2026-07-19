using System.Security.Cryptography;
using System.Text;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models;
using Microsoft.EntityFrameworkCore;

namespace devault.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly DevaultDbContext _context;

    private readonly TimeSpan _expirationTime = TimeSpan.FromDays(7);

    public RefreshTokenService(DevaultDbContext context)
    {
        _context = context;
    }


    public RefreshToken Create(Guid userId)
    {
        var token = GenerateToken();
        var tokenHash = HashToken(token);

        var refreshToken = new RefreshToken(
            userId,
            tokenHash,
            token,
            _expirationTime
        );

        _context.RefreshTokens.Add(refreshToken);

        return refreshToken;
    }


    public RefreshToken? GetByToken(string token)
    {
        var tokenHash = HashToken(token);
        return _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefault(r => r.Token == tokenHash);
    }


    public bool IsValid(RefreshToken token)
    {
        return token.IsActive;
    }


    public void Revoke(RefreshToken token)
    {
        token.Revoke();
    }


    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}