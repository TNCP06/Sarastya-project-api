namespace SaraDrive.Domain.Entities;

// A generated subtitle track for a part. One row per language ('orig' or ISO-639-1).
// The VTT bytes live on the streamer's volume; this row only advertises availability.
// Composite PK (PartId, Lang).
public class Subtitle
{
    public long PartId { get; set; }
    public string Lang { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
