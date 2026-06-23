using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraDrive.Application.DTOs;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Api.Controllers;

// Item WRITE endpoints. (Reads for a single item live on DriveController: GET /api/items/{id}.)
[ApiController]
[Authorize]
[Route("api/items")]
public class ItemsController(IItemService items) : ControllerBase
{
    // Metadata edit (title / kind / tags). slug is immutable and never changes.
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ItemUpdateDto dto)
        => Ok(await items.UpdateAsync(id, dto));

    [HttpPost("{id:long}/favorite")]
    public async Task<IActionResult> Favorite(long id, [FromBody] ItemFavoriteDto dto)
    {
        await items.SetFavoriteAsync(id, dto.Value);
        return NoContent();
    }

    [HttpPost("{id:long}/private")]
    public async Task<IActionResult> Private(long id, [FromBody] ItemPrivateDto dto)
    {
        await items.SetPrivateAsync(id, dto.Value);
        return NoContent();
    }

    [HttpPost("{id:long}/move")]
    public async Task<IActionResult> Move(long id, [FromBody] ItemMoveDto dto)
    {
        await items.MoveAsync(id, dto.FolderId);
        return NoContent();
    }

    // Soft delete → Trash. The file stays on Telegram (restore is lossless).
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> SoftDelete(long id)
    {
        await items.SoftDeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:long}/restore")]
    public async Task<IActionResult> Restore(long id)
    {
        await items.RestoreAsync(id);
        return NoContent();
    }

    // Permanent delete (must be in Trash). Enqueues a bot 'delete' job for Telegram cleanup.
    [HttpPost("{id:long}/purge")]
    public async Task<IActionResult> Purge(long id)
    {
        await items.PurgeAsync(id);
        return NoContent();
    }
}
