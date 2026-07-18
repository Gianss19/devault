using devault.Models;

namespace devault.Interfaces;

public interface ITokenService
{
    public string GenerateAccessToken(User user);
    
}
