using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Data.Enums;
using TodoApi.Tests.Infrastructure;
using Xunit;

namespace TodoApi.Tests.Todos
{
    [Collection("Integration")]
    public class DeleteTodoTests : IAsyncLifetime
    {
        private readonly IntegrationTestFactory _factory;
        private readonly HttpClient _client;

        public DeleteTodoTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // Sikeres törlés → 204 No Content
        [Fact]
        public async Task DeleteTodo_ExistingId_Returns204()
        {
            // Arrange
            var todo = CreateTodo();
            await SeedAsync(todo);

            // Act
            var response = await _client.DeleteAsync($"/api/todos/{todo.Id.Value}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Törlés után GET /api/todos/{id} → 404 Not Found
        [Fact]
        public async Task DeleteTodo_AfterDelete_GetReturns404()
        {
            // Arrange
            var todo = CreateTodo();
            await SeedAsync(todo);

            // Act
            await _client.DeleteAsync($"/api/todos/{todo.Id.Value}");
            var getResponse = await _client.GetAsync($"/api/todos/{todo.Id.Value}");

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Nem létező ID → 404 Not Found
        [Fact]
        public async Task DeleteTodo_NonExistingId_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/todos/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // --- Helpers ---

        private async Task SeedAsync(params TodoItem[] items)
        {
            using var scope = _factory.Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TodoDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            db.Todos.AddRange(items);
            await db.SaveChangesAsync();
        }

        private static TodoItem CreateTodo() => new()
        {
            Id = UserId.New(),
            Title = "Test Todo",
            Status = TodoStatus.Open,
            Priority = TodoPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
