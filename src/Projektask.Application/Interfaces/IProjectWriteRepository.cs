using Projektask.Domain.Entities;

namespace Projektask.Application.Interfaces;

public interface IProjectWriteRepository
{
    Task<Project> CreateAsync(Project project);
    Task<Project?> UpdateAsync(Project project);
    Task<bool> DeleteAsync(int id, int userId);
    Task<Project?> GetByIdForWriteAsync(int id, int userId);
}
