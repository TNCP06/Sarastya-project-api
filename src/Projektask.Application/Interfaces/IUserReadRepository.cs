using Projektask.Domain.Entities;

namespace Projektask.Application.Interfaces;

public interface IUserReadRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
}
