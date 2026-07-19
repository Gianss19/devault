using devault.Models.Enums;

namespace devault.DTO.Users;

public record UserResponseDto(Guid id, string Name, string Email, Roles Rol);
