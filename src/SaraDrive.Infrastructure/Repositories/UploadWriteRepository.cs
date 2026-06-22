using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;
using SaraDrive.Infrastructure.Data;

namespace SaraDrive.Infrastructure.Repositories;

// Enqueue an upload job via EF Core. The Python watcher LISTENs/polls upload_jobs and does the
// actual Telegram upload; the API only inserts the row (status defaults to 'queued').
public class UploadWriteRepository(AppDbContext db) : IUploadWriteRepository
{
    public async Task<UploadJob> EnqueueAsync(UploadJob job)
    {
        db.UploadJobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }
}
