using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Infrastructure
{
    public static class HostExtensions
    {
        public static async Task MigrateDbAsync(this IHost host)
        {
            await using var scope = host.Services.CreateAsyncScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TodoDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await db.Database.MigrateAsync();
        }
    }
}
