namespace SaraDrive.Domain.Entities;

// A drive folder. Self-referencing tree via ParentId (ON DELETE CASCADE in the DB).
// `IsPrivate` partitions the drive into the public Main space and the PIN-gated Private space.
public class Folder
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public bool IsPrivate { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public string? DeletedAt { get; set; }

    public Folder? Parent { get; set; }
    public ICollection<Folder> Children { get; set; } = [];
    public ICollection<Item> Items { get; set; } = [];
}
