using System.Data;
using Dapper;
using SaraDrive.Application.DTOs;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Infrastructure.Repositories;

// All drive READ queries — raw SQL via Dapper. Mirrors the queries the existing Next.js
// dashboard runs (web/lib/items.ts), reshaped into the API contract. The drive is
// single-tenant, so no user_id filtering: `isPrivate` partitions Main vs the PIN-gated space.
public class DriveReadRepository(IDbConnection db) : IDriveReadRepository
{
    // Raw rows (int 0/1 flags) before conversion to bool DTOs.
    private sealed class ItemRow
    {
        public long Id { get; set; }
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public string Kind { get; set; } = "";
        public int TotalParts { get; set; }
        public long TotalSize { get; set; }
        public int IsFavorite { get; set; }
        public int IsPrivate { get; set; }
        public string DateAdded { get; set; } = "";
        public string UpdatedAt { get; set; } = "";
        public string? DeletedAt { get; set; }
        public long? FolderId { get; set; }
    }

    private sealed class FirstPartRow
    {
        public long ItemId { get; set; }
        public long PartId { get; set; }
        public string? FileName { get; set; }
    }

    private const string ItemColumns = """
        id AS Id, slug AS Slug, title AS Title, kind AS Kind,
        total_parts AS TotalParts, total_size AS TotalSize,
        is_favorite AS IsFavorite, is_private AS IsPrivate,
        date_added AS DateAdded, updated_at AS UpdatedAt,
        deleted_at AS DeletedAt, folder_id AS FolderId
        """;

    public async Task<DriveDto> GetDriveAsync(bool isPrivate)
    {
        var p = isPrivate ? 1 : 0;

        var items = (await db.QueryAsync<ItemRow>(
            $"SELECT {ItemColumns} FROM items WHERE is_private = @P", new { P = p })).ToList();

        var tags = await db.QueryAsync<TagDto>("""
            SELECT id AS Id, name AS Name, color AS Color
            FROM tags
            WHERE id IN (
                SELECT DISTINCT it.tag_id FROM item_tags it
                JOIN items i ON i.id = it.item_id WHERE i.is_private = @P)
            ORDER BY lower(name)
            """, new { P = p });

        var itemTags = await db.QueryAsync<(long ItemId, long TagId)>("""
            SELECT it.item_id AS ItemId, it.tag_id AS TagId FROM item_tags it
            JOIN items i ON i.id = it.item_id WHERE i.is_private = @P
            """, new { P = p });

        var folders = await db.QueryAsync<FolderDto>("""
            SELECT id AS Id, name AS Name, parent_id AS ParentId,
                   (is_private = 1) AS IsPrivate, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM folders WHERE is_private = @P ORDER BY lower(name)
            """, new { P = p });

        var (coverItems, firstParts) = await LoadCoversAndFirstPartsAsync();

        var tagsByItem = itemTags
            .GroupBy(t => t.ItemId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.TagId).ToList());

        var files = items.Select(i => ToSummary(i, tagsByItem, coverItems, firstParts));

