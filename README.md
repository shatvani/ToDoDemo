# TodoAiDemo

> **Demo projekt** — AI-támogatott fejlesztői workflow bemutatása ASP.NET Core 10 + HTMX + GitHub Actions környezetben.

![CI](https://github.com/{personal-github}/todo-ai-demo/actions/workflows/ci.yml/badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=todo-ai-demo&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=todo-ai-demo)

---

## Mi ez a projekt?

Ez egy Todo alkalmazás, de a lényeg nem a Todo funkcionalitás — hanem az **AI-támogatott fejlesztői pipeline** bemutatása:

| Fázis | AI eszköz | Mit csinál |
|---|---|---|
| Tervezés | Claude (on-demand) | Spec, architektúra, edge case-ek, docs |
| Kódírás | GitHub Copilot (IDE) | Feature implementáció, xUnit váz |
| CI/CD | Copilot Agent + GitHub Models API | Code review, PR description, smoke test, bug task |
| Projektmenedzsment | GitHub Models API + OpenProject | Task létrehozás, státusz frissítés |

---

## Tech stack

- **Backend:** ASP.NET Core 10 Minimal API + EF Core 10 + PostgreSQL 16
- **Architektúra:** Vertical Slice Architecture (VSA)
- **Frontend:** HTMX 2.0 + Tailwind CSS 4 (Razor partial-ok)
- **Tesztelés:** xUnit + Testcontainers
- **CI/CD:** GitHub Actions (self-hosted runner)
- **Kódminőség:** Roslyn + StyleCop + dotnet format + SonarCloud

---

## Lokális fejlesztői környezet

### Előfeltételek

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (PostgreSQL-hez)

### Indítás

```bash
# 1. Klónozás
git clone https://github.com/{personal-github}/todo-ai-demo.git
cd todo-ai-demo

# 2. PostgreSQL indítása Docker-rel
docker compose up -d db

# 3. API indítása
dotnet run --project src/TodoApi

# Az API elérhető: http://localhost:5000
# Scalar API doc:  http://localhost:5000/scalar
```

### Teljes stack Docker-rel

```bash
docker compose up -d
# Az alkalmazás elérhető: http://localhost:5000
```

### Tesztek futtatása

```bash
dotnet test
# A tesztek Testcontainers segítségével automatikusan indítanak egy PostgreSQL konténert.
# Előfeltétel: Docker legyen futó állapotban.
```

---

## Kódminőség

```bash
# Formázás ellenőrzése
dotnet format --verify-no-changes

# Build (Roslyn + StyleCop)
dotnet build
```

> `TreatWarningsAsErrors = true` — minden warning buildhiba.

---

## Projekt struktúra

```
todo-ai-demo/
├── src/TodoApi/
│   ├── Features/Todos/          # Vertical Slice-ok (CreateTodo, GetTodos, stb.)
│   ├── Infrastructure/
│   │   ├── Persistence/         # EF Core DbContext, Migrations
│   │   └── OpenProject/         # OpenProject REST API kliens
│   ├── Views/Todos/             # Razor partial-ok (HTMX válaszok)
│   └── wwwroot/                 # htmx.min.js, Tailwind CSS
├── tests/TodoApi.Tests/         # xUnit + Testcontainers
├── docker/                      # Dockerfile, nginx.conf
├── .github/
│   ├── workflows/               # ci.yml, cd-staging.yml, op-integration.yml
│   ├── scripts/                 # GitHub Models API szkriptek
│   └── copilot-instructions.md  # Copilot projekt instrukciók
├── docs/
│   ├── SPEC.md                  # Részletes specifikáció
│   ├── TASKS.md                 # Feladatlista (OpenProject WP-khez igazítva)
│   └── adr/                     # Architecture Decision Records
└── CLAUDE.md                    # Claude konzultáns instrukciók
```

---

## AI szerepkörök

### Claude (konzultáns)
On-demand, emberi kezdeményezésre. Sosem fut automatikusan a pipeline-ban. Instrukciók: [`CLAUDE.md`](CLAUDE.md)

### GitHub Copilot (kódírás)
IDE-ben, fejlesztés közben. Feature implementáció, xUnit váz, PR description. Instrukciók: [`.github/copilot-instructions.md`](.github/copilot-instructions.md)

### GitHub Models API (pipeline automatizáció)
- Failed build → AI log elemzés → OpenProject Bug WP
- Deploy → WP státusz frissítés
- PO igény → task lebontás és assignálás

---

## Dokumentáció

- [`docs/SPEC.md`](docs/SPEC.md) — teljes specifikáció (tech stack, pipeline, OP integráció)
- [`docs/TASKS.md`](docs/TASKS.md) — UC-k és task-ok lebontása (82 task, 6 EPIC)
- [`docs/adr/`](docs/adr/) — architektúra döntések (ADR-001, ADR-002, ADR-003)

---

## Licence

MIT
