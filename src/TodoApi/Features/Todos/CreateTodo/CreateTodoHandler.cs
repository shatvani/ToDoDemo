using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Data.Enums;
using TodoApi.DTOs;
using Wolverine.Http;

namespace TodoApi.Features.Todos.CreateTodo
{
    public class CreateTodoHandler(IDbContextFactory<TodoDbContext> dbFactory)
    {
        [WolverinePost("/api/todos")]
        public async Task<IResult> Handle(CreateTodoCommand command)
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            var newTodo = new TodoItem
            {
                Id = UserId.New(),
                Title = command.Title,
                Description = command.Description,
                Status = TodoStatus.Open,
                Priority = command.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DueDate = command.DueDate?.UtcDateTime,
                Tags = command.Tags,
            };

            await db.Todos.AddAsync(newTodo);
            await db.SaveChangesAsync();

            var dto = new TodoItemDto(
                newTodo.Id.Value,
                newTodo.Title,
                newTodo.Description,
                newTodo.Status.ToString(),
                newTodo.Priority.ToString(),
                newTodo.CreatedAt,
                newTodo.DueDate,
                newTodo.Tags);

            return Results.Created($"/api/todos/{newTodo.Id.Value}", dto);
        }
    }
}
