using Projektask.Application.DTOs;

namespace Projektask.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto> GetMeAsync(int userId);
}
