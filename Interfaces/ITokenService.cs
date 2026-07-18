using devalut.Models;

namespace devalut.Interfaces;

public interface ITokenService
{
    public string GenerateAccessToken(User user);
    
}
