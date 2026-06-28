using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TodoApi.Pages.Todos
{
    [IgnoreAntiforgeryToken]
    public class DeleteModel(IHttpClientFactory httpClientFactory) : PageModel
    {
        public async Task<IActionResult> OnDeleteAsync(Guid id)
        {
            var client = httpClientFactory.CreateClient("TodoApi");
            await client.DeleteAsync($"/api/todos/{id}");
            return Content(string.Empty, "text/html");
        }
    }
}