        return new DriveDto(files, tags, folders);
    }

    public async Task<IEnumerable<ItemSummaryDto>> SearchAsync(string query, bool isPrivate)
    {
        var p = isPrivate ? 1 : 0;
        var items = (await db.QueryAsync<ItemRow>($"""
            SELECT {ItemColumns} FROM items
            WHERE is_private = @P AND deleted_at IS NULL AND title ILIKE @Q
            ORDER BY date_added DESC
            """, new { P = p, Q = $"%{query}%" })).ToList();

        var ids = items.Select(i => i.Id).ToArray();
        var tagsByItem = await TagsByItemAsync(ids);
        var (coverItems, firstParts) = await LoadCoversAndFirstPartsAsync();
        return items.Select(i => ToSummary(i, tagsByItem, coverItems, firstParts));
    }

    public async Task<IEnumerable<ItemSummaryDto>> GetTrashAsync()
    {
        var items = (await db.QueryAsync<ItemRow>(
            $"SELECT {ItemColumns} FROM items WHERE deleted_at IS NOT NULL ORDER BY deleted_at DESC")).ToList();
        var ids = items.Select(i => i.Id).ToArray();
        var tagsByItem = await TagsByItemAsync(ids);
        var (coverItems, firstParts) = await LoadCoversAndFirstPartsAsync();
        return items.Select(i => ToSummary(i, tagsByItem, coverItems, firstParts));
    }

    public async Task<ItemDetailDto?> GetItemAsync(long id)
    {
        var item = await db.QueryFirstOrDefaultAsync<ItemRow>(
            $"SELECT {ItemColumns} FROM items WHERE id = @Id LIMIT 1", new { Id = id });
        if (item is null) return null;

        var tagNames = await db.QueryAsync<string>("""
            SELECT t.name FROM item_tags it JOIN tags t ON t.id = it.tag_id
            WHERE it.item_id = @Id ORDER BY lower(t.name)
            """, new { Id = id });

        var parts = await db.QueryAsync<PartDto>("""
            SELECT p.id AS Id, p.part_number AS PartNumber, p.channel_msg_id AS ChannelMsgId,
                   p.file_name AS FileName, p.file_size AS FileSize, p.uploaded_at AS UploadedAt,
                   EXISTS(SELECT 1 FROM thumbnails th WHERE th.part_id = p.id) AS HasThumb
            FROM parts p WHERE p.item_id = @Id ORDER BY p.channel_msg_id
            """, new { Id = id });

        return new ItemDetailDto(
            item.Id, item.Slug, item.Title, item.Kind, item.TotalParts, item.TotalSize,
            item.IsFavorite == 1, item.IsPrivate == 1, item.DateAdded, item.UpdatedAt,
            item.DeletedAt, item.FolderId, tagNames, parts);
    }

    public async Task<IEnumerable<GalleryPartDto>> GetGalleryAsync(long itemId)
        => await db.QueryAsync<GalleryPartDto>("""
            SELECT th.part_id AS PartId, p.file_name AS FileName, th.mime AS Mime, th.data AS Data
            FROM thumbnails th JOIN parts p ON p.id = th.part_id
            WHERE p.item_id = @Id ORDER BY p.channel_msg_id
            """, new { Id = itemId });

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        => await db.QueryAsync<TagDto>(
            "SELECT id AS Id, name AS Name, color AS Color FROM tags ORDER BY lower(name)");

    public async Task<IEnumerable<UploadJobDto>> GetUploadsAsync()
        => await db.QueryAsync<UploadJobDto>("""
            SELECT id AS Id, kind AS Kind, title AS Title, tags AS Tags, status AS Status,
                   progress AS Progress, message AS Message, parts_done AS PartsDone,
                   total_bytes AS TotalBytes, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM upload_jobs ORDER BY created_at DESC
            """);

    public async Task<StreamInfoDto?> GetStreamInfoAsync(long itemId, string streamerBase)
    {
        var head = await db.QueryFirstOrDefaultAsync<(long Id, string Kind)>(
            "SELECT id AS Id, kind AS Kind FROM items WHERE id = @Id LIMIT 1", new { Id = itemId });
        if (head.Id == 0) return null;

        var basePart = streamerBase.TrimEnd('/');
        var parts = (await db.QueryAsync<(long PartId, int PartNumber, string? FileName, long ChannelMsgId)>("""
            SELECT id AS PartId, part_number AS PartNumber, file_name AS FileName, channel_msg_id AS ChannelMsgId
            FROM parts WHERE item_id = @Id ORDER BY channel_msg_id
            """, new { Id = itemId }))
            .Select(p => new StreamPartDto(p.PartId, p.PartNumber, p.FileName, p.ChannelMsgId,
                $"{basePart}/stream/{p.PartId}"));

        return new StreamInfoDto(head.Id, head.Kind, basePart, parts);
    }

    public async Task<IEnumerable<SubtitleDto>> GetSubtitlesAsync(long partId)
        => await db.QueryAsync<SubtitleDto>("""
            SELECT lang AS Lang, created_at AS CreatedAt FROM subtitles
            WHERE part_id = @Id ORDER BY lang
            """, new { Id = partId });

    // --- helpers ---------------------------------------------------------

    // Cover existence (item has ≥1 thumbnail) + first part (smallest channel_msg_id) per item.
    // Both are global (cheap) and intersected by membership when building summaries.
    private async Task<(HashSet<long> CoverItems, Dictionary<long, FirstPartRow> FirstParts)> LoadCoversAndFirstPartsAsync()
    {
        var covers = await db.QueryAsync<long>(
            "SELECT DISTINCT p.item_id FROM thumbnails t JOIN parts p ON p.id = t.part_id");

        var firsts = await db.QueryAsync<FirstPartRow>("""
            WITH first_part AS (
                SELECT p.item_id AS ItemId, p.id AS PartId, p.file_name AS FileName,
                       ROW_NUMBER() OVER (PARTITION BY p.item_id ORDER BY p.channel_msg_id) AS rn
                FROM parts p)
            SELECT ItemId, PartId, FileName FROM first_part WHERE rn = 1
            """);

        return (covers.ToHashSet(), firsts.ToDictionary(f => f.ItemId));
    }

    private async Task<Dictionary<long, List<long>>> TagsByItemAsync(long[] itemIds)
    {
        if (itemIds.Length == 0) return new();
        var rows = await db.QueryAsync<(long ItemId, long TagId)>("""
            SELECT item_id AS ItemId, tag_id AS TagId FROM item_tags WHERE item_id = ANY(@Ids)
            """, new { Ids = itemIds });
        return rows.GroupBy(r => r.ItemId).ToDictionary(g => g.Key, g => g.Select(x => x.TagId).ToList());
    }

    private static ItemSummaryDto ToSummary(
        ItemRow i,
        IReadOnlyDictionary<long, List<long>> tagsByItem,
        HashSet<long> coverItems,
        IReadOnlyDictionary<long, FirstPartRow> firstParts)
    {
        firstParts.TryGetValue(i.Id, out var fp);
        return new ItemSummaryDto(
            i.Id, i.Slug, i.Title, i.Kind, i.TotalParts, i.TotalSize,
            i.IsFavorite == 1, i.DateAdded, i.UpdatedAt, i.DeletedAt, i.FolderId,
            tagsByItem.TryGetValue(i.Id, out var t) ? t : [],
            coverItems.Contains(i.Id),
            fp?.PartId, fp?.FileName);
    }
}
