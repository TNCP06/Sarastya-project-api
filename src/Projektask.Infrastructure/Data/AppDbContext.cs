using Microsoft.EntityFrameworkCore;
using Projektask.Domain.Entities;

namespace Projektask.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        // Petakan nama tabel ke snake_case sesuai skema SQL di kontrak
        model.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
        });

        model.Entity<Project>(e =>
        {
            e.ToTable("projects");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UserId).HasColumnName("user_id");
            e.Property(p => p.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            e.Property(p => p.Description).HasColumnName("description").HasMaxLength(1000);
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.HasOne(p => p.User).WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<TaskItem>(e =>
        {
            e.ToTable("tasks");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.ProjectId).HasColumnName("project_id");
            e.Property(t => t.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
            e.Property(t => t.Description).HasColumnName("description").HasMaxLength(1000);
            e.Property(t => t.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("todo").IsRequired();
            e.Property(t => t.DueDate).HasColumnName("due_date");
            e.Property(t => t.CreatedAt).HasColumnName("created_at");
            e.HasOne(t => t.Project).WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);
            // CHECK constraint sesuai skema di kontrak — pakai ToTable() builder (EF Core 8 way)
            e.ToTable(t => t.HasCheckConstraint("CHK_tasks_status",
                "status IN ('todo','in_progress','done')"));
        });
    }
}
