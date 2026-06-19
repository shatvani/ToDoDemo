using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI / Scalar
builder.Services.AddOpenApi();

builder.Services.AddWolverineHttp();
builder.Host.UseWolverine(opts =>
{
    opts.Discovery
        .IncludeAssembly(typeof(Program).Assembly);
});

// Razor Pages (HTMX views — EPIC-3)
builder.Services.AddRazorPages();

// Health Checks (DB check added in EPIC-2 after DbContext is wired up)
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});

app.Run();
