using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Application.Services;

// Implementasi penuh ada di Checkpoint 5
public class TaskService(
    ITaskReadRepository readRepo,
    ITaskWriteRepository writeRepo,
    IProjectWriteRepository projectRepo) : ITaskService
{
    public Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, int userId, string? status) => throw new NotImplementedException();
    public Task<TaskDto> CreateAsync(int projectId, TaskUpsertDto dto, int userId) => throw new NotImplementedException();
    public Task<TaskDto> UpdateAsync(int id, TaskUpsertDto dto, int userId) => throw new NotImplementedException();
    public Task DeleteAsync(int id, int userId) => throw new NotImplementedException();
}
