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
            var queryable = db.Todos.AsQueryable();

            if (query.Status is not null)
            {
                queryable = queryable.Where(x => x.Status.ToString() == query.Status);
            }

            if (query.Priority is not null)
            {
                queryable = queryable.Where(x => x.Priority.ToString() == query.Priority);
            }

            if (query.Tag is not null)
            {
                queryable = queryable.Where(x => x.Tags != null && x.Tags.Contains(query.Tag));
            }

            return queryable.Select(x => new TodoItemDto(
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
