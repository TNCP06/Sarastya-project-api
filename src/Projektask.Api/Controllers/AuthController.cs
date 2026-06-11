using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        return CreatedAtAction(nameof(Me), result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var user = await authService.GetMeAsync(userId);
        return Ok(user);
    }
}
