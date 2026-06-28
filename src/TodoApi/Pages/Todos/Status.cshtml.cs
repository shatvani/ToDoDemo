using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApi.DTOs;

namespace TodoApi.Pages.Todos
{
    [IgnoreAntiforgeryToken]
    public class StatusModel(IHttpClientFactory httpClientFactory) : PageModel
    {
        [BindProperty]
        public string NewStatus { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            var client = httpClientFactory.CreateClient("TodoApi");

            var payload = new { Status = NewStatus };
            var patchResponse = await client.PatchAsJsonAsync($"/api/todos/{id}/status", payload);

            if (!patchResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)patchResponse.StatusCode);
            }

            var todo = await client.GetFromJsonAsync<TodoItemDto>($"/api/todos/{id}");
            if (todo is null)
            {
                return NotFound();
            }

            return Partial("_TodoCard", todo);
        }
    }
}
