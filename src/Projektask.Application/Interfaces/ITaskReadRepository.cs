using Projektask.Application.DTOs;

namespace Projektask.Application.Interfaces;

public interface ITaskReadRepository
{
    Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, int userId, string? status);
}
