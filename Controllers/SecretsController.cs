using System.Security.Claims;
using devault.DTO.Secrets;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace devault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecretsController : ControllerBase
{
    private readonly IEncryptService _encryptService;
    private readonly DevaultDbContext _context;

    public SecretsController(IEncryptService encryptService, DevaultDbContext context)
    {
        _encryptService = encryptService;
        _context = context;
    }

    [HttpPost]
    [Route("create")]
    [Authorize(Roles = "User, Admin")]
    public async Task<ActionResult> Generate([FromBody] SecretRequestDto secret)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            return Unauthorized("Token inválido.");

        if (string.IsNullOrWhiteSpace(secret.Name))
            return BadRequest("El nombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(secret.Value))
            return BadRequest("El valor del secreto no puede estar vacío.");

        var secretEncrypted = _encryptService.Encrypt(secret.Value);
        var newSecret = new Secret(secret.Name, secretEncrypted, userId);

        _context.Secrets.Add(newSecret);
        await _context.SaveChangesAsync();

        return Created();
    }

    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    [Route("all")]
    public async Task<ActionResult<IReadOnlyList<SecretResponseDto>>> AllSecretsPerUserId()
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            return Unauthorized("Token inválido.");

        var secretsList = await _context.Secrets
            .Where(s => s.UserId == userId)
            .Select(s => new SecretResponseDto(s.Id, s.Name, s.CreatedAt))
            .ToListAsync();

        return Ok(secretsList);
    }

    [HttpGet]
    [Authorize(Roles = "User, Admin")]
    [Route("{id:guid}")]
    public async Task<ActionResult<SecretResponseDto>> GetSecretById(Guid id)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            return Unauthorized("Token inválido.");

        var secret = await _context.Secrets.FindAsync(id);

        if (secret == null || secret.UserId != userId)
            return NotFound();

        return Ok(new SecretResponseDto(secret.Id, secret.Name, secret.CreatedAt));
    }

    [HttpDelete]
    [Authorize(Roles = "User, Admin")]
    [Route("{id:guid}")]
    public async Task<ActionResult> DeleteById(Guid id)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            return Unauthorized("Token inválido.");

        var secret = await _context.Secrets.FindAsync(id);

        if (secret == null || secret.UserId != userId)
            return NotFound();

        _context.Secrets.Remove(secret);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
