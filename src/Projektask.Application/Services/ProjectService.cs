using Projektask.Application.DTOs;
using Projektask.Application.Exceptions;
using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;

namespace Projektask.Application.Services;

public class ProjectService(
    IProjectReadRepository readRepo,
    IProjectWriteRepository writeRepo) : IProjectService
{
    public async Task<IEnumerable<ProjectListDto>> GetAllAsync(int userId)
        => await readRepo.GetAllByUserAsync(userId);

    public async Task<ProjectDetailDto> GetByIdAsync(int id, int userId)
        => await readRepo.GetByIdAsync(id, userId)
           ?? throw new NotFoundException();

    public async Task<ProjectResponseDto> CreateAsync(ProjectUpsertDto dto, int userId)
    {
        var project = new Project
        {
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description
        };

        var created = await writeRepo.CreateAsync(project);
        return ToResponseDto(created);
    }

    public async Task<ProjectResponseDto> UpdateAsync(int id, ProjectUpsertDto dto, int userId)
    {
        var project = await writeRepo.GetByIdForWriteAsync(id, userId)
            ?? throw new NotFoundException();

        project.Name = dto.Name;
        project.Description = dto.Description;

        var updated = await writeRepo.UpdateAsync(project);
        return ToResponseDto(updated!);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var deleted = await writeRepo.DeleteAsync(id, userId);
        if (!deleted) throw new NotFoundException();
    }

    private static ProjectResponseDto ToResponseDto(Project p)
        => new(p.Id, p.Name, p.Description, p.CreatedAt);
}
