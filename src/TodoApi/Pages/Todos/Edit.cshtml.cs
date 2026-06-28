using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApi.DTOs;

namespace TodoApi.Pages.Todos
{
    [IgnoreAntiforgeryToken]
    public class EditModel(
        IHttpClientFactory httpClientFactory) : PageModel
    {
        [BindProperty]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public string Priority { get; set; } = "Medium";

        [BindProperty]
        public DateTime? DueDate { get; set; }

        [BindProperty]
        public string? TagsRaw { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var client = httpClientFactory.CreateClient("TodoApi");
            var todo = await client.GetFromJsonAsync<TodoItemDto>($"/api/todos/{id}");

            if (todo is null)
            {
                return NotFound();
            }

            return Partial("_EditForm", todo);
        }

        public async Task<IActionResult> OnPutAsync(Guid id)
        {
            var client = httpClientFactory.CreateClient("TodoApi");

            var tags = string.IsNullOrWhiteSpace(TagsRaw)
                ? null
                : TagsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Where(t => t.Length > 0)
                         .ToArray();

            var payload = new
            {
                title = Title,
                description = Description,
                priority = Priority,
                dueDate = DueDate.HasValue ? (DateTimeOffset?)new DateTimeOffset(DueDate.Value, TimeSpan.Zero) : null,
                tags,
            };

            var putResponse = await client.PutAsJsonAsync($"/api/todos/{id}", payload);

            if (!putResponse.IsSuccessStatusCode)
            {
                if ((int)putResponse.StatusCode == 400)
                {
                    var stream = await putResponse.Content.ReadAsStreamAsync();
                    using var jsonDoc = await JsonDocument.ParseAsync(stream);
                    if (jsonDoc.RootElement.TryGetProperty("errors", out var errorsElement))
                    {
                        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                        foreach (var prop in errorsElement.EnumerateObject())
                        {
                            errors[prop.Name] = prop.Value.EnumerateArray()
                                .Select(e => e.GetString() ?? string.Empty)
                                .ToArray();
                        }

                        ViewData["Errors"] = errors;
                    }
                }

                var todo = await client.GetFromJsonAsync<TodoItemDto>($"/api/todos/{id}");
                if (todo is null)
                {
                    return NotFound();
                }

                return Partial("_EditForm", todo);
            }

            var updated = await putResponse.Content.ReadFromJsonAsync<TodoItemDto>();
            if (updated is null)
            {
                return NotFound();
            }

            return Partial("_TodoCard", updated);
        }
    }
}
