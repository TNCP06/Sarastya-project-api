using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await projectService.GetAllAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await projectService.GetByIdAsync(id, UserId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProjectUpsertDto dto)
    {
        var result = await projectService.CreateAsync(dto, UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectUpsertDto dto)
        => Ok(await projectService.UpdateAsync(id, dto, UserId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await projectService.DeleteAsync(id, UserId);
        return NoContent();
    }
}
