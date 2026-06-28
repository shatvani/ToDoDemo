using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TodoApi.Data;

namespace TodoApi.Infrastructure.SaveChangesInterceptor
{
    public class UpdatedAtInterceptor : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context != null)
            {
                foreach (var entry in context.ChangeTracker.Entries<TodoItem>()
                    .Where(e => e.State == EntityState.Modified))
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
