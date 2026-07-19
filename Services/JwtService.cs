using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using devault.Data.Configuration;
using devault.Interfaces;
using devault.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace devault.Services;

public class JwtService : ITokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IOptions<JwtSettings> options)
    {
        _key = options.Value.SecretKey;
        _issuer = options.Value.Issuer;
        _audience = options.Value.Audience;

    }
    public string GenerateAccessToken(User user)
    {
        if(string.IsNullOrEmpty(_key))
            throw new KeyNotFoundException("SecretKey no encontrada.");
        if(string.IsNullOrEmpty(_issuer))
            throw new KeyNotFoundException("issuer no encontrada.");
        if(string.IsNullOrEmpty(_audience))
            throw new KeyNotFoundException("Audience no encontrada.");
            
        byte[] keyBytes = Encoding.UTF8.GetBytes(_key);
        var key = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
          new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
          new Claim(ClaimTypes.Name, user.Name),
          new Claim(ClaimTypes.Email, user.Email),
          new Claim(ClaimTypes.Role, user.Rol.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _issuer,
            Audience = _audience,
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        var token = tokenHandler.CreateToken(tokenDescriptor);



        return tokenHandler.WriteToken(token);
    }

}
