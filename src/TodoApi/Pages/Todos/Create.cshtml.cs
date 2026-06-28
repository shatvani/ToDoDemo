using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TodoApi.DTOs;

namespace TodoApi.Pages.Todos
{
    [IgnoreAntiforgeryToken]
    public class CreateModel(
        IHttpClientFactory httpClientFactory,
        ICompositeViewEngine viewEngine,
        ITempDataProvider tempDataProvider) : PageModel
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

        public IActionResult OnGet()
        {
            return Partial("_TodoForm", null);
        }

        public async Task<IActionResult> OnPostAsync()
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

            var postResponse = await client.PostAsJsonAsync("/api/todos", payload);

            if (!postResponse.IsSuccessStatusCode)
            {
                if ((int)postResponse.StatusCode == 400)
                {
                    var stream = await postResponse.Content.ReadAsStreamAsync();
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

                return Partial("_TodoForm", null);
            }

            var todos = await client.GetFromJsonAsync<List<TodoItemDto>>("/api/todos") ?? new List<TodoItemDto>();
            var listHtml = await RenderPartialToStringAsync("_TodoList", todos);

            var html = $"""<div id="todo-list" hx-swap-oob="true">{listHtml}</div>""";
            return Content(html, "text/html");
        }

        private async Task<string> RenderPartialToStringAsync(string viewName, object model)
        {
            var actionContext = new ActionContext(
                HttpContext,
                RouteData,
                PageContext.ActionDescriptor);

            var viewResult = viewEngine.FindView(actionContext, viewName, isMainPage: false);
            if (!viewResult.Success)
            {
                throw new ArgumentException($"View '{viewName}' not found.");
            }

            using var writer = new StringWriter();
            var viewData = new ViewDataDictionary<object>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: ModelState)
            {
                Model = model,
            };
            var tempData = new TempDataDictionary(HttpContext, tempDataProvider);
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);
            return writer.ToString();
        }
    }
}
