using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using TodoApi.DTOs;
using TodoApi.Tests.Infrastructure;
using Xunit;

namespace TodoApi.Tests.Todos
{
    [Collection("Integration")]
    public class CreateTodoTests : IAsyncLifetime
    {
        private readonly IntegrationTestFactory _factory;
        private readonly HttpClient _client;

        public CreateTodoTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // Sikeres létrehozás → 201 Created + Location header + DTO visszakapva
        [Fact]
        public async Task CreateTodo_ValidCommand_Returns201WithLocationAndDto()
        {
            // Arrange
            var command = new { title = "New Task", description = "Leírás", priority = "High", tags = new[] { "urgent" } };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todos", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto>();

            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().EndWith($"/api/todos/{result!.Id}");
            result.Title.Should().Be("New Task");
            result.Description.Should().Be("Leírás");
            result.Priority.Should().Be("High");
            result.Status.Should().Be("Open");
            result.Tags.Should().BeEquivalentTo(["urgent"]);
        }

        // Üres Title → 400 Bad Request + validációs hiba
        [Fact]
        public async Task CreateTodo_EmptyTitle_Returns400()
        {
            // Arrange
            var command = new { title = "" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todos", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // Túl hosszú Title (>200 karakter) → 400 Bad Request
        [Fact]
        public async Task CreateTodo_TitleTooLong_Returns400()
        {
            // Arrange
            var command = new { title = new string('x', 201) };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todos", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // Priority nem kötelező (default: Medium) → hiányzó Priority esetén 201 + Medium
        [Fact]
        public async Task CreateTodo_MissingPriority_Returns201WithMediumPriority()
        {
            // Arrange — Priority szándékosan kimarad: default értéke TodoPriority.Medium
            var command = new { title = "Task priority nélkül" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todos", command);
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            result!.Priority.Should().Be("Medium");
        }

        // Érvénytelen Priority érték → 400 Bad Request (IsInEnum FluentValidation rule)
        [Fact]
        public async Task CreateTodo_InvalidPriorityValue_Returns400()
        {
            // Arrange — 999 érvénytelen enum érték, raw JSON-ként küldve
            var json = """{"title": "Task", "priority": 999}""";
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/todos", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // Description > 500 karakter → 400 Bad Request
        [Fact]
        public async Task CreateTodo_DescriptionTooLong_Returns400()
        {
            // Arrange
            var command = new { title = "Task", description = new string('x', 501) };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todos", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // DueDate a múltban → 400 Bad Request
        [Fact]
        public async Task CreateTodo_DueDateInPast_Returns400()
        {
            // Arrange
            var command = new { title = "Task", dueDate = DateTimeOffset.UtcNow.AddDays(-1) };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todos", command);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // DueDate null → 201 Created (opcionális mező, GreaterThan null-ra nem fut le)
        [Fact]
        public async Task CreateTodo_NullDueDate_Returns201()
        {
            // Arrange
            var command = new { title = "Task határidő nélkül", dueDate = (DateTimeOffset?)null };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todos", command);
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            result!.DueDate.Should().BeNull();
        }
    }
}
