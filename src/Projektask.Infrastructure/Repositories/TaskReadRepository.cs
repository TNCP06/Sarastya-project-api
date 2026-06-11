using System.Data;
using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Infrastructure.Repositories;

// Dapper read repository — implementasi penuh di Checkpoint 5
public class TaskReadRepository(IDbConnection db) : ITaskReadRepository
{
    public Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, int userId, string? status) => throw new NotImplementedException();
}
