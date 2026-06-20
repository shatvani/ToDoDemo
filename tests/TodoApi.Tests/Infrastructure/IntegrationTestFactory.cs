using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using TodoApi.Data;
using Xunit;

namespace TodoApi.Tests.Infrastructure
{
    public class IntegrationTestFactory
        : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _db = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public async Task InitializeAsync()
        {
            await _db.StartAsync();

            using var scope = Services.CreateScope();
            var factory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<TodoDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await db.Database.MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _db.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDbContextFactory<TodoDbContext>>();
                services.RemoveAll<DbContextOptions<TodoDbContext>>();
                services.RemoveAll<TodoDbContext>();

                services.AddDbContextFactory<TodoDbContext>(opts =>
                    opts.UseSqlServer(_db.GetConnectionString()));
            });
        }

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var factory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<TodoDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            db.Todos.RemoveRange(db.Todos);
            await db.SaveChangesAsync();
        }
    }
}
