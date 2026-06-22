namespace SaraDrive.Application.DTOs;

public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);

public record UserDto(long Id, string Name, string Email);
public record AuthResponseDto(string Token, UserDto User);
