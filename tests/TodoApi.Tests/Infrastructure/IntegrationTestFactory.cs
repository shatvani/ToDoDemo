using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

            // MigrateAsync a host teljes konfigurálása UTÁN fut — ConfigureWebHost() +
            // Wolverine IHostBuilder.ConfigureServices() is lefutott már. A DI container
            // a mi test factory-nkat adja vissza (_db.GetConnectionString()).
            using var scope = host.Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TodoDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.Migrate();

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
