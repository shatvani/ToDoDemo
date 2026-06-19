using Spectre.Console;
using TodoApi.Data;
using TodoApi.DTOs;
using Wolverine.Http;

namespace TodoApi.Features.Todos.GetTodos
{
    public class GetTodosHandler
    {
        private readonly TodoDbContext _db;

        public GetTodosHandler(TodoDbContext db)
        {
            _db = db;
        }

        [WolverineGet("/api/todos")]
        public async Task<IEnumerable<TodoItemDto>> Handle(
            GetTodosQuery query, TodoDbContext db)
        {
            return _db.Todos.Select(x => new TodoItemDto(
                x.Id.Value,
                x.Title,
                x.Description,
                x.Status.ToString(),
                x.Priority.ToString(),
                x.CreatedAt,
                x.DueDate,
                x.Tags)).ToList();
        }
    }
}
