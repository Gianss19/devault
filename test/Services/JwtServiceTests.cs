using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using devault.Data.Configuration;
using devault.Models;
using devault.Models.Enums;
using devault.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace test.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _jwtService = new JwtService(
            Options.Create(new JwtSettings
            {
                SecretKey = "test_key_that_is_at_least_32_chars_long!",
                Issuer = "test",
                Audience = "test"
            }));
    }

    [Fact]
    public void GenerateAccessToken_ReturnsNonEmptyString()
    {
        var user = new User("TestUser", "test@example.com", "hash");

        var token = _jwtService.GenerateAccessToken(user);

        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public void GenerateAccessToken_ContainsCorrectClaims()
    {
        var user = new User("TestUser", "test@example.com", "hash");

        var tokenString = _jwtService.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        Assert.Contains(jwtToken.Claims, c => c.Type == "nameid" && c.Value == user.Id.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == "unique_name" && c.Value == "TestUser");
        Assert.Contains(jwtToken.Claims, c => c.Type == "email" && c.Value == "test@example.com");
        Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == Roles.User.ToString());
    }

    [Fact]
    public void GenerateAccessToken_TokenCanBeValidated()
    {
        var user = new User("TestUser", "test@example.com", "hash");

        var tokenString = _jwtService.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test_key_that_is_at_least_32_chars_long!")),
            ValidateIssuer = true,
            ValidIssuer = "test",
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(tokenString, validationParameters, out _);

        Assert.NotNull(principal);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void GenerateAccessToken_ThrowsWhenSecretKeyIsEmpty()
    {
        var service = new JwtService(
            Options.Create(new JwtSettings
            {
                SecretKey = "",
                Issuer = "test",
                Audience = "test"
            }));

        var user = new User("TestUser", "test@example.com", "hash");

        Assert.Throws<KeyNotFoundException>(() => service.GenerateAccessToken(user));
    }

    [Fact]
    public void GenerateAccessToken_ThrowsWhenIssuerIsEmpty()
    {
        var service = new JwtService(
            Options.Create(new JwtSettings
            {
                SecretKey = "test_key_that_is_at_least_32_chars_long!",
                Issuer = "",
                Audience = "test"
            }));

        var user = new User("TestUser", "test@example.com", "hash");

        Assert.Throws<KeyNotFoundException>(() => service.GenerateAccessToken(user));
    }
}
