namespace TodoApi.Features.Todos.GetTodos
{
    public record GetTodosQuery(
        string? Status,
        string? Priority,
        string? Tag);
}
