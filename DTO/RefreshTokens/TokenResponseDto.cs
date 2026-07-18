namespace devault.DTO.RefreshTokens;

public record TokenResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
