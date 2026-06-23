using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraDrive.Application.DTOs;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Api.Controllers;

// Upload queue. The API enqueues + controls DB status; the Python watcher does Telegram I/O.
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

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateQueued(long id, [FromBody] UploadUpdateDto dto)
    {
        await uploads.UpdateQueuedAsync(id, dto);
        return NoContent();
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        await uploads.CancelAsync(id);
        return NoContent();
    }

    [HttpPost("{id:long}/start")]
    public async Task<IActionResult> Start(long id)
    {
        await uploads.StartAsync(id);
        return NoContent();
    }

    [HttpPost("{id:long}/retry")]
    public async Task<IActionResult> Retry(long id)
    {
        await uploads.RetryAsync(id);
        return NoContent();
    }

    [HttpPost("start-all")]
    public async Task<IActionResult> StartAll()
    {
        await uploads.StartAllAsync();
        return NoContent();
    }

    [HttpDelete("finished")]
    public async Task<IActionResult> ClearFinished()
    {
        await uploads.ClearFinishedAsync();
        return NoContent();
    }
}
