using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.Data.Enums;
using TodoApi.DTOs;
using TodoApi.Tests.Infrastructure;
using Xunit;

namespace TodoApi.Tests.Todos
{
    [Collection("Integration")]
    public class GetTodoByIdTests : IAsyncLifetime
    {
        private readonly IntegrationTestFactory _factory;
        private readonly HttpClient _client;

        public GetTodoByIdTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // Létező ID → 200 OK + helyes DTO
        [Fact]
        public async Task GetTodoById_ExistingId_Returns200WithCorrectDto()
        {
            // Arrange
            var todo = CreateTodo("My Task", TodoStatus.InProgress, TodoPriority.High, ["urgent"]);
            await SeedAsync(todo);

            // Act
            var response = await _client.GetAsync($"/api/todos/{todo.Id.Value}");
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result!.Id.Should().Be(todo.Id.Value);
            result.Title.Should().Be("My Task");
            result.Status.Should().Be("InProgress");
            result.Priority.Should().Be("High");
            result.Tags.Should().BeEquivalentTo(["urgent"]);
        }

        // Nem létező ID → 404 Not Found
        [Fact]
        public async Task GetTodoById_NonExistingId_Returns404()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/todos/{nonExistingId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Érvénytelen GUID formátum → 404 (Wolverine {id:guid} route constraint miatt a route nem illeszkedik)
        [Fact]
        public async Task GetTodoById_InvalidGuidFormat_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/todos/not-a-valid-guid");

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

        private static TodoItem CreateTodo(
            string title,
            TodoStatus status,
            TodoPriority priority,
            string[]? tags = null) => new()
            {
                Id = UserId.New(),
                Title = title,
                Status = status,
                Priority = priority,
                Tags = tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
    }
}
