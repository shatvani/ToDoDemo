using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using Wolverine.Http;

namespace TodoApi.Features.Todos.UpdateTodo
{
    public class UpdateTodoHandler(IDbContextFactory<TodoDbContext> dbFactory)
    {
        [WolverinePut("/api/todos/{id}")]
        public async Task<Results<Ok<TodoItemDto>, NotFound>> Handle(Guid id, UpdateTodoCommand command)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var todo = await db.Todos.FindAsync(new UserId(id));

            if (todo == null)
            {
                return TypedResults.NotFound();
            }

            todo.Title = command.Title;
            todo.Description = command.Description;
            todo.Priority = command.Priority;
            todo.Tags = command.Tags;
            todo.UpdatedAt = DateTime.UtcNow;
            todo.DueDate = command.DueDate.HasValue ? command.DueDate.Value.UtcDateTime : null;

            await db.SaveChangesAsync();

            var dto = new TodoItemDto(
                todo.Id.Value,
                todo.Title,
                todo.Description,
                todo.Status.ToString(),
                todo.Priority.ToString(),
                todo.CreatedAt,
                todo.DueDate,
                todo.Tags);

            return TypedResults.Ok(dto);
        }
    }
}
