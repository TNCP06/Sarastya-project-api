using Projektask.Domain.Entities;

namespace Projektask.Application.Interfaces;

public interface IUserWriteRepository
{
    Task<User> CreateAsync(User user);
}
