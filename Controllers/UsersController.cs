using System.Security.Claims;
using devault.DTO.Users;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace devault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DevaultDbContext _context;
    private readonly IHasherService _bcryptService;

    public UsersController(DevaultDbContext context, IHasherService bcryptService)
    {
        _context = context;
        _bcryptService = bcryptService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route("all")]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAllUsers()
    {
        var listUsers = await _context.Users
            .Select(u => new UserResponseDto(u.Id, u.Name, u.Email, u.Rol))
            .ToListAsync();

        return Ok(listUsers);
    }

    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    [Route("me")]
    public async Task<ActionResult<UserResponseDto>> GetProfile()
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            return Unauthorized("Token inválido.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        return Ok(new UserResponseDto(user.Id, user.Name, user.Email, user.Rol));
    }

    [HttpPost]
    [Authorize(Roles = "User, Admin")]
    [Route("change-name")]
    public async Task<ActionResult> ChangeName([FromBody] ChangeNameUserDto user)
    {
        if (string.IsNullOrWhiteSpace(user.newName))
            return BadRequest("Invalid New Name");

        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            return Unauthorized("Token inválido.");

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (existingUser == null)
            return NotFound("Usuario no encontrado.");

        existingUser.ChangeName(user.newName);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "User, Admin")]
    [Route("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto password)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            return Unauthorized("Token inválido.");

        if (string.IsNullOrWhiteSpace(password.NewPassword))
            return BadRequest("Invalid New Password");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound("Usuario no encontrado.");

        var newPasswordHash = _bcryptService.GenerateHash(password.NewPassword);
        user.ChangePassword(newPasswordHash);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [Route("{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
