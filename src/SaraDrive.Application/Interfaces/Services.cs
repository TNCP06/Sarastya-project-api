using SaraDrive.Application.DTOs;

namespace SaraDrive.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto> GetMeAsync(long userId);
}

public interface IDriveService
{
    Task<DriveDto> GetDriveAsync(string space);
    Task<ItemDetailDto> GetItemAsync(long id);
    Task<IEnumerable<ItemSummaryDto>> SearchAsync(string query, string space);
    Task<IEnumerable<GalleryPartDto>> GetGalleryAsync(long itemId);
    Task<IEnumerable<ItemSummaryDto>> GetTrashAsync();
    Task<StreamInfoDto> GetStreamInfoAsync(long itemId);
    Task<IEnumerable<SubtitleDto>> GetSubtitlesAsync(long partId);
}

public interface IFolderService
{
    Task<FolderDto> CreateAsync(FolderCreateDto dto);
    Task<FolderDto> RenameAsync(long id, FolderRenameDto dto);
    Task MoveAsync(long id, FolderMoveDto dto);
    Task DeleteAsync(long id);
}

public interface IItemService
{
    Task<ItemDetailDto> UpdateAsync(long id, ItemUpdateDto dto);
    Task SetFavoriteAsync(long id, bool value);
    Task SetPrivateAsync(long id, bool value);
    Task MoveAsync(long id, long? folderId);
    Task SoftDeleteAsync(long id);
    Task RestoreAsync(long id);
    Task PurgeAsync(long id);
}

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllAsync();
    Task<TagDto> CreateAsync(TagUpsertDto dto);
    Task<TagDto> UpdateAsync(long id, TagUpsertDto dto);
    Task DeleteAsync(long id);
}

public interface IUploadService
{
    Task<UploadJobDto> EnqueueAsync(UploadEnqueueDto dto);
    Task<IEnumerable<UploadJobDto>> GetAllAsync();
}
