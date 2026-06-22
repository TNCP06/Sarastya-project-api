namespace SaraDrive.Application.DTOs;

// --- Folders ---
public record FolderCreateDto(string Name, long? ParentId, bool IsPrivate = false);
public record FolderRenameDto(string Name);
public record FolderMoveDto(long? TargetParentId);

// --- Items ---
// Metadata edit. `Tags` replaces the item's tag set when non-null (empty list clears them);
// null leaves tags unchanged. `Slug` is intentionally not editable (immutable grouping key).
public record ItemUpdateDto(string Title, string Kind, IEnumerable<string>? Tags);
public record ItemFavoriteDto(bool Value);
public record ItemPrivateDto(bool Value);
public record ItemMoveDto(long? FolderId);

// --- Tags ---
public record TagUpsertDto(string Name, string? Color);

// --- Uploads ---
public record UploadEnqueueDto(
    string Kind,
    string Title,
    string? Tags,
    string SourcePath,
    int PartSize = 1500,
    long TotalBytes = 0,
    bool CleanupSource = false,
    string Origin = "upload");

public record UploadJobDto(
    long Id,
    string Kind,
    string Title,
    string Tags,
    string Status,
    int Progress,
    string? Message,
    int PartsDone,
    long TotalBytes,
    string CreatedAt,
    string UpdatedAt);

// --- Helpers ---
public record StreamPartDto(long PartId, int PartNumber, string? FileName, long ChannelMsgId, string StreamUrl);
public record StreamInfoDto(long ItemId, string Kind, string StreamerBase, IEnumerable<StreamPartDto> Parts);
public record SubtitleDto(string Lang, string CreatedAt);
