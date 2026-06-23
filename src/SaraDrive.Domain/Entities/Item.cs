namespace SaraDrive.Domain.Entities;

// A drive file — one logical file made of one or more Parts (Telegram channel messages).
// `kind` is 'archive' (multi-part download) or 'media' (streamable). `slug` is immutable:
// it is the multi-part grouping key and the download deep-link target, so rename never
// changes it. Soft delete is `DeletedAt` (NULL = active; set = in Trash).
public class Item
{
    public long Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Kind { get; set; } = "archive";
    public int TotalParts { get; set; }
    public long TotalSize { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPrivate { get; set; }
    public string DateAdded { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public string? DeletedAt { get; set; }
    public long? FolderId { get; set; }

    public Folder? Folder { get; set; }
    public ICollection<Part> Parts { get; set; } = [];
    public ICollection<ItemTag> ItemTags { get; set; } = [];
}
