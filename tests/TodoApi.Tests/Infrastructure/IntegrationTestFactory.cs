using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
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

            // Nyers ADO.NET — bypass-olja az EF Core migration runnert és a Wolverine
            // host-szintű regisztrációit. MigrateAsync() a test context-ben 0 pending
            // migration-t talál (assembly scanning issue), ezért a tábla nem jön létre.
            await using var connection = new SqlConnection(_db.GetConnectionString());
            await connection.OpenAsync();
            await using var cmd = new SqlCommand(@"
                IF OBJECT_ID(N'[Todos]', N'U') IS NULL
                    CREATE TABLE [Todos] (
                        [Id]          uniqueidentifier NOT NULL,
                        [Title]       nvarchar(200)    NOT NULL,
                        [Description] nvarchar(max)    NULL,
                        [Status]      int              NOT NULL,
                        [Priority]    int              NOT NULL,
                        [CreatedAt]   datetime2        NOT NULL,
                        [UpdatedAt]   datetime2        NOT NULL,
                        [DueDate]     datetime2        NULL,
                        [Tags]        nvarchar(max)    NULL,
                        CONSTRAINT [PK_Todos] PRIMARY KEY ([Id])
                    )", connection);
            await cmd.ExecuteNonQueryAsync();
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
            await using var connection = new SqlConnection(_db.GetConnectionString());
            await connection.OpenAsync();
            await using var cmd = new SqlCommand("DELETE FROM [Todos]", connection);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
