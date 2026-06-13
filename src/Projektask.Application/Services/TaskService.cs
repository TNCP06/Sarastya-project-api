using Projektask.Application.DTOs;
using Projektask.Application.Exceptions;
using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;

namespace Projektask.Application.Services;

public class TaskService(
    ITaskReadRepository readRepo,
    ITaskWriteRepository writeRepo,
    IProjectWriteRepository projectRepo) : ITaskService
{
    public async Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, int userId, string? status)
    {
        // Verifikasi project milik user sebelum ambil tasks
        var project = await projectRepo.GetByIdForWriteAsync(projectId, userId);
        if (project is null) throw new NotFoundException();

        return await readRepo.GetByProjectAsync(projectId, userId, status);
    }

    public async Task<TaskDto> CreateAsync(int projectId, TaskUpsertDto dto, int userId)
    {
        var project = await projectRepo.GetByIdForWriteAsync(projectId, userId);
        if (project is null) throw new NotFoundException();

        var task = new TaskItem
        {
            ProjectId = projectId,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            DueDate = dto.DueDate
        };

        var created = await writeRepo.CreateAsync(task);
        return ToDto(created);
    }

    public async Task<TaskDto> UpdateAsync(int id, TaskUpsertDto dto, int userId)
    {
        var task = await writeRepo.GetByIdForWriteAsync(id, userId)
            ?? throw new NotFoundException();

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.DueDate = dto.DueDate;

        var updated = await writeRepo.UpdateAsync(task);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deleted = await writeRepo.DeleteAsync(id, userId);
        if (!deleted) throw new NotFoundException();
    }

    private static TaskDto ToDto(TaskItem t)
        => new(t.Id, t.Title, t.Description, t.Status, t.DueDate, t.CreatedAt);
}
