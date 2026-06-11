namespace Projektask.Application.DTOs;

public record ProjectUpsertDto(string Name, string? Description);

public record ProjectListDto(
    int Id,
    string Name,
    string? Description,
    int TaskCount,
    int DoneTaskCount,
    DateTime CreatedAt);

public record ProjectResponseDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt);

public record ProjectDetailDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IEnumerable<TaskDto> Tasks);
