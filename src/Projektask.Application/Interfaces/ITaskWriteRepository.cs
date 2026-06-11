using Projektask.Domain.Entities;

namespace Projektask.Application.Interfaces;

public interface ITaskWriteRepository
{
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> GetByIdForWriteAsync(int id, int userId);
    Task<TaskItem?> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(int id, int userId);
}
