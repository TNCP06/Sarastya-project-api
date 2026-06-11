using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Application.Services;

// Implementasi penuh ada di Checkpoint 3
public class AuthService(IUserReadRepository userRead) : IAuthService
{
    public Task<AuthResponseDto> RegisterAsync(RegisterDto dto) => throw new NotImplementedException();
    public Task<AuthResponseDto> LoginAsync(LoginDto dto) => throw new NotImplementedException();
    public Task<UserDto> GetMeAsync(int userId) => throw new NotImplementedException();
}
