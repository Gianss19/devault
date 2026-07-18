using System.Security.Claims;
using devault;
using devault.Controllers;
using devault.DTO.Users;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models;
using devault.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace test.Controllers;

public class UsersControllerTests
{
    private readonly DevaultDbContext _context;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        var options = new DbContextOptionsBuilder<DevaultDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new DevaultDbContext(options);

        var hasherServiceMock = new Mock<IHasherService>();
        hasherServiceMock.Setup(h => h.GenerateHash(It.IsAny<string>()))
            .Returns((string s) => "hash_" + s);

        _controller = new UsersController(_context, hasherServiceMock.Object);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsAllUsers()
    {
        var adminId = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }))
            }
        };

        _context.Users.Add(new User("User1", "user1@example.com", "hash1"));
        _context.Users.Add(new User("User2", "user2@example.com", "hash2"));
        await _context.SaveChangesAsync();

        var result = await _controller.GetAllUsers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IReadOnlyList<UserResponseDto>>(okResult.Value);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task ChangeName_WithValidName_ReturnsOk()
    {
        var user = new User("OldName", "test@example.com", "hash");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }))
            }
        };

        var dto = new ChangeNameUserDto("NewValidName");

        var result = await _controller.ChangeName(dto);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task ChangeName_WithEmptyName_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        var dto = new ChangeNameUserDto("");

        var result = await _controller.ChangeName(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid New Name", badRequest.Value);
    }

    [Fact]
    public async Task ChangePassword_WithValidPassword_ReturnsOk()
    {
        var user = new User("TestUser", "test@example.com", "old_hash");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }))
            }
        };

        var dto = new ChangePasswordDto("New@12345");

        var result = await _controller.ChangePassword(dto);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task ChangePassword_WithEmptyPassword_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        var dto = new ChangePasswordDto("");

        var result = await _controller.ChangePassword(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid New Password", badRequest.Value);
    }

    [Fact]
    public async Task DeleteUser_RemovesUserAndReturnsNoContent()
    {
        var adminId = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }))
            }
        };

        var targetUser = new User("TargetUser", "target@example.com", "hash");
        _context.Users.Add(targetUser);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteUser(targetUser.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await _context.Users.FindAsync(targetUser.Id));
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ReturnsNotFound()
    {
        var adminId = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }))
            }
        };

        var result = await _controller.DeleteUser(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
