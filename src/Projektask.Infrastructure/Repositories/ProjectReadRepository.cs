using System.Data;
using Dapper;
using Projektask.Application.DTOs;
using Projektask.Application.Interfaces;

namespace Projektask.Infrastructure.Repositories;

public class ProjectReadRepository(IDbConnection db) : IProjectReadRepository
{
    public async Task<IEnumerable<ProjectListDto>> GetAllByUserAsync(int userId)
    {
        // JOIN + COUNT di SQL — ini yang jadi syarat tes "bukan sekadar SELECT *"
        // FILTER (WHERE ...) adalah sintaks PostgreSQL untuk conditional COUNT
        const string sql = """
            SELECT
                p.id          AS Id,
                p.name        AS Name,
                p.description AS Description,
                COUNT(t.id)::int                                     AS TaskCount,
                (COUNT(t.id) FILTER (WHERE t.status = 'done'))::int  AS DoneTaskCount,
                p.created_at  AS CreatedAt
            FROM projects p
            LEFT JOIN tasks t ON t.project_id = p.id
            WHERE p.user_id = @UserId
            GROUP BY p.id, p.name, p.description, p.created_at
            ORDER BY p.created_at DESC
            """;

        return await db.QueryAsync<ProjectListDto>(sql, new { UserId = userId });
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(int id, int userId)
    {
        // Query 1: ambil data project (filter user_id untuk otorisasi)
        const string projectSql = """
            SELECT id AS Id, name AS Name, description AS Description, created_at AS CreatedAt
            FROM projects
            WHERE id = @Id AND user_id = @UserId
            LIMIT 1
            """;

        var project = await db.QueryFirstOrDefaultAsync<(int Id, string Name, string? Description, DateTime CreatedAt)>(
            projectSql, new { Id = id, UserId = userId });

        // Kalau project tidak ditemukan atau bukan milik user ini → return null
        if (project == default) return null;

        // Query 2: ambil semua tasks milik project ini
        const string tasksSql = """
            SELECT
                id          AS Id,
                title       AS Title,
                description AS Description,
                status      AS Status,
                due_date    AS DueDate,
                created_at  AS CreatedAt
            FROM tasks
            WHERE project_id = @ProjectId
            ORDER BY created_at ASC
            """;

        var tasks = await db.QueryAsync<TaskDto>(tasksSql, new { ProjectId = id });

        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.CreatedAt,
            tasks);
    }
}
