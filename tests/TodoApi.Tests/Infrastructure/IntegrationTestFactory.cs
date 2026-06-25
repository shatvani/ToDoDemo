using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using TodoApi.Data;
using TodoApi.Infrastructure.SaveChangesInterceptor;
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

            // Közvetlen DbContext — bypass-olja a DI-t és a Wolverine host-szintű
            // service-regisztrációit, amelyek a ConfigureWebHost() UTÁN futnak.
            await using var db = CreateDirectDbContext();
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

                services.AddSingleton<UpdatedAtInterceptor>();
                services.AddDbContextFactory<TodoDbContext>((sp, opts) =>
                {
                    opts.UseSqlServer(_db.GetConnectionString());
                    opts.AddInterceptors(sp.GetRequiredService<UpdatedAtInterceptor>());
                });
            });
        }

        public async Task ResetDatabaseAsync()
        {
            await using var db = CreateDirectDbContext();
            await db.Set<TodoItem>().ExecuteDeleteAsync();
        }

        private TodoDbContext CreateDirectDbContext()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseSqlServer(_db.GetConnectionString())
                .Options;
            return new TodoDbContext(options);
        }
    }
}
