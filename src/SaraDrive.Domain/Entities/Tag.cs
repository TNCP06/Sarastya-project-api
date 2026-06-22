namespace SaraDrive.Domain.Entities;

// A label applied to items (N─N via ItemTag). `Color` is a palette key (sage, ochre, …)
// or '' to derive a deterministic color from the name on the client.
public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;

    public ICollection<ItemTag> ItemTags { get; set; } = [];
}
