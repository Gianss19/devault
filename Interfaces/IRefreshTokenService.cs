using devalut.Models;

namespace devalut.Interfaces;

public interface IRefreshTokenService
{
    RefreshToken Create(Guid userId);

    RefreshToken? GetByToken(string token);

    bool IsValid(RefreshToken token);

    void Revoke(RefreshToken token);
}
