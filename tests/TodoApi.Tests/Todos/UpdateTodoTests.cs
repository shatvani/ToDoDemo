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
    public class UpdateTodoTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
    {
        private readonly IntegrationTestFactory _factory;
        private readonly HttpClient _client;

        public UpdateTodoTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // Sikeres módosítás → 200 OK + frissített DTO
        [Fact]
        public async Task UpdateTodo_ValidCommand_Returns200WithUpdatedDto()
        {
            // Arrange
            var todo = CreateTodo("Original Title", TodoPriority.Low);
            await SeedAsync(todo);
            var command = new { title = "Updated Title", description = "New desc", priority = "High", tags = new[] { "updated" } };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todos/{todo.Id.Value}", command);
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result!.Id.Should().Be(todo.Id.Value);
            result.Title.Should().Be("Updated Title");
            result.Description.Should().Be("New desc");
            result.Priority.Should().Be("High");
            result.Tags.Should().BeEquivalentTo(["updated"]);
        }

        // UpdatedAt frissült az interceptor által
        [Fact]
        public async Task UpdateTodo_ValidCommand_UpdatedAtIsRefreshed()
        {
            // Arrange
            var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
            var todo = CreateTodo("Task", TodoPriority.Medium, updatedAt: originalUpdatedAt);
            await SeedAsync(todo);
            var command = new { title = "Changed", priority = "Medium" };

            // Act
            await _client.PutAsJsonAsync($"/api/todos/{todo.Id.Value}", command);

            // Assert — DB-ből kiolvasva ellenőrzünk, nem a DTO-ból
            var dbTodo = await GetFromDbAsync(todo.Id);
            dbTodo!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        // Nem létező ID → 404 Not Found
        [Fact]
        public async Task UpdateTodo_NonExistingId_Returns404()
        {
            // Arrange
            var command = new { title = "Title", priority = "Medium" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todos/{Guid.NewGuid()}", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // Üres Title → 400 Bad Request
        [Fact]
        public async Task UpdateTodo_EmptyTitle_Returns400()
        {
            // Arrange
            var todo = CreateTodo("Task", TodoPriority.Medium);
            await SeedAsync(todo);
            var command = new { title = "", priority = "Medium" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todos/{todo.Id.Value}", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // Title > 200 karakter → 400 Bad Request
        [Fact]
        public async Task UpdateTodo_TitleTooLong_Returns400()
        {
            // Arrange
            var todo = CreateTodo("Task", TodoPriority.Medium);
            await SeedAsync(todo);
            var command = new { title = new string('x', 201), priority = "Medium" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todos/{todo.Id.Value}", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // DueDate a múltban → 400 Bad Request
        [Fact]
        public async Task UpdateTodo_DueDateInPast_Returns400()
        {
            // Arrange
            var todo = CreateTodo("Task", TodoPriority.Medium);
            await SeedAsync(todo);
            var command = new { title = "Task", priority = "Medium", dueDate = DateTimeOffset.UtcNow.AddDays(-1) };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todos/{todo.Id.Value}", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

        private async Task<TodoItem?> GetFromDbAsync(UserId id)
        {
            using var scope = _factory.Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TodoDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            return await db.Todos.FindAsync(id);
        }

        private static TodoItem CreateTodo(
            string title,
            TodoPriority priority,
            DateTime? updatedAt = null) => new()
            {
                Id = UserId.New(),
                Title = title,
                Status = TodoStatus.Open,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = updatedAt ?? DateTime.UtcNow,
            };
    }
}
