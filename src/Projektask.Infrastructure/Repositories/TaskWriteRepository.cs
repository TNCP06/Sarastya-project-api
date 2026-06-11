using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;
using Projektask.Infrastructure.Data;

namespace Projektask.Infrastructure.Repositories;

// EF Core write repository — implementasi penuh di Checkpoint 5
public class TaskWriteRepository(AppDbContext db) : ITaskWriteRepository
{
    public Task<TaskItem> CreateAsync(TaskItem task) => throw new NotImplementedException();
    public Task<TaskItem?> GetByIdForWriteAsync(int id, int userId) => throw new NotImplementedException();
    public Task<TaskItem?> UpdateAsync(TaskItem task) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id, int userId) => throw new NotImplementedException();
}
