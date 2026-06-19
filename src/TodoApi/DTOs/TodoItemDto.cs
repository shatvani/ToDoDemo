namespace TodoApi.DTOs
{
    public record TodoItemDto(
        Guid Id,
        string Title,
        string? Description,
        string Status,
        string Priority,
        DateTimeOffset CreatedAt,
        DateTimeOffset? DueDate,
        string[]? Tags);
}
