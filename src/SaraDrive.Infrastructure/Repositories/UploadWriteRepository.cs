using Microsoft.EntityFrameworkCore;
using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;
using SaraDrive.Infrastructure.Data;

namespace SaraDrive.Infrastructure.Repositories;

// Upload queue writes via EF Core. The Python watcher executes the actual Telegram upload.
public class UploadWriteRepository(AppDbContext db) : IUploadWriteRepository
{
    public async Task<UploadJob> EnqueueAsync(UploadJob job)
    {
        db.UploadJobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    public async Task<UploadJob?> GetBySourcePathAsync(string sourcePath)
        => await db.UploadJobs.FirstOrDefaultAsync(j => j.SourcePath == sourcePath);

    public async Task<bool> UpdateQueuedAsync(long id, string title, string tags, int? partSize)
    {
        var update = db.UploadJobs
            .Where(j => j.Id == id && j.Status == "queued")
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.Title, title)
                .SetProperty(j => j.Tags, tags)
                .SetProperty(j => j.UpdatedAt, SqlTime.NowText()));

        var count = await update;
        if (count > 0 && partSize is > 0)
        {
            await db.UploadJobs
                .Where(j => j.Id == id && j.Status == "queued")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(j => j.PartSize, partSize.Value)
                    .SetProperty(j => j.UpdatedAt, SqlTime.NowText()));
        }
        return count > 0;
    }

    public async Task<bool> MarkStatusAsync(long id, string[] allowedStatuses, string status, string message)
    {
        var count = await db.UploadJobs
            .Where(j => j.Id == id && allowedStatuses.Contains(j.Status))
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.Status, status)
                .SetProperty(j => j.Message, message)
                .SetProperty(j => j.UpdatedAt, SqlTime.NowText()));
        return count > 0;
    }

    public async Task StartAllQueuedAsync()
        => await db.UploadJobs
            .Where(j => j.Status == "queued")
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.Status, "pending")
                .SetProperty(j => j.Message, "start requested...")
                .SetProperty(j => j.UpdatedAt, SqlTime.NowText()));

    public async Task ClearFinishedAsync()
        => await db.UploadJobs
            .Where(j => j.Status == "done" || j.Status == "error" || j.Status == "canceled")
            .ExecuteDeleteAsync();
}
