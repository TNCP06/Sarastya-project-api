namespace SaraDrive.Application.Options;

// Read from appsettings / env var Jwt__Secret, stored in DI for AuthService.
public record JwtSettings(string Secret, int ExpiresInHours);
