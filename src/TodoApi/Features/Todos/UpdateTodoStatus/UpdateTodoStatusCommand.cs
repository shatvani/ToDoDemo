using TodoApi.Data.Enums;

namespace TodoApi.Features.Todos.UpdateTodoStatus
{
    public record UpdateTodoStatusCommand(
        TodoStatus Status);
}
