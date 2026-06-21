using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using Wolverine.Http;

namespace TodoApi.Features.Todos.GetTodoById
{
    public class GetTodoByIdHandler(IDbContextFactory<TodoDbContext> dbFactory)
    {
        [WolverineGet("/api/todos/{id}")]
        public async Task<TodoItemDto?> Handle(
           [FromRoute] Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            var todo = await db.Todos.FindAsync(id);

            if (todo == null)
            {
                return null;
            }

            return new TodoItemDto(
                todo.Id.Value,
                todo.Title,
                todo.Description,
                todo.Status.ToString(),
                todo.Priority.ToString(),
                todo.CreatedAt,
                todo.DueDate,
                todo.Tags);
        }
    }
}
