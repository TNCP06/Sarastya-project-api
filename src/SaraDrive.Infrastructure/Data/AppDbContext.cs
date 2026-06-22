using Microsoft.EntityFrameworkCore;
using SaraDrive.Domain.Entities;

namespace SaraDrive.Infrastructure.Data;

// EF Core context used for WRITE/CUD paths only. It is mapped onto the EXISTING tables
// owned by the umbrella schema.sql (Python engine + web share them) — so there are NO EF
// migrations in production. Integer 0/1 flags (is_private, is_favorite) are mapped to bool;
// timestamps stay TEXT 'YYYY-MM-DD HH:MM:SS' (UTC) and default to now_text() at the DB.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ItemTag> ItemTags => Set<ItemTag>();
    public DbSet<Thumbnail> Thumbnails => Set<Thumbnail>();
    public DbSet<Subtitle> Subtitles => Set<Subtitle>();
    public DbSet<UploadJob> UploadJobs => Set<UploadJob>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Name).HasColumnName("name").IsRequired();
            e.Property(u => u.Email).HasColumnName("email").IsRequired();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(u => u.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
        });

        model.Entity<Folder>(e =>
        {
            e.ToTable("folders");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).HasColumnName("id");
            e.Property(f => f.Name).HasColumnName("name").IsRequired();
            e.Property(f => f.ParentId).HasColumnName("parent_id");
            e.Property(f => f.IsPrivate).HasColumnName("is_private").HasConversion<int>();
            e.Property(f => f.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
            e.Property(f => f.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
            e.Property(f => f.DeletedAt).HasColumnName("deleted_at");
            e.HasOne(f => f.Parent).WithMany(f => f.Children)
                .HasForeignKey(f => f.ParentId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Item>(e =>
        {
            e.ToTable("items");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("id");
            e.Property(i => i.Slug).HasColumnName("slug").IsRequired();
            e.HasIndex(i => i.Slug).IsUnique();
            e.Property(i => i.Title).HasColumnName("title").IsRequired();
            e.Property(i => i.Kind).HasColumnName("kind").IsRequired();
            e.Property(i => i.TotalParts).HasColumnName("total_parts");
            e.Property(i => i.TotalSize).HasColumnName("total_size");
            e.Property(i => i.IsFavorite).HasColumnName("is_favorite").HasConversion<int>();
            e.Property(i => i.IsPrivate).HasColumnName("is_private").HasConversion<int>();
            e.Property(i => i.DateAdded).HasColumnName("date_added")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
            e.Property(i => i.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
            e.Property(i => i.DeletedAt).HasColumnName("deleted_at");
            e.Property(i => i.FolderId).HasColumnName("folder_id");
            e.HasOne(i => i.Folder).WithMany(f => f.Items)
                .HasForeignKey(i => i.FolderId).OnDelete(DeleteBehavior.SetNull);
        });

        model.Entity<Part>(e =>
        {
            e.ToTable("parts");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.ItemId).HasColumnName("item_id");
            e.Property(p => p.PartNumber).HasColumnName("part_number");
            e.Property(p => p.ChannelMsgId).HasColumnName("channel_msg_id");
            e.HasIndex(p => p.ChannelMsgId).IsUnique();
            e.Property(p => p.FileName).HasColumnName("file_name");
            e.Property(p => p.FileSize).HasColumnName("file_size");
            e.Property(p => p.FileId).HasColumnName("file_id");
            e.Property(p => p.UploadedAt).HasColumnName("uploaded_at");
            e.HasOne(p => p.Item).WithMany(i => i.Parts)
                .HasForeignKey(p => p.ItemId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Tag>(e =>
        {
            e.ToTable("tags");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.Name).HasColumnName("name").IsRequired();
            e.HasIndex(t => t.Name).IsUnique();
            e.Property(t => t.Color).HasColumnName("color");
        });

        model.Entity<ItemTag>(e =>
        {
            e.ToTable("item_tags");
            e.HasKey(it => new { it.ItemId, it.TagId });
            e.Property(it => it.ItemId).HasColumnName("item_id");
            e.Property(it => it.TagId).HasColumnName("tag_id");
            e.HasOne(it => it.Item).WithMany(i => i.ItemTags)
                .HasForeignKey(it => it.ItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(it => it.Tag).WithMany(t => t.ItemTags)
                .HasForeignKey(it => it.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Thumbnail>(e =>
        {
            e.ToTable("thumbnails");
            e.HasKey(t => t.PartId);
            e.Property(t => t.PartId).HasColumnName("part_id").ValueGeneratedNever();
            e.Property(t => t.Mime).HasColumnName("mime");
            e.Property(t => t.Data).HasColumnName("data").IsRequired();
            e.HasOne(t => t.Part).WithOne(p => p.Thumbnail)
                .HasForeignKey<Thumbnail>(t => t.PartId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Subtitle>(e =>
        {
            e.ToTable("subtitles");
            e.HasKey(s => new { s.PartId, s.Lang });
            e.Property(s => s.PartId).HasColumnName("part_id");
            e.Property(s => s.Lang).HasColumnName("lang");
            e.Property(s => s.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
        });

        model.Entity<UploadJob>(e =>
        {
            e.ToTable("upload_jobs");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Kind).HasColumnName("kind").IsRequired();
            e.Property(u => u.Title).HasColumnName("title").IsRequired();
            e.Property(u => u.Tags).HasColumnName("tags");
            e.Property(u => u.SourcePath).HasColumnName("source_path").IsRequired();
            e.Property(u => u.PartSize).HasColumnName("part_size");
            e.Property(u => u.Origin).HasColumnName("origin").IsRequired();
            e.Property(u => u.CleanupSource).HasColumnName("cleanup_source").HasConversion<int>();
            e.Property(u => u.PartsDone).HasColumnName("parts_done");
            e.Property(u => u.TotalBytes).HasColumnName("total_bytes");
            e.Property(u => u.Status).HasColumnName("status").IsRequired();
            e.Property(u => u.Progress).HasColumnName("progress");
            e.Property(u => u.Message).HasColumnName("message");
            e.Property(u => u.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at")
                .HasDefaultValueSql("now_text()").ValueGeneratedOnAdd();
        });
    }
}
