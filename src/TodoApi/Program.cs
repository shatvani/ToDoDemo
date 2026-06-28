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

builder.Services.AddHttpClient("TodoApi", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["TodoApi:BaseUrl"]!);
});

builder.Services.AddWolverineHttp();
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    opts.UseEntityFrameworkCoreTransactions();
    opts.UseSystemTextJsonForSerialization(o =>
        o.Converters.Add(new JsonStringEnumConverter()));
});

// Razor Pages (HTMX views — EPIC-3)
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Health Checks (DB check added in EPIC-2 after DbContext is wired up)
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString!);

builder.Services.AddSingleton<UpdatedAtInterceptor>();

// A factory-s minta jobb választás a Wolverine handlerekhez: a kézzel kontrollált await using élettartam elkerüli a scope-ütközéseket, ha egy handler párhuzamosan vagy hosszabb műveletben fut.
builder.Services.AddDbContextFactory<TodoDbContext>((sp, opts) =>
{
    opts.UseSqlServer(connectionString);
    opts.AddInterceptors(sp.GetRequiredService<UpdatedAtInterceptor>());
});

builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStaticFiles();
app.MapRazorPages();
app.MapHealthChecks("/api/health");

app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});

await app.MigrateDbAsync();
await app.RunAsync();
