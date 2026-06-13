using System.Data;
using Dapper;
using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Infrastructure.Repositories;

public class TaskReadRepository(IDbConnection db) : ITaskReadRepository
{
    public async Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, int userId, string? status)
    {
        // Urutan kolom harus cocok dengan urutan parameter constructor TaskDto
        // TaskDto(int Id, string Title, string? Description, string Status, DateOnly? DueDate, DateTime CreatedAt)
        const string sql = """
            SELECT
                t.id          AS Id,
                t.title       AS Title,
                t.description AS Description,
                t.status      AS Status,
                t.due_date    AS DueDate,
                t.created_at  AS CreatedAt
            FROM tasks t
            WHERE t.project_id = @ProjectId
              AND (@Status IS NULL OR t.status = @Status)
            ORDER BY t.created_at ASC
            """;

        return await db.QueryAsync<TaskDto>(sql, new { ProjectId = projectId, Status = status });
    }
}
