using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;
using Projektask.Infrastructure.Data;

namespace Projektask.Infrastructure.Repositories;

// EF Core write repository — implementasi penuh di Checkpoint 4
public class ProjectWriteRepository(AppDbContext db) : IProjectWriteRepository
{
    public Task<Project> CreateAsync(Project project) => throw new NotImplementedException();
    public Task<Project?> UpdateAsync(Project project) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id, int userId) => throw new NotImplementedException();
    public Task<Project?> GetByIdForWriteAsync(int id, int userId) => throw new NotImplementedException();
}
