using Projektask.Application.DTOs;

namespace Projektask.Application.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectListDto>> GetAllAsync(int userId);
    Task<ProjectDetailDto> GetByIdAsync(int id, int userId);
    Task<ProjectResponseDto> CreateAsync(ProjectUpsertDto dto, int userId);
    Task<ProjectResponseDto> UpdateAsync(int id, ProjectUpsertDto dto, int userId);
    Task DeleteAsync(int id, int userId);
}
