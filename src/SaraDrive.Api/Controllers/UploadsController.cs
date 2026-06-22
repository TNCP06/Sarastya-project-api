using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraDrive.Application.DTOs;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Api.Controllers;

// Upload queue. The API only enqueues + reports status; the Python watcher does the Telegram I/O.
[ApiController]
[Authorize]
[Route("api/uploads")]
public class UploadsController(IUploadService uploads) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await uploads.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Enqueue([FromBody] UploadEnqueueDto dto)
    {
        var result = await uploads.EnqueueAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
