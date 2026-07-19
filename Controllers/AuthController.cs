using System.ComponentModel.DataAnnotations;
using devault.DTO;
using devault.DTO.RefreshTokens;
using devault.DTO.Users;
using devault.Entities.Persistance;
using devault.Interfaces;
using devault.Models;
using devault.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace devault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DevaultDbContext _context;
    private readonly ITokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IHasherService _bcryptService;

    public AuthController(
        DevaultDbContext context,
        ITokenService jwtService,
        IRefreshTokenService refreshTokenService,
        IHasherService bcryptService)
    {
        _context = context;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _bcryptService = bcryptService;
    }

    [HttpPost]
    [Route("signup")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserRegisterDto user)
    {
        if (user == null)
            return BadRequest("User is null");

        if (await _context.Users.AnyAsync(u => u.Email == user.Email || u.Name == user.Name))
            return BadRequest("User already exists");

        var passwordHash = _bcryptService.GenerateHash(user.Password);
        User newUser = new(user.Name, user.Email, passwordHash);

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new UserResponseDto(newUser.Id, newUser.Name, newUser.Email, newUser.Rol));
    }

    [HttpPost]
    [Route("register-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponseDto>> RegisterAdmin([FromBody] AdminRegisterDto user)
    {
        if (user == null)
            return BadRequest("User is null");

        if (await _context.Users.AnyAsync(u => u.Email == user.Email || u.Name == user.Name))
            return BadRequest("User already exists");

        var passwordHash = _bcryptService.GenerateHash(user.Password);
        User newUser = new(user.Name, user.Email, passwordHash, Roles.Admin);

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new UserResponseDto(newUser.Id, newUser.Name, newUser.Email, newUser.Rol));
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] UserAuthRequestDto user)
    {
        if (string.IsNullOrWhiteSpace(user.Password))
            return BadRequest("Invalid Credentials.");

        var loginUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (loginUser == null)
            return BadRequest("Invalid Credentials.");

        var isValid = _bcryptService.IsPasswordValid(user.Password, loginUser.PasswordHash);
        if (!isValid)
            return BadRequest("Invalid Credentials.");

        var accessToken = _jwtService.GenerateAccessToken(loginUser);
        var refreshToken = _refreshTokenService.Create(loginUser.Id);

        await _context.SaveChangesAsync();

        return Ok(new TokenResponseDto(accessToken, refreshToken.PlainTextToken, refreshToken.ExpiresAt));
    }

    [HttpPost]
    [Route("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var refreshToken = _refreshTokenService.GetByToken(request.RefreshToken);
        if (refreshToken == null || !refreshToken.IsActive)
            return Unauthorized("Refresh token inválido o expirado.");

        var user = await _context.Users.FindAsync(refreshToken.UserId);
        if (user == null)
            return Unauthorized("Usuario no encontrado.");

        _refreshTokenService.Revoke(refreshToken);
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _refreshTokenService.Create(user.Id);

        await _context.SaveChangesAsync();

        return Ok(new TokenResponseDto(newAccessToken, newRefreshToken.PlainTextToken, newRefreshToken.ExpiresAt));
    }

    [HttpPost]
    [Route("logout")]
    [Authorize(Roles = "User, Admin")]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        var refreshToken = _refreshTokenService.GetByToken(request.RefreshToken);
        if (refreshToken == null || !refreshToken.IsActive)
            return BadRequest("Token inválido o ya revocado.");

        _refreshTokenService.Revoke(refreshToken);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
