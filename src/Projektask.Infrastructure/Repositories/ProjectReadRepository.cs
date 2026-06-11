using System.Data;
using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Infrastructure.Repositories;

// Dapper read repository — implementasi penuh di Checkpoint 4
public class ProjectReadRepository(IDbConnection db) : IProjectReadRepository
{
    public Task<IEnumerable<ProjectListDto>> GetAllByUserAsync(int userId) => throw new NotImplementedException();
    public Task<ProjectDetailDto?> GetByIdAsync(int id, int userId) => throw new NotImplementedException();
}
