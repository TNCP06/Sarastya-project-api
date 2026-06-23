using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;
using SaraDrive.Infrastructure.Data;

namespace SaraDrive.Infrastructure.Repositories;

// Item writes via EF Core (raw SQL where the TEXT-timestamp / ON CONFLICT / cascade shape
// makes EF awkward — same call sites the existing web actions use). `slug` is never modified.
public class ItemWriteRepository(AppDbContext db) : IItemWriteRepository
{
    public async Task<Item?> GetByIdAsync(long id)
        => await db.Items.FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Item> UpdateAsync(Item item)
    {
        item.UpdatedAt = SqlTime.NowText();
        await db.SaveChangesAsync();
        return item;
    }

    // Replace an item's tag set: upsert tag names (color '' = derived on client), then rewrite
    // the item_tags relations. Orphaned tags are intentionally NOT deleted (avoids racing the
    // bot's indexing) — same policy as the web's updateMetadata.
    public async Task ReplaceTagsAsync(long itemId, IEnumerable<string> tagNames)
    {
        var names = tagNames
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM item_tags WHERE item_id = {itemId}");

        foreach (var name in names)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO tags (name, color) VALUES ({name}, '') ON CONFLICT (name) DO NOTHING");
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO item_tags (item_id, tag_id)
                SELECT {itemId}, id FROM tags WHERE lower(name) = lower({name})
                ON CONFLICT DO NOTHING
                """);
        }
    }

    public async Task<bool> SetPrivateAsync(long id, bool value)
    {
        var n = await db.Items
            .Where(i => i.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.IsPrivate, value)
                .SetProperty(i => i.FolderId, (long?)null));
        return n > 0;
    }

    public async Task<bool> SoftDeleteAsync(long id)
    {
        // Trashing is not a content change → leave updated_at/date_added untouched (sort stability).
        var n = await db.Items
            .Where(i => i.Id == id && i.DeletedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.DeletedAt, SqlTime.NowText()));
        return n > 0;
    }

    public async Task<bool> RestoreAsync(long id)
    {
        var item = await db.Items.FindAsync(id);
        if (item != null && item.FolderId.HasValue)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                WITH RECURSIVE sub AS (
                    SELECT id, parent_id FROM folders WHERE id = {item.FolderId.Value}
                    UNION ALL
                    SELECT f.id, f.parent_id FROM folders f JOIN sub ON f.id = sub.parent_id
                )
                UPDATE folders SET deleted_at = NULL WHERE id IN (SELECT id FROM sub)
                """);
        }

        var n = await db.Items
            .Where(i => i.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.DeletedAt, (string?)null));
        return n > 0;
    }

    // Permanent delete. The API does NOT do Telegram I/O — it enqueues a 'delete' job (with the
    // part message ids in the payload, item_id NULL so the row survives the cascade) for the Python
    // bot to remove the channel messages, then hard-deletes the metadata. The DB FK cascades
    // (parts/item_tags via item_id, thumbnails via part_id) clean up the rest in one DELETE.
    public async Task<bool> PurgeAsync(long id)
    {
        var msgIds = await db.Parts.Where(p => p.ItemId == id)
            .Select(p => p.ChannelMsgId).ToListAsync();

        var payload = JsonSerializer.Serialize(new { channel_msg_ids = msgIds });
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO jobs (type, item_id, status, payload) VALUES ('delete', NULL, 'pending', {payload})");

        var n = await db.Items.Where(i => i.Id == id).ExecuteDeleteAsync();
        return n > 0;
    }
}
