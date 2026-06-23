using SaraDrive.Application.DTOs;
using SaraDrive.Domain.Entities;

namespace SaraDrive.Application.Interfaces;

// READ repositories — Dapper raw SQL.
public interface IUserReadRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(long id);
}

public interface IDriveReadRepository
{
    Task<DriveDto> GetDriveAsync(bool isPrivate);
    Task<ItemDetailDto?> GetItemAsync(long id);
    Task<IEnumerable<ItemSummaryDto>> SearchAsync(string query, bool isPrivate);
    Task<IEnumerable<GalleryPartDto>> GetGalleryAsync(long itemId);
    Task<IEnumerable<ItemSummaryDto>> GetTrashAsync();
    Task<IEnumerable<TagDto>> GetAllTagsAsync();
    Task<IEnumerable<UploadJobDto>> GetUploadsAsync();
    Task<StreamInfoDto?> GetStreamInfoAsync(long itemId, string streamerBase);
    Task<IEnumerable<SubtitleDto>> GetSubtitlesAsync(long partId);
}

// WRITE repositories — EF Core (raw SQL where the TEXT-timestamp / ON CONFLICT shape makes EF awkward).
public interface IUserWriteRepository
{
    Task<User> CreateAsync(User user);
}

public interface IFolderWriteRepository
{
    Task<Folder> CreateAsync(Folder folder);
    Task<Folder?> GetByIdAsync(long id);
    Task<Folder> UpdateAsync(Folder folder);
    Task DeleteRecursiveAsync(long id);
    Task<IReadOnlyList<long>> GetDescendantFolderIdsAsync(long id);
    Task SetPrivateRecursiveAsync(long id, bool value);
}

public interface IItemWriteRepository
{
    Task<Item?> GetByIdAsync(long id);
    Task<Item> UpdateAsync(Item item);
    Task ReplaceTagsAsync(long itemId, IEnumerable<string> tagNames);
    Task<bool> SetPrivateAsync(long id, bool value);
    Task<bool> SoftDeleteAsync(long id);
    Task<bool> RestoreAsync(long id);
    Task<bool> PurgeAsync(long id);
}

public interface ITagWriteRepository
{
    Task<Tag> CreateAsync(string name, string color);
    Task<Tag?> GetByIdAsync(long id);
    Task<Tag?> GetByNameAsync(string name);
    Task<Tag> UpdateAsync(Tag tag);
    Task<bool> DeleteAsync(long id);
}

public interface IUploadWriteRepository
{
    Task<UploadJob> EnqueueAsync(UploadJob job);
    Task<UploadJob?> GetBySourcePathAsync(string sourcePath);
    Task<bool> UpdateQueuedAsync(long id, string title, string tags, int? partSize);
    Task<bool> MarkStatusAsync(long id, string[] allowedStatuses, string status, string message);
    Task StartAllQueuedAsync();
    Task ClearFinishedAsync();
}
