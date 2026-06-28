using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Data.Enums;
using Wolverine.Http;

namespace TodoApi.Features.Todos.UpdateTodoStatus
{
    public class UpdateTodoStatusHandler(IDbContextFactory<TodoDbContext> dbFactory)
    {
        private static readonly Dictionary<TodoStatus, HashSet<TodoStatus>> _validTransitions = new()
        {
            [TodoStatus.Open] = [TodoStatus.InProgress, TodoStatus.Cancelled],
            [TodoStatus.InProgress] = [TodoStatus.Done, TodoStatus.Cancelled, TodoStatus.Open],
            [TodoStatus.Done] = [],
            [TodoStatus.Cancelled] = [],
        };

        [WolverinePatch("/api/todos/{id}/status")]
        public async Task<Results<Ok, UnprocessableEntity, NotFound>> Handle(Guid id, UpdateTodoStatusCommand command)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var todo = await db.Todos.FindAsync(new UserId(id));

            if (todo == null)
            {
                return TypedResults.NotFound();
            }

            if (!_validTransitions[todo.Status].Contains(command.Status))
            {
                return TypedResults.UnprocessableEntity();
            }

            todo.Status = command.Status;
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}
