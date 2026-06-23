using SaraDrive.Application.DTOs;
using SaraDrive.Application.Exceptions;
using SaraDrive.Application.Interfaces;
using SaraDrive.Application.Options;

namespace SaraDrive.Application.Services;

// Read-side orchestration. `space` is "main" (public) or "private" (PIN-gated); anything
// else is treated as main. All methods are behind [Authorize] at the controller.
public class DriveService(IDriveReadRepository read, StreamerSettings streamer) : IDriveService
{
    private static bool IsPrivate(string space)
        => string.Equals(space, "private", StringComparison.OrdinalIgnoreCase);

    public Task<DriveDto> GetDriveAsync(string space)
        => read.GetDriveAsync(IsPrivate(space));

    public async Task<ItemDetailDto> GetItemAsync(long id)
        => await read.GetItemAsync(id) ?? throw new NotFoundException();

    public Task<IEnumerable<ItemSummaryDto>> SearchAsync(string query, string space)
        => read.SearchAsync((query ?? string.Empty).Trim(), IsPrivate(space));

    public Task<IEnumerable<GalleryPartDto>> GetGalleryAsync(long itemId)
        => read.GetGalleryAsync(itemId);

    public Task<IEnumerable<ItemSummaryDto>> GetTrashAsync()
        => read.GetTrashAsync();

    public async Task<StreamInfoDto> GetStreamInfoAsync(long itemId)
        => await read.GetStreamInfoAsync(itemId, streamer.BaseUrl) ?? throw new NotFoundException();

    public Task<IEnumerable<SubtitleDto>> GetSubtitlesAsync(long partId)
        => read.GetSubtitlesAsync(partId);
}
