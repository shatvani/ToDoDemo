using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Data.Enums;
using TodoApi.Tests.Infrastructure;
using Xunit;

namespace TodoApi.Tests.Todos
{
    public class UpdateTodoStatusTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
    {
        private readonly IntegrationTestFactory _factory;
        private readonly HttpClient _client;

        public UpdateTodoStatusTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // Open → InProgress → 200 OK
        [Fact]
        public async Task UpdateStatus_OpenToInProgress_Returns200()
        {
            // Arrange
            var todo = CreateTodo(TodoStatus.Open);
            await SeedAsync(todo);

            // Act
            var response = await _client.PatchAsJsonAsync(
                $"/api/todos/{todo.Id.Value}/status",
                new { status = "InProgress" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Open → Cancelled → 200 OK
        [Fact]
        public async Task UpdateStatus_OpenToCancelled_Returns200()
        {
            // Arrange
            var todo = CreateTodo(TodoStatus.Open);
            await SeedAsync(todo);

            // Act
            var response = await _client.PatchAsJsonAsync(
                $"/api/todos/{todo.Id.Value}/status",
                new { status = "Cancelled" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // InProgress → Done → 200 OK
        [Fact]
        public async Task UpdateStatus_InProgressToDone_Returns200()
        {
            // Arrange
            var todo = CreateTodo(TodoStatus.InProgress);
            await SeedAsync(todo);

            // Act
            var response = await _client.PatchAsJsonAsync(
                $"/api/todos/{todo.Id.Value}/status",
                new { status = "Done" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // InProgress → Open → 200 OK
        [Fact]
        public async Task UpdateStatus_InProgressToOpen_Returns200()
        {
            // Arrange
            var todo = CreateTodo(TodoStatus.InProgress);
            await SeedAsync(todo);

            // Act
            var response = await _client.PatchAsJsonAsync(
                $"/api/todos/{todo.Id.Value}/status",
                new { status = "Open" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Done → Open → 422 Unprocessable Entity
        [Fact]
        public async Task UpdateStatus_DoneToOpen_Returns422()
        {
            // Arrange
            var todo = CreateTodo(TodoStatus.Done);
            await SeedAsync(todo);

            // Act
            var response = await _client.PatchAsJsonAsync(
                $"/api/todos/{todo.Id.Value}/status",
                new { status = "Open" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        // Cancelled → Open → 422 Unprocessable Entity
        [Fact]
        public async Task UpdateStatus_CancelledToOpen_Returns422()
        {
            // Arrange
            var todo = CreateTodo(TodoStatus.Cancelled);
            await SeedAsync(todo);

            // Act
            var response = await _client.PatchAsJsonAsync(
                $"/api/todos/{todo.Id.Value}/status",
                new { status = "Open" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        // Nem létező ID → 404 Not Found
        [Fact]
        public async Task UpdateStatus_NonExistingId_Returns404()
        {
            // Act
            var response = await _client.PatchAsJsonAsync(
                $"/api/todos/{Guid.NewGuid()}/status",
                new { status = "InProgress" });

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

        private static TodoItem CreateTodo(TodoStatus status) => new()
        {
            Id = UserId.New(),
            Title = "Test Todo",
            Status = status,
            Priority = TodoPriority.Medium,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
