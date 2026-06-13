using Microsoft.EntityFrameworkCore;
using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;
using Projektask.Infrastructure.Data;

namespace Projektask.Infrastructure.Repositories;

public class TaskWriteRepository(AppDbContext db) : ITaskWriteRepository
{
    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    // Otorisasi lewat JOIN ke projects — t.Project.UserId ditranslasi EF Core jadi INNER JOIN
    public async Task<TaskItem?> GetByIdForWriteAsync(int id, int userId)
        => await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.Project.UserId == userId);

    public async Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        db.Tasks.Update(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var task = await GetByIdForWriteAsync(id, userId);
        if (task is null) return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }
}
