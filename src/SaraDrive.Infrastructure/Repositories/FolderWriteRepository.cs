using Microsoft.EntityFrameworkCore;
using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;
using SaraDrive.Infrastructure.Data;

namespace SaraDrive.Infrastructure.Repositories;

// Folder writes via EF Core. created_at/updated_at default to now_text() at the DB.
public class FolderWriteRepository(AppDbContext db) : IFolderWriteRepository
{
    public async Task<Folder> CreateAsync(Folder folder)
    {
        db.Folders.Add(folder);
        await db.SaveChangesAsync();
        return folder;
    }

    public async Task<Folder?> GetByIdAsync(long id)
        => await db.Folders.FirstOrDefaultAsync(f => f.Id == id);

    public async Task<Folder> UpdateAsync(Folder folder)
    {
        folder.UpdatedAt = SqlTime.NowText();
        await db.SaveChangesAsync();
        return folder;
    }

    // Soft-delete every item inside the folder subtree, then hard-delete the top folder.
    // parent_id is ON DELETE CASCADE, so child folders go with it. Items use ON DELETE SET NULL,
    // so we soft-delete them first (matching the web's deleteFolder behavior) to keep them in Trash.
    public async Task DeleteRecursiveAsync(long id)
    {
        var folderIds = await GetDescendantFolderIdsAsync(id);
        await db.Items
            .Where(i => i.FolderId != null && folderIds.Contains(i.FolderId.Value) && i.DeletedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.DeletedAt, SqlTime.NowText()));

        await db.Folders.Where(f => f.Id == id).ExecuteDeleteAsync();
    }

    // The folder itself plus all descendant folder ids (recursive CTE).
    public async Task<IReadOnlyList<long>> GetDescendantFolderIdsAsync(long id)
    {
        var ids = await db.Database
            .SqlQuery<long>($"""
                WITH RECURSIVE sub AS (
                    SELECT id FROM folders WHERE id = {id}
                    UNION ALL
                    SELECT f.id FROM folders f JOIN sub ON f.parent_id = sub.id)
                SELECT id AS "Value" FROM sub
                """)
            .ToListAsync();
        return ids;
    }
}
