using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraDrive.Application.DTOs;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/folders")]
public class FoldersController(IFolderService folders) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FolderCreateDto dto)
    {
        var result = await folders.CreateAsync(dto);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Rename(long id, [FromBody] FolderRenameDto dto)
        => Ok(await folders.RenameAsync(id, dto));

    [HttpPost("{id:long}/move")]
    public async Task<IActionResult> Move(long id, [FromBody] FolderMoveDto dto)
    {
        await folders.MoveAsync(id, dto);
        return NoContent();
    }

    [HttpPost("{id:long}/private")]
    public async Task<IActionResult> Private(long id, [FromBody] FolderPrivateDto dto)
    {
        await folders.SetPrivateAsync(id, dto.Value);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await folders.DeleteAsync(id);
        return NoContent();
    }
}
