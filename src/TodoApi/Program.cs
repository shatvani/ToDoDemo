var builder = WebApplication.CreateBuilder(args);

// OpenAPI / Scalar
builder.Services.AddOpenApi();

// Razor Pages (HTMX views — EPIC-3)
builder.Services.AddRazorPages();

// Health Checks (DB check added in EPIC-2 after DbContext is wired up)
builder.Services.AddHealthChecks();

// TODO EPIC-2: EF Core + PostgreSQL
/* builder.Services.AddDbContext<TodoDbContext>(...); */

// TODO EPIC-2: FluentValidation
/* builder.Services.AddFluentValidationAutoValidation(); */

// TODO EPIC-2: Application Insights
/* builder.Services.AddApplicationInsightsTelemetry(); */

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStaticFiles();
app.MapRazorPages();
app.MapHealthChecks("/api/health");

// TODO EPIC-2: Feature slice endpoints
/* app.MapTodoEndpoints(); */

app.Run();
