using Projektask.Application.DTOs;

namespace Projektask.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, int userId, string? status);
    Task<TaskDto> CreateAsync(int projectId, TaskUpsertDto dto, int userId);
    Task<TaskDto> UpdateAsync(int id, TaskUpsertDto dto, int userId);
    Task DeleteAsync(int id, int userId);
}
