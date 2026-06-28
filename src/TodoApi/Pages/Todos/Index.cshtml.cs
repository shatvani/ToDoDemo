using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApi.DTOs;

namespace TodoApi.Pages.Todos
{
    public class IndexModel(IHttpClientFactory httpClientFactory) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Priority { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Tag { get; set; }

        public List<TodoItemDto> Todos { get; set; } = [];

        public async Task OnGetAsync()
        {
            var client = httpClientFactory.CreateClient("TodoApi");

            var query = new List<string>();
            if (!string.IsNullOrEmpty(Status))
            {
                query.Add($"status={Uri.EscapeDataString(Status)}");
            }

            if (!string.IsNullOrEmpty(Priority))
            {
                query.Add($"priority={Uri.EscapeDataString(Priority)}");
            }

            if (!string.IsNullOrEmpty(Tag))
            {
                query.Add($"tag={Uri.EscapeDataString(Tag)}");
            }

            var url = query.Count > 0
                ? $"/api/todos?{string.Join("&", query)}"
                : "/api/todos";

            var result = await client.GetFromJsonAsync<List<TodoItemDto>>(url);
            Todos = result ?? [];
        }
    }
}
