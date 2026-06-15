# Copilot Instructions — TodoAiDemo

## Projekt kontextus

Ez egy **Todo alkalmazás demo projekt**, amelynek elsődleges célja nem a Todo funkcionalitás, hanem az **AI-támogatott fejlesztői workflow** bemutatása. A három AI szereplő (Claude, GitHub Copilot, OpenProject AI integráció) együttműködve lefedi a fejlesztési életciklus minden fázisát.

A projekt repo: `{personal-github}/todo-ai-demo` (publikus)

---

## Architektúra

| Réteg | Technológia |
|---|---|
| Backend | ASP.NET Core 10 Minimal API |
| ORM | Entity Framework Core 10 + PostgreSQL 16 |
| Architektúra | **Vertical Slice Architecture (VSA)** |
| Validáció | FluentValidation 11 |
| Frontend | HTMX 2.0 + Tailwind CSS 4 (Razor partial-ok) |
| Tesztelés | xUnit + Testcontainers (PostgreSQL) |
| Logging | Microsoft.Extensions.Logging + Application Insights |
| API doc | Scalar / OpenAPI |

---

## Feature slice struktúra (VSA) — KÖTELEZŐ MINTA

Minden új feature **önálló mappába** kerül. Nincs külön Controller, Service, Repository réteg.

```
src/TodoApi/Features/Todos/{FeatureName}/
├── {FeatureName}Command.cs       # request DTO (POST/PUT/PATCH/DELETE esetén)
├── {FeatureName}Query.cs         # request DTO (GET esetén)
├── {FeatureName}Response.cs      # response DTO
├── {FeatureName}Validator.cs     # FluentValidation validator
├── {FeatureName}Handler.cs       # az üzleti logika (DbContext injektálva)
└── {FeatureName}Endpoint.cs      # Minimal API endpoint regisztráció
```

**Példa — CreateTodo:**
```csharp
// CreateTodoCommand.cs
public record CreateTodoCommand(string Title, string? Description, Priority Priority, DateTime? DueDate);

// CreateTodoValidator.cs
public class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

// CreateTodoHandler.cs
public static class CreateTodoHandler
{
    public static async Task<IResult> HandleAsync(
        CreateTodoCommand command,
        TodoDbContext db,
        IValidator<CreateTodoCommand> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var todo = new TodoItem { Title = command.Title, /* ... */ };
        db.Todos.Add(todo);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/todos/{todo.Id}", todo);
    }
}

// CreateTodoEndpoint.cs
public static class CreateTodoEndpoint
{
    public static IEndpointRouteBuilder MapCreateTodo(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/todos", CreateTodoHandler.HandleAsync)
           .WithName("CreateTodo")
           .WithOpenApi();
        return app;
    }
}
```

Az összes endpoint regisztrációja `TodoEndpointExtensions.cs`-ben:
```csharp
public static class TodoEndpointExtensions
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateTodo();
        app.MapGetTodos();
        // ...
        return app;
    }
}
```

---

## Entitás — TodoItem

```csharp
public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;        // max 200, kötelező
    public string? Description { get; set; }
    public TodoStatus Status { get; set; } = TodoStatus.Open;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime CreatedAt { get; set; }                  // UTC, automatikus
    public DateTime UpdatedAt { get; set; }                  // UTC, automatikus
    public DateTime? DueDate { get; set; }
    public string[] Tags { get; set; } = [];
}

public enum TodoStatus { Open, InProgress, Done, Cancelled }
public enum Priority { Low, Medium, High }
```

---

## API végpontok (SPEC 3.2)

```
GET    /api/todos              → lista (szűrés: status, priority, tag)
GET    /api/todos/{id}         → egy elem
POST   /api/todos              → létrehozás → 201 Created + Location header
PUT    /api/todos/{id}         → teljes frissítés → 200 OK
PATCH  /api/todos/{id}/status  → csak státusz → 200 OK / 422 érvénytelen átmenet
DELETE /api/todos/{id}         → 204 No Content
GET    /api/health             → health check
```

**Státuszgép** — érvényes átmenetek:
- `Open → InProgress, Cancelled`
- `InProgress → Done, Cancelled`
- `Done` és `Cancelled` → nem nyitható újra

---

## Kódolási szabályok

- `TreatWarningsAsErrors = true` — minden Roslyn / StyleCop warning buildhiba
- `Nullable = enable` — minden nullability explicit
- Privát field: `_camelCase` prefix (pl. `_dbContext`)
- Public API: `PascalCase`
- `var` preferált, ha a típus a jobb oldalból egyértelmű
- Kapcsos zárójelek **mindig** kötelezők (`csharp_prefer_braces = true`)
- Async metódusok neve `...Async` végződéssel
- **Nem használunk**: Repository pattern, MediatR, AutoMapper — a slice-ok közvetlen DbContext hozzáféréssel dolgoznak

---

## HTMX / Razor partial szabályok (EPIC-3)

- Az API végpontok HTMX kérésre **HTML partial-t** adnak vissza, JSON helyett
- A partial nézetek helye: `src/TodoApi/Views/Todos/`
- Nincs Node.js build step — `htmx.min.js` és Tailwind CSS statikus fájlok a `wwwroot`-ban
- Alpine.js csak minimális interaktivitáshoz, ha szükséges

---

## Tesztelési elvárások

- Minden endpoint-hoz **xUnit integrációs teszt** (`TodoApi.Tests` projektben)
- **Testcontainers PostgreSQL** — nem mock, mindig valódi adatbázis
- `Microsoft.AspNetCore.Mvc.Testing` `WebApplicationFactory` a test host-hoz
- Teszt struktúra: Arrange / Act / Assert szekciók kommenttel
- Edge case-ek kötelezők: nem létező ID (404), érvénytelen input (400/422), üres lista (200 + [])
- **FluentAssertions** az assertionökhöz

---

## Amit NE csinálj

- Ne generálj **Controller osztályt** — Minimal API endpoint-okat használunk
- Ne adj hozzá **MediatR-t** — felesleges közvetítő réteg
- Ne adj hozzá **AutoMapper-t** — kézi mapping, átláthatóság kedvéért
- Ne generálj **mock-ot adatbázishoz** (InMemory EF Core sem) — Testcontainers
- Ne hozz létre **külön Service réteget** — a Handler tartalmazza az üzleti logikát
- Ne generálj **Node.js / npm** konfigurációt — nincs frontend build step
- Ne módosítsd a `Directory.Build.props`-t

---

## AI szerepkörök ebben a projektben

| Szereplő | Feladat |
|---|---|
| **Claude** | Tervezés, konzultáció, edge case-ek, architektúra döntések, dokumentáció — csak emberi kezdeményezésre |
| **Copilot (te)** | Kód generálás IDE-ben, xUnit váz generálás, PR description |
| **GitHub Models API** | Pipeline automatizáció: log elemzés, bug task, PO asszisztens |

> ⚠️ Claude **nem fut automatikusan** a pipeline-ban. Minden automatizált AI hívás GitHub Models API-n keresztül történik.

---

## Branch névkonvenció

```
feature/OP-{id}-{rovid-leiras}
bugfix/OP-{id}-{rovid-leiras}
chore/OP-{id}-{rovid-leiras}
```

Példák: `feature/OP-20-get-todos`, `bugfix/OP-57-null-ref-handler`

---

*Ez a fájl a SPEC.md-del szinkronban tartandó. Architektúra döntés változásakor mindkettőt frissíteni kell.*
