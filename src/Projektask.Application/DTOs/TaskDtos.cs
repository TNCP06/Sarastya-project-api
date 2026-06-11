namespace Projektask.Application.DTOs;

public record TaskUpsertDto(
    string Title,
    string? Description,
    string Status,
    DateOnly? DueDate);

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    string Status,
    DateOnly? DueDate,
    DateTime CreatedAt);
