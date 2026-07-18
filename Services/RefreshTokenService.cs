using System.Security.Cryptography;
using devalut.Entities.Persistance;
using devalut.Interfaces;
using devalut.Models;
using Microsoft.EntityFrameworkCore;

namespace devalut.Services;

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

        var refreshToken = new RefreshToken(
            userId,
            token,
            _expirationTime
        );

        _context.RefreshTokens.Add(refreshToken);

        return refreshToken;
    }


    public RefreshToken? GetByToken(string token)
    {
        return _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefault(r => r.Token == token);
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
}