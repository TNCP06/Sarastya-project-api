using SaraDrive.Application.DTOs;
using SaraDrive.Application.Exceptions;
using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;

namespace SaraDrive.Application.Services;

public class UploadService(IUploadWriteRepository write, IDriveReadRepository read) : IUploadService
{
    private static readonly string[] ValidKinds = ["archive", "media"];

    public async Task<UploadJobDto> EnqueueAsync(UploadEnqueueDto dto)
    {
        if (!ValidKinds.Contains(dto.Kind)) throw new BadRequestException("Kind tidak valid");
        var title = (dto.Title ?? string.Empty).Trim();
        if (title.Length == 0) throw new BadRequestException("Judul tidak boleh kosong");
        if (string.IsNullOrWhiteSpace(dto.SourcePath)) throw new BadRequestException("source_path wajib diisi");

        // Browser upload finalize can be retried after a network drop. source_path is the
        // staging token directory, so returning the existing job keeps completion idempotent.
        var existing = await write.GetBySourcePathAsync(dto.SourcePath);
        if (existing is not null) return ToDto(existing);

        var job = await write.EnqueueAsync(new UploadJob
        {
            Kind = dto.Kind,
            Title = title,
            Tags = (dto.Tags ?? string.Empty).Trim(),
            SourcePath = dto.SourcePath,
            PartSize = dto.PartSize,
            Origin = dto.Origin,
            CleanupSource = dto.CleanupSource,
            TotalBytes = dto.TotalBytes,
            Status = "queued"
        });

        return ToDto(job);
    }

    public Task<IEnumerable<UploadJobDto>> GetAllAsync() => read.GetUploadsAsync();

    public async Task UpdateQueuedAsync(long id, UploadUpdateDto dto)
    {
        var title = (dto.Title ?? string.Empty).Trim();
        if (title.Length == 0) throw new BadRequestException("Judul tidak boleh kosong");
        if (!await write.UpdateQueuedAsync(id, title, (dto.Tags ?? string.Empty).Trim(), dto.PartSize))
            throw new BadRequestException("Upload job tidak ditemukan atau tidak berstatus queued");
    }

    public async Task CancelAsync(long id)
    {
        if (!await write.MarkStatusAsync(id, ["queued", "pending"], "canceled", "canceled by user"))
            throw new BadRequestException("Upload job tidak bisa dibatalkan");
    }

    public async Task StartAsync(long id)
    {
        if (!await write.MarkStatusAsync(id, ["queued"], "pending", "start requested..."))
            throw new BadRequestException("Upload job tidak bisa dimulai");
    }

    public async Task RetryAsync(long id)
    {
        if (!await write.MarkStatusAsync(id, ["error"], "pending", "retry requested..."))
            throw new BadRequestException("Upload job tidak bisa di-retry");
    }

    public Task StartAllAsync() => write.StartAllQueuedAsync();

    public Task ClearFinishedAsync() => write.ClearFinishedAsync();

    private static UploadJobDto ToDto(UploadJob job)
        => new(job.Id, job.Kind, job.Title, job.Tags, job.SourcePath, job.PartSize, job.Origin,
            job.Status, job.Progress, job.Message, job.PartsDone, job.TotalBytes, job.CreatedAt, job.UpdatedAt);
}
