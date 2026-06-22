namespace SaraDrive.Domain.Entities;

// One physical piece of an Item — a single message in the storage channel.
// `ChannelMsgId` is UNIQUE: the re-index idempotency key and the copy_message download target.
public class Part
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public int PartNumber { get; set; }
    public long ChannelMsgId { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? FileId { get; set; }
    public string? UploadedAt { get; set; }

    public Item Item { get; set; } = null!;
    public Thumbnail? Thumbnail { get; set; }
}
