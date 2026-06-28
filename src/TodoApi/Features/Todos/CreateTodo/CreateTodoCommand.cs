using TodoApi.Data.Enums;

namespace TodoApi.Features.Todos.CreateTodo
{
    public record CreateTodoCommand(
        string Title,
        string? Description = null,
        TodoPriority Priority = TodoPriority.Medium,
        DateTimeOffset? DueDate = null,
        string[]? Tags = null);
}
