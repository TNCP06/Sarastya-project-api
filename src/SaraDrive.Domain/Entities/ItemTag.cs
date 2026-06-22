namespace SaraDrive.Domain.Entities;

// Join row for the Item (N─N) Tag relationship. Composite PK (ItemId, TagId).
public class ItemTag
{
    public long ItemId { get; set; }
    public long TagId { get; set; }

    public Item Item { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
