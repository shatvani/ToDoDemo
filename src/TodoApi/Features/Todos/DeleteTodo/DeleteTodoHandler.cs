using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using Wolverine.Http;

namespace TodoApi.Features.Todos.DeleteTodo
{
    public class DeleteTodoHandler(IDbContextFactory<TodoDbContext> dbFactory)
    {
        [WolverineDelete("/api/todos/{id}")]
        public async Task<Results<NoContent, NotFound>> Handle(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var todo = await db.Todos.FindAsync(new UserId(id));

            if (todo == null)
            {
                return TypedResults.NotFound();
            }

            db.Todos.Remove(todo);
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        }
    }
}
