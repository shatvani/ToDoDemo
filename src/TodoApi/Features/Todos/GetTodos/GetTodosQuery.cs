namespace TodoApi.Features.Todos.GetTodos
{
    public record GetTodosQuery(
        string? Status = null,
        string? Priority = null,
        string? Tag = null);
}
