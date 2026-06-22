using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SaraDrive.Application.DTOs;
using SaraDrive.Application.Exceptions;
using SaraDrive.Application.Interfaces;
using SaraDrive.Application.Options;
using SaraDrive.Domain.Entities;

namespace SaraDrive.Application.Services;

public class AuthService(
    IUserReadRepository userRead,
    IUserWriteRepository userWrite,
    JwtSettings jwtSettings) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await userRead.GetByEmailAsync(dto.Email) is not null)
            throw new ConflictException("Email sudah terdaftar");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        user = await userWrite.CreateAsync(user);
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userRead.GetByEmailAsync(dto.Email);

        // Generic message — never reveal whether it was the email or the password.
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Email atau password salah");

        return BuildAuthResponse(user);
    }

    public async Task<UserDto> GetMeAsync(long userId)
    {
        var user = await userRead.GetByIdAsync(userId) ?? throw new NotFoundException();
        return new UserDto(user.Id, user.Name, user.Email);
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [new Claim("sub", user.Id.ToString())],
            expires: DateTime.UtcNow.AddHours(jwtSettings.ExpiresInHours),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var userDto = new UserDto(user.Id, user.Name, user.Email);
        return new AuthResponseDto(tokenString, userDto);
    }
}
