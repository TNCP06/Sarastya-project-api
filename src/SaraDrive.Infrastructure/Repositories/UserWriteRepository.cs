using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;
using SaraDrive.Infrastructure.Data;

namespace SaraDrive.Infrastructure.Repositories;

// Write via EF Core — INSERT a new user at register. created_at is filled by the DB default.
public class UserWriteRepository(AppDbContext db) : IUserWriteRepository
{
    public async Task<User> CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
