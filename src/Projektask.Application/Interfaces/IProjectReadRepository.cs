using Projektask.Application.DTOs;

namespace Projektask.Application.Interfaces;

public interface IProjectReadRepository
{
    Task<IEnumerable<ProjectListDto>> GetAllByUserAsync(int userId);
    Task<ProjectDetailDto?> GetByIdAsync(int id, int userId);
}
