using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Application.Services;

// Implementasi penuh ada di Checkpoint 4
public class ProjectService(
    IProjectReadRepository readRepo,
    IProjectWriteRepository writeRepo) : IProjectService
{
    public Task<IEnumerable<ProjectListDto>> GetAllAsync(int userId) => throw new NotImplementedException();
    public Task<ProjectDetailDto> GetByIdAsync(int id, int userId) => throw new NotImplementedException();
    public Task<ProjectResponseDto> CreateAsync(ProjectUpsertDto dto, int userId) => throw new NotImplementedException();
    public Task<ProjectResponseDto> UpdateAsync(int id, ProjectUpsertDto dto, int userId) => throw new NotImplementedException();
    public Task DeleteAsync(int id, int userId) => throw new NotImplementedException();
}
