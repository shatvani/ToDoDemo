using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
        }

        public new async Task DisposeAsync()
        {
            await _db.DisposeAsync();
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TodoDbContext>>();
            using var db = dbFactory.CreateDbContext();

            var factoryConn = db.Database.GetConnectionString() ?? "(null)";
            var containerConn = _db.GetConnectionString();

            // ---- ideiglenes diagnosztika ----
            Console.WriteLine($"[TEST] Conn: {db.Database.GetConnectionString()}");
            Console.WriteLine($"[TEST] All migrations: {string.Join(", ", db.Database.GetMigrations())}");
            Console.WriteLine($"[TEST] Pending: {string.Join(", ", db.Database.GetPendingMigrations())}");
            // ----------------------------------

            db.Database.Migrate();

            // T-57 DIAGNOSTIC: ellenőrzés, hogy a Migrate() a helyes DB-n futott-e
            using var conn = db.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CAST(CASE WHEN OBJECT_ID('Todos','U') IS NOT NULL THEN 1 ELSE 0 END AS INT)";
            var tableExists = (int)cmd.ExecuteScalar()! == 1;

            if (!tableExists)
            {
                throw new InvalidOperationException(
                    $"T-57: Migrate() lefutott, de a Todos tábla nem létezik. " +
                    $"Factory connection: [{factoryConn}] | " +
                    $"Container connection: [{containerConn}]");
            }
            return host;
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
            using var scope = Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TodoDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await db.Set<TodoItem>().ExecuteDeleteAsync();
        }
    }
}
