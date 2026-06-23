namespace SaraDrive.Application.DTOs;

// One file in a drive listing. Timestamps are returned as raw 'YYYY-MM-DD HH:MM:SS' (UTC)
// strings — the same format the existing web/mobile clients already parse. Presentation
// concerns (title→family/version parsing, thumb URL, color derivation) stay on the client.
public record ItemSummaryDto(
    long Id,
    string Slug,
    string Title,
    string Kind,
    int TotalParts,
    long TotalSize,
    bool IsFavorite,
    string DateAdded,
    string UpdatedAt,
    string? DeletedAt,
    long? FolderId,
    IEnumerable<long> Tags,
    bool HasThumb,
    long? FirstPartId,
    string? FirstPartFileName);

public record TagDto(long Id, string Name, string Color);

public record FolderDto(
    long Id,
    string Name,
    long? ParentId,
    bool IsPrivate,
    string CreatedAt,
    string UpdatedAt,
    string? DeletedAt);

// The full payload for one space (Main or Private): the shape the web's getDriveData expects.
public record DriveDto(
    IEnumerable<ItemSummaryDto> Files,
    IEnumerable<TagDto> Tags,
    IEnumerable<FolderDto> Folders);

// A single part of an item (for item detail / stream-info).
public record PartDto(
    long Id,
    int PartNumber,
    long ChannelMsgId,
    string? FileName,
    long FileSize,
    string? UploadedAt,
    bool HasThumb);

public record ItemDetailDto(
    long Id,
    string Slug,
    string Title,
    string Kind,
    int TotalParts,
    long TotalSize,
    bool IsFavorite,
    bool IsPrivate,
    string DateAdded,
    string UpdatedAt,
    string? DeletedAt,
    long? FolderId,
    IEnumerable<string> Tags,
    IEnumerable<PartDto> Parts);

// One thumbnail in a media item's gallery (base64 bytes for the preview drawer).
public record GalleryPartDto(long PartId, string? FileName, string Mime, string Data);
