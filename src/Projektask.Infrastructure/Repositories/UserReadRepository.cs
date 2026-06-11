using System.Data;
using Dapper;
using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;

namespace Projektask.Infrastructure.Repositories;

// Dapper read repository — implementasi penuh di Checkpoint 3
public class UserReadRepository(IDbConnection db) : IUserReadRepository
{
    public Task<User?> GetByEmailAsync(string email) => throw new NotImplementedException();
    public Task<User?> GetByIdAsync(int id) => throw new NotImplementedException();
}
