using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraDrive.Application.DTOs;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tags")]
public class TagsController(ITagService tags) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await tags.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TagUpsertDto dto)
    {
        var result = await tags.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] TagUpsertDto dto)
        => Ok(await tags.UpdateAsync(id, dto));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await tags.DeleteAsync(id);
        return NoContent();
    }
}
