using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Projektask.Application.DTOs;
using Projektask.Application.Exceptions;
using Projektask.Application.Interfaces;
using Projektask.Application.Options;
using Projektask.Domain.Entities;

namespace Projektask.Application.Services;

public class AuthService(
    IUserReadRepository userRead,
    IUserWriteRepository userWrite,
    JwtSettings jwtSettings) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Cek email belum terdaftar — kalau sudah, lempar 409
        var existing = await userRead.GetByEmailAsync(dto.Email);
        if (existing is not null)
            throw new ConflictException("Email sudah terdaftar");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            // BCrypt hash password — mirip bcrypt() di PHP/Laravel
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        user = await userWrite.CreateAsync(user);
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userRead.GetByEmailAsync(dto.Email);

        // Pesan generik: tidak boleh bocorkan mana yang salah (email vs password)
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Email atau password salah");

        return BuildAuthResponse(user);
    }

    public async Task<UserDto> GetMeAsync(int userId)
    {
        var user = await userRead.GetByIdAsync(userId)
            ?? throw new NotFoundException();
        return new UserDto(user.Id, user.Name, user.Email);
    }

    // Buat JWT token — claim berisi user id sebagai "sub"
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
