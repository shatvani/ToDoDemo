using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Infrastructure.SaveChangesInterceptor;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI / Scalar
builder.Services.AddOpenApi();

builder.Services.AddWolverineHttp();
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    opts.UseEntityFrameworkCoreTransactions();
});

// Razor Pages (HTMX views — EPIC-3)
builder.Services.AddRazorPages();

// Health Checks (DB check added in EPIC-2 after DbContext is wired up)
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSingleton<UpdatedAtInterceptor>();

// A factory-s minta jobb választás a Wolverine handlerekhez: a kézzel kontrollált await using élettartam elkerüli a scope-ütközéseket, ha egy handler párhuzamosan vagy hosszabb műveletben fut.
builder.Services.AddDbContextFactory<TodoDbContext>((sp, opts) =>
{
    opts.UseSqlServer(connectionString);
    opts.AddInterceptors(sp.GetRequiredService<UpdatedAtInterceptor>());
});

builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// TODO EPIC-2: FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
