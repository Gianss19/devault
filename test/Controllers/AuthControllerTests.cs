using System.Security.Claims;
using devault.Controllers;
using devault.Data.Configuration;
using devault.DTO;
using devault.DTO.RefreshTokens;
using devault.DTO.Users;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models;
using devault.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace test.Controllers;

public class AuthControllerTests
{
    private readonly DevaultDbContext _context;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<DevaultDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new DevaultDbContext(options);

        var jwtService = new devault.Services.JwtService(
            Options.Create(new JwtSettings
            {
                SecretKey = "test_key_that_is_at_least_32_chars_long!",
                Issuer = "devault",
                Audience = "devault"
            }));

        var bcryptService = new devault.Services.BcryptService();

        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        refreshTokenServiceMock.Setup(s => s.Create(It.IsAny<Guid>()))
            .Returns((Guid userId) => new RefreshToken(userId, "test_refresh_token_hash", "test_refresh_token", TimeSpan.FromDays(7)));
        refreshTokenServiceMock.Setup(s => s.GetByToken(It.IsAny<string>()))
            .Returns((string token) => _context.RefreshTokens.FirstOrDefault(t => t.Token == token));

        _controller = new AuthController(_context, jwtService, refreshTokenServiceMock.Object, bcryptService);
    }

    private void SetAdminUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }))
            }
        };
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithUserResponseDto()
    {
        var dto = new UserRegisterDto("TestUser", "test@example.com", "Test@1234");

        var result = await _controller.Register(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);
        Assert.Equal("TestUser", response.Name);
        Assert.Equal("test@example.com", response.Email);
    }

    [Fact]
    public async Task Register_WithNullUser_ReturnsBadRequest()
    {
        var result = await _controller.Register(null);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User is null", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        _context.Users.Add(new User("ExistingUser", "duplicate@example.com", "hash"));
        await _context.SaveChangesAsync();

        var dto = new UserRegisterDto("AnotherUser", "duplicate@example.com", "Test@1234");

        var result = await _controller.Register(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_WithDuplicateName_ReturnsBadRequest()
    {
        _context.Users.Add(new User("DuplicateName", "first@example.com", "hash"));
        await _context.SaveChangesAsync();

        var dto = new UserRegisterDto("DuplicateName", "second@example.com", "Test@1234");

        var result = await _controller.Register(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task RegisterAdmin_WithValidData_ReturnsOkWithAdminRole()
    {
        SetAdminUser();

        var dto = new AdminRegisterDto("NewAdmin", "admin@example.com", "Admin@1234");

        var result = await _controller.RegisterAdmin(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserResponseDto>(okResult.Value);
        Assert.Equal("NewAdmin", response.Name);

        var createdUser = await _context.Users.FindAsync(response.id);
        Assert.NotNull(createdUser);
        Assert.Equal(Roles.Admin, createdUser!.Rol);
    }

    [Fact]
    public async Task RegisterAdmin_WithNullUser_ReturnsBadRequest()
    {
        SetAdminUser();

        var result = await _controller.RegisterAdmin(null);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User is null", badRequestResult.Value);
    }

    [Fact]
    public async Task RegisterAdmin_WithDuplicateEmail_ReturnsBadRequest()
    {
        SetAdminUser();

        _context.Users.Add(new User("ExistingUser", "admin@example.com", "hash"));
        await _context.SaveChangesAsync();

        var dto = new AdminRegisterDto("NewAdmin", "admin@example.com", "Admin@1234");

        var result = await _controller.RegisterAdmin(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokenResponseDto()
    {
        var bcryptService = new devault.Services.BcryptService();
        var hash = bcryptService.GenerateHash("Test@1234");
        var user = new User("TestUser", "test@example.com", hash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new UserAuthRequestDto("test@example.com", "Test@1234");

        var result = await _controller.Login(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TokenResponseDto>(okResult.Value);
        Assert.False(string.IsNullOrEmpty(response.AccessToken));
        Assert.False(string.IsNullOrEmpty(response.RefreshToken));
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsBadRequest()
    {
        var dto = new UserAuthRequestDto("nonexistent@example.com", "Test@1234");

        var result = await _controller.Login(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Credentials.", badRequestResult.Value);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        var bcryptService = new devault.Services.BcryptService();
        var hash = bcryptService.GenerateHash("Test@1234");
        var user = new User("TestUser", "test@example.com", hash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new UserAuthRequestDto("test@example.com", "Wrong@1234");

        var result = await _controller.Login(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Credentials.", badRequestResult.Value);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        var dto = new UserAuthRequestDto("test@example.com", "");

        var result = await _controller.Login(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Credentials.", badRequestResult.Value);
    }

    [Fact]
    public async Task Logout_WithValidToken_RevokesToken()
    {
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        var refresh = new RefreshToken(Guid.NewGuid(), "valid_token_hash", "valid_token", TimeSpan.FromDays(7));
        refreshTokenServiceMock.Setup(s => s.GetByToken("valid_token")).Returns(refresh);

        var jwtService = new devault.Services.JwtService(
            Options.Create(new JwtSettings
            {
                SecretKey = "test_key_that_is_at_least_32_chars_long!",
                Issuer = "devault",
                Audience = "devault"
            }));
        var bcryptService = new devault.Services.BcryptService();

        var controller = new AuthController(_context, jwtService, refreshTokenServiceMock.Object, bcryptService);

        var dto = new RefreshTokenRequestDto("valid_token");

        var result = await controller.Logout(dto);

        Assert.IsType<OkResult>(result);
        refreshTokenServiceMock.Verify(s => s.Revoke(refresh), Times.Once);
    }

    [Fact]
    public async Task Logout_WithInvalidToken_ReturnsBadRequest()
    {
        var refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        refreshTokenServiceMock.Setup(s => s.GetByToken("invalid_token")).Returns((RefreshToken?)null);

        var jwtService = new devault.Services.JwtService(
            Options.Create(new JwtSettings
            {
                SecretKey = "test_key_that_is_at_least_32_chars_long!",
                Issuer = "devault",
                Audience = "devault"
            }));
        var bcryptService = new devault.Services.BcryptService();

        var controller = new AuthController(_context, jwtService, refreshTokenServiceMock.Object, bcryptService);

        var dto = new RefreshTokenRequestDto("invalid_token");

        var result = await controller.Logout(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Token inválido o ya revocado.", badRequestResult.Value);
    }
}
