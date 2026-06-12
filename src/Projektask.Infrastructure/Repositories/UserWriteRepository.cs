using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;
using Projektask.Infrastructure.Data;

namespace Projektask.Infrastructure.Repositories;

// Write pakai EF Core — untuk INSERT user baru saat register
public class UserWriteRepository(AppDbContext db) : IUserWriteRepository
{
    public async Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
