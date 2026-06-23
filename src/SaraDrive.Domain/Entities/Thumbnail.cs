namespace SaraDrive.Domain.Entities;

// Per-part cover image (base64). An item's cover is the thumbnail of its first part
// (smallest channel_msg_id). PK is PartId (1─1 with Part).
public class Thumbnail
{
    public long PartId { get; set; }
    public string Mime { get; set; } = "image/jpeg";
    public string Data { get; set; } = string.Empty;

    public Part Part { get; set; } = null!;
}
