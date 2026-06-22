using System.Data;
using Dapper;
using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;

namespace SaraDrive.Infrastructure.Repositories;

// All READ queries use Dapper (raw SQL) — per the test requirement.
public class UserReadRepository(IDbConnection db) : IUserReadRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        // Case-insensitive match (lower) — same as the unique index on lower(email).
        const string sql = """
            SELECT id AS Id, name AS Name, email AS Email,
                   password_hash AS PasswordHash, created_at AS CreatedAt
            FROM users
            WHERE lower(email) = lower(@Email)
            LIMIT 1
            """;
        return await db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        const string sql = """
            SELECT id AS Id, name AS Name, email AS Email,
                   password_hash AS PasswordHash, created_at AS CreatedAt
            FROM users
            WHERE id = @Id
            LIMIT 1
            """;
        return await db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }
}
