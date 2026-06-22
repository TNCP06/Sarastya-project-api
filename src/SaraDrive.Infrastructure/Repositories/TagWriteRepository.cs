using Microsoft.EntityFrameworkCore;
using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;
using SaraDrive.Infrastructure.Data;

namespace SaraDrive.Infrastructure.Repositories;

// Tag CRUD writes via EF Core.
public class TagWriteRepository(AppDbContext db) : ITagWriteRepository
{
    public async Task<Tag> CreateAsync(string name, string color)
    {
        var tag = new Tag { Name = name, Color = color };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> GetByIdAsync(long id)
        => await db.Tags.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Tag?> GetByNameAsync(string name)
        => await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        await db.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        // item_tags.tag_id is ON DELETE CASCADE, so relations clear automatically.
        var n = await db.Tags.Where(t => t.Id == id).ExecuteDeleteAsync();
        return n > 0;
    }
}
