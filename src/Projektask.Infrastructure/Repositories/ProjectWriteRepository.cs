using Microsoft.EntityFrameworkCore;
using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;
using Projektask.Infrastructure.Data;

namespace Projektask.Infrastructure.Repositories;

public class ProjectWriteRepository(AppDbContext db) : IProjectWriteRepository
{
    public async Task<Project> CreateAsync(Project project)
    {
        project.CreatedAt = DateTime.UtcNow;
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    // Dipakai oleh Service sebelum update/delete — pastikan project milik user ini
    public async Task<Project?> GetByIdForWriteAsync(int id, int userId)
        => await db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

    public async Task<Project?> UpdateAsync(Project project)
    {
        db.Projects.Update(project);
        await db.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var project = await GetByIdForWriteAsync(id, userId);
        if (project is null) return false;

        db.Projects.Remove(project);
        await db.SaveChangesAsync();
        return true;
    }
}
