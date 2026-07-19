namespace devault.DTO.Secrets;

public record SecretDetailResponseDto(Guid Id, string Name, string Value, DateTime CreatedAt);
