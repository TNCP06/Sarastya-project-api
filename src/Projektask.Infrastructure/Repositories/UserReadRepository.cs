using System.Data;
using Dapper;
using Projektask.Application.Interfaces;
using Projektask.Domain.Entities;

namespace Projektask.Infrastructure.Repositories;

// Semua query READ pakai Dapper (raw SQL) — sesuai syarat tes
public class UserReadRepository(IDbConnection db) : IUserReadRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = """
            SELECT id, name, email, password_hash AS PasswordHash, created_at AS CreatedAt
            FROM users
            WHERE email = @Email
            LIMIT 1
            """;
        return await db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT id, name, email, password_hash AS PasswordHash, created_at AS CreatedAt
            FROM users
            WHERE id = @Id
            LIMIT 1
            """;
        return await db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }
}
