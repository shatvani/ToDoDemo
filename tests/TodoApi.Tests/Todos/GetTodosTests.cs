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
    /*
     * [Collection("Integration")] — megosztott IntegrationTestFactory fixture az összes tesztosztályban (1 konténer az egész teszt assembly-hez)
     * IAsyncLifetime.InitializeAsync() — ResetDatabaseAsync() törli a Todos táblát minden teszt előtt
     */
    [Collection("Integration")]
    public class GetTodosTests : IAsyncLifetime
    {
        private readonly IntegrationTestFactory _factory;
        private readonly HttpClient _client;

        public GetTodosTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();

        public Task DisposeAsync() => Task.CompletedTask;

        // 200 OK + [] üres JSON tömb
        [Fact]
        public async Task GetTodos_EmptyDatabase_Returns200WithEmptyArray()
        {
            // Act
            var response = await _client.GetAsync("/api/todos");
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().BeEmpty();
        }

        // 3 beseeded elem → 3 visszakapott elem
        [Fact]
        public async Task GetTodos_WithData_ReturnsAllItems()
        {
            // Arrange
            await SeedAsync(
                CreateTodo("Todo 1", TodoStatus.Open, TodoPriority.Low),
                CreateTodo("Todo 2", TodoStatus.InProgress, TodoPriority.High),
                CreateTodo("Todo 3", TodoStatus.Done, TodoPriority.Medium)
            );

            // Act
            var response = await _client.GetAsync("/api/todos");
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().HaveCount(3);
        }

        // ?status=Open → csak Open státuszú elemek
        [Fact]
        public async Task GetTodos_FilterByStatus_ReturnsOnlyMatchingItems()
        {
            // Arrange
            await SeedAsync(
                CreateTodo("Open Todo", TodoStatus.Open, TodoPriority.Low),
                CreateTodo("InProgress Todo", TodoStatus.InProgress, TodoPriority.Low),
                CreateTodo("Done Todo", TodoStatus.Done, TodoPriority.Low)
            );

            // Act
            var response = await _client.GetAsync("/api/todos?status=Open");
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().HaveCount(1)
                .And.OnlyContain(x => x.Status == "Open");
        }

        // ?priority=High → csak High prioritású elemek
        [Fact]
        public async Task GetTodos_FilterByPriority_ReturnsOnlyMatchingItems()
        {
            // Arrange
            await SeedAsync(
                CreateTodo("Low Todo", TodoStatus.Open, TodoPriority.Low),
                CreateTodo("High Todo 1", TodoStatus.Open, TodoPriority.High),
                CreateTodo("High Todo 2", TodoStatus.InProgress, TodoPriority.High)
            );

            // Act
            var response = await _client.GetAsync("/api/todos?priority=High");
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().HaveCount(2)
                .And.OnlyContain(x => x.Priority == "High");
        }

        // ?tag=urgent → csak az urgent tag-et tartalmazó elem
        [Fact]
        public async Task GetTodos_FilterByTag_ReturnsOnlyMatchingItems()
        {
            // Arrange
            await SeedAsync(
                CreateTodo("Urgent Todo", TodoStatus.Open, TodoPriority.High, tags: ["urgent", "important"]),
                CreateTodo("Normal Todo", TodoStatus.Open, TodoPriority.Low, tags: ["normal"]),
                CreateTodo("No Tag Todo", TodoStatus.Open, TodoPriority.Low)
            );

            // Act
            var response = await _client.GetAsync("/api/todos?tag=urgent");
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Urgent Todo");
        }

        // ?status=InvalidStatus → string-összehasonlítás sosem lesz igaz → 200 + []
        [Fact]
        public async Task GetTodos_InvalidStatusFilter_Returns200WithEmptyArray()
        {
            // Arrange
            await SeedAsync(
                CreateTodo("Todo 1", TodoStatus.Open, TodoPriority.Low)
            );

            // Act
            var response = await _client.GetAsync("/api/todos?status=InvalidStatus");
            var result = await response.Content.ReadFromJsonAsync<TodoItemDto[]>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().BeEmpty();
        }

        // --- Helpers ---

        // DI scope-on keresztül közvetlenül írja az adatbázist (nem HTTP-n)
        private async Task SeedAsync(params TodoItem[] items)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
            db.Todos.AddRange(items);
            await db.SaveChangesAsync();
        }

        // •	CreateTodo — static factory, csak a releváns mezőket állítja be, a többi default értéken marad
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
