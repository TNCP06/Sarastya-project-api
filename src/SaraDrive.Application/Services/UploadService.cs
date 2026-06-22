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
        var title = dto.Title.Trim();
        if (title.Length == 0) throw new BadRequestException("Judul tidak boleh kosong");
        if (string.IsNullOrWhiteSpace(dto.SourcePath)) throw new BadRequestException("source_path wajib diisi");

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

        return new UploadJobDto(job.Id, job.Kind, job.Title, job.Tags, job.Status, job.Progress,
            job.Message, job.PartsDone, job.TotalBytes, job.CreatedAt, job.UpdatedAt);
    }

    public Task<IEnumerable<UploadJobDto>> GetAllAsync() => read.GetUploadsAsync();
}
