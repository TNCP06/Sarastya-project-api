using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Api.Controllers;

[ApiController]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("api/projects/{projectId}/tasks")]
    public async Task<IActionResult> GetByProject(int projectId, [FromQuery] string? status)
        => Ok(await taskService.GetByProjectAsync(projectId, UserId, status));

    [HttpPost("api/projects/{projectId}/tasks")]
    public async Task<IActionResult> Create(int projectId, [FromBody] TaskUpsertDto dto)
    {
        var result = await taskService.CreateAsync(projectId, dto, UserId);
        return Created($"api/tasks/{result.Id}", result);
    }

    [HttpPut("api/tasks/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskUpsertDto dto)
        => Ok(await taskService.UpdateAsync(id, dto, UserId));

    [HttpDelete("api/tasks/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await taskService.DeleteAsync(id, UserId);
        return NoContent();
    }
}
