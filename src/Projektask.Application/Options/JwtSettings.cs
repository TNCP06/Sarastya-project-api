namespace Projektask.Application.Options;

// Dibaca dari appsettings / env var JWT_SECRET, disimpan di DI container
public record JwtSettings(string Secret, int ExpiresInHours);
