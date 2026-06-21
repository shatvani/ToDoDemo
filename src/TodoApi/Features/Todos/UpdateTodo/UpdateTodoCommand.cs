using TodoApi.Data.Enums;

namespace TodoApi.Features.Todos.UpdateTodo
{
    public record UpdateTodoCommand(
        Guid Id,
        string Title,
        string? Description,
        TodoPriority Priority,
        DateTimeOffset? DueDate,
        string[]? Tags);
}
