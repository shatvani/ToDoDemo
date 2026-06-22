using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApi.DTOs;

namespace TodoApi.Pages.Todos
{
    [IgnoreAntiforgeryToken]
    public class TodoCardModel(IHttpClientFactory httpClientFactory) : PageModel
    {
        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var client = httpClientFactory.CreateClient("TodoApi");
            var todo = await client.GetFromJsonAsync<TodoItemDto>($"/api/todos/{id}");

            if (todo is null)
            {
                return NotFound();
            }

            return Partial("_TodoCard", todo);
        }
    }
}
