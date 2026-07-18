using System.Security.Claims;
using devault.Controllers;
using devault.DTO.Secrets;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace test.Controllers;

public class SecretsControllerTests
{
    private readonly DevaultDbContext _context;
    private readonly SecretsController _controller;
    private readonly Guid _userId;

    public SecretsControllerTests()
    {
        var options = new DbContextOptionsBuilder<DevaultDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new DevaultDbContext(options);

        _userId = Guid.NewGuid();

        var encryptServiceMock = new Mock<IEncryptService>();
        encryptServiceMock.Setup(e => e.Encrypt(It.IsAny<string>()))
            .Returns((string s) => "encrypted_" + s);

        _controller = new SecretsController(encryptServiceMock.Object, _context);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
                }))
            }
        };
    }

    [Fact]
    public async Task Generate_WithValidData_ReturnsCreated()
    {
        var dto = new SecretRequestDto("MySecret", "my_value");

        var result = await _controller.Generate(dto);

        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public async Task Generate_WithEmptyName_ReturnsBadRequest()
    {
        var dto = new SecretRequestDto("", "my_value");

        var result = await _controller.Generate(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("El nombre no puede estar vacío.", badRequest.Value);
    }

    [Fact]
    public async Task Generate_WithEmptyValue_ReturnsBadRequest()
    {
        var dto = new SecretRequestDto("MySecret", "");

        var result = await _controller.Generate(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("El valor del secreto no puede estar vacío.", badRequest.Value);
    }

    [Fact]
    public async Task AllSecretsPerUserId_ReturnsUserSecrets()
    {
        _context.Secrets.Add(new Secret("Secret1", "encrypted_val1", _userId));
        _context.Secrets.Add(new Secret("Secret2", "encrypted_val2", _userId));
        await _context.SaveChangesAsync();

        var result = await _controller.AllSecretsPerUserId();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var secrets = Assert.IsAssignableFrom<IReadOnlyList<SecretResponseDto>>(okResult.Value);
        Assert.Equal(2, secrets.Count);
    }

    [Fact]
    public async Task AllSecretsPerUserId_DoesNotReturnOtherUsersSecrets()
    {
        _context.Secrets.Add(new Secret("MySecret", "encrypted_val1", _userId));
        _context.Secrets.Add(new Secret("OtherSecret", "encrypted_val2", Guid.NewGuid()));
        await _context.SaveChangesAsync();

        var result = await _controller.AllSecretsPerUserId();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var secrets = Assert.IsAssignableFrom<IReadOnlyList<SecretResponseDto>>(okResult.Value);
        Assert.Single(secrets);
        Assert.Equal("MySecret", secrets[0].Name);
    }

    [Fact]
    public async Task GetSecretById_ReturnsSecretWhenOwnedByUser()
    {
        var secret = new Secret("MySecret", "encrypted_val", _userId);
        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();

        var result = await _controller.GetSecretById(secret.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SecretResponseDto>(okResult.Value);
        Assert.Equal("MySecret", response.Name);
    }

    [Fact]
    public async Task GetSecretById_ReturnsNotFoundWhenNotOwnedByUser()
    {
        var secret = new Secret("OtherSecret", "encrypted_val", Guid.NewGuid());
        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();

        var result = await _controller.GetSecretById(secret.Id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteById_RemovesSecretAndReturnsNoContent()
    {
        var secret = new Secret("MySecret", "encrypted_val", _userId);
        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteById(secret.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await _context.Secrets.FindAsync(secret.Id));
    }

    [Fact]
    public async Task DeleteById_ReturnsNotFoundWhenNotOwnedByUser()
    {
        var secret = new Secret("OtherSecret", "encrypted_val", Guid.NewGuid());
        _context.Secrets.Add(secret);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteById(secret.Id);

        Assert.IsType<NotFoundResult>(result);
        Assert.NotNull(await _context.Secrets.FindAsync(secret.Id));
    }
}
