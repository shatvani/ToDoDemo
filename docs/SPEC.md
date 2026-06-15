# AI-Assisted CI/CD Todo Demo — SPEC.md

> **Verzió:** 1.3  
> **Státusz:** Tervezési fázis  
> **Szerző:** Hatvani Sándor (FPH)  
> **Cél:** FPH belső bemutató + saját tanulási projekt

---

## 1. Projekt összefoglalás

Ez a projekt egy **Todo alkalmazás**, amely elsősorban nem a funkcionalitásáról szól, hanem a köré épített **AI-támogatott fejlesztői workflow** bemutatásáról. A három AI szereplő (Claude, GitHub Copilot, OpenProject AI integráció) együttműködve lefedi a fejlesztési életciklus minden fázisát: tervezéstől a deployig és a projektmenedzsmentig.

### Amit bemutat

| Fázis | AI eszköz | Mit csinál |
|---|---|---|
| Tervezés + konzultáció | **Claude** (on-demand, emberi kezdeményezésre) | Spec, API kontrakt, edge case-ek, interfész terv, hiányzó tesztesetek jelzése, docs váz |
| Kódírás | **GitHub Copilot** (IDE) | Feature branch kód, xUnit váz generálás, implementáció |
| CI/CD | **GitHub Actions + Copilot Agent** | Build, quality gate, AI code review, PR description, smoke test |
| Projektmenedzsment | **Copilot + OpenProject REST API** | Task létrehozás, assignálás, státusz frissítés |

> ⚠️ **Fontos:** Claude **nem fut automatikusan** — kizárólag emberi kezdeményezésre, chatben vesz részt. A fejlesztési ciklus bármely pontján igénybe vehető (task értelmezés, architektúra döntés, teszt review, docs), de mindig a fejlesztő hívja. Automatizált pipeline-ban **nem** fut Claude API hívás — minden AI automatizáció a GitHub Copilot / GitHub Models API-n keresztül történik (költségkontroll).

---

## 2. Infrastruktúra és környezet

### Szerverek

| Szerver | Szerepkör | IP |
|---|---|---|
| LINVDOCK1 | Production (OpenProject fut itt) | 172.22.0.131 |
| LINVDOCK1T | Test / Staging + GitHub Actions self-hosted runner | — |

### GitHub Actions runner

- **Típus:** Self-hosted runner, LINVDOCK1T-n Docker-ben futtatva
- **Runner label:** `self-hosted`, `linux`, `docker`
- A runner Docker-in-Docker (DinD) képességgel rendelkezik, hogy a pipeline Docker image-eket tudjon buildelni

### Container registry

- **GitHub Container Registry (GHCR):** `ghcr.io/fphgov/todo-ai-demo`

### OpenProject

- Már fut LINVDOCK1-en
- REST API v3 elérhető belső hálózaton
- API token: GitHub Actions Secrets-ben tárolva (`OP_API_TOKEN`)

---

## 3. Todo alkalmazás — Funkcionális követelmények

### 3.1 Entitások

#### `TodoItem`
| Mező | Típus | Leírás |
|---|---|---|
| `Id` | `Guid` | Elsődleges kulcs |
| `Title` | `string` (max 200) | Kötelező, a feladat rövid neve |
| `Description` | `string?` | Opcionális részletes leírás |
| `Status` | `enum` | `Open`, `InProgress`, `Done`, `Cancelled` |
| `Priority` | `enum` | `Low`, `Medium`, `High` |
| `CreatedAt` | `DateTime` | UTC, automatikus |
| `UpdatedAt` | `DateTime` | UTC, automatikus |
| `DueDate` | `DateTime?` | Opcionális határidő |
| `Tags` | `string[]` | Opcionális tag lista |

### 3.2 API végpontok (REST)

```
GET    /api/todos              → lista (szűrés: status, priority, tag)
GET    /api/todos/{id}         → egy elem
POST   /api/todos              → létrehozás
PUT    /api/todos/{id}         → teljes frissítés
PATCH  /api/todos/{id}/status  → csak státusz frissítés
DELETE /api/todos/{id}         → törlés
GET    /api/health             → health check (CI/CD-hez)
```

### 3.3 Frontend

- **HTMX 2.0** — szerver-side rendering, hypermedia responses
- Az API végpontok HTML partial-okat adnak vissza HTMX kérésekre
- Alap stílus: **Tailwind CSS 4** (nem stílustalan, de nem cél az összetett SPA UI)
- UI design: Claude segítségével tervezett, egyedi arculat (nem generikus sablon)
- Funkciók: lista nézet, create/edit form, státusz toggle, szűrés tag/prioritás szerint
- **Nincs Node.js build step** — a `htmx.min.js` statikus fájl a `wwwroot`-ban, nem kell `npm install` / `npm run build` a pipeline-ban

---

## 4. Tech Stack

### Backend
| Réteg | Technológia |
|---|---|
| Framework | ASP.NET Core 10 Minimal API |
| ORM | Entity Framework Core 10 |
| Adatbázis | PostgreSQL 16 |
| Architektúra | Vertical Slice Architecture (VSA) |
| Validáció | FluentValidation |
| Tesztelés | xUnit + Testcontainers (PostgreSQL) — Copilot generálja a vázat IDE-ben, fejlesztő írja az edge case-eket |
| Logging | Microsoft.Extensions.Logging + Application Insights sink |
| Kódminőség | Roslyn Analyzers, StyleCop.Analyzers, dotnet format |
| API doc | Swagger / Scalar |

### Frontend
| Réteg | Technológia |
|---|---|
| Megközelítés | HTMX 2.0 (hypermedia, szerver-side rendering) |
| Styling | Tailwind CSS 4 |
| Scriptek | Alpine.js (opcionális, kisebb interaktivitáshoz) |

### Infrastruktúra
| Elem | Technológia |
|---|---|
| Containerizáció | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Runner | Self-hosted (LINVDOCK1T) |
| Registry | GHCR |
| Reverse proxy | Nginx (staging-en) |
| Kódminőség (cloud) | SonarCloud (ingyenes, publikus repo) |

---

## 5. Repository struktúra

### Solution felépítése

Ez **egyetlen Visual Studio solution** (`TodoAiDemo.sln`), egyetlen deployolható egység:

- `TodoApi` — ASP.NET Core 10 Minimal API (backend + HTMX frontend együtt)
- `TodoApi.Tests` — xUnit tesztprojekt

Nincs külön frontend projekt, nincs külön microservice.

### Branch stratégia

```
feature/OP-{id}-{leiras}  ──┐
bugfix/OP-{id}-{leiras}   ──┤→  develop  →  main (= Release branch)
chore/OP-{id}-{leiras}    ──┘                    ↓
                                            tag: v1.0.0  (automatikus, pipeline)
                                                 ↓
                                            CD workflow triggerelődik
```

- `main` = Release branch: csak PR-on keresztül, `develop`-ból
- Minden `main`-re merge után a pipeline **automatikusan létrehozza a Git tag-et** (szemantikus verzió)
- A tag triggeli a CD (staging deploy) workflow-t

### Fájlstruktúra

```
todo-ai-demo/
├── .github/
│   ├── workflows/
│   │   ├── ci.yml              # Build + Test + SonarCloud
│   │   ├── cd-staging.yml      # Deploy staging (LINVDOCK1T) + tagging
│   │   └── op-integration.yml  # OpenProject reusable workflow
│   └── CODEOWNERS
├── src/
│   ├── TodoApi/                # ASP.NET Core 10 Minimal API
│   │   ├── Features/
│   │   │   └── Todos/
│   │   │       ├── CreateTodo/
│   │   │       ├── GetTodos/
│   │   │       ├── UpdateTodo/
│   │   │       └── DeleteTodo/
│   │   ├── Infrastructure/
│   │   │   ├── Persistence/    # EF Core DbContext, Migrations
│   │   │   └── OpenProject/    # OP REST API kliens
│   │   ├── Views/              # Razor partial-ok (HTMX válaszok)
│   │   │   └── Todos/
│   │   ├── wwwroot/            # Statikus fájlok (htmx.min.js, tailwind output css)
│   │   └── Program.cs
├── tests/
│   └── TodoApi.Tests/          # xUnit + Testcontainers
├── docker/
│   ├── api.Dockerfile
│   └── nginx.conf
├── docker-compose.yml          # Lokális fejlesztés
├── docker-compose.staging.yml  # Staging deploy
├── TodoAiDemo.sln

├── CLAUDE.md                   # Claude konzultáns instrukciók (planning-first) + ADR index
├── docs/
│   ├── SPEC.md                 # Ez a fájl
│   ├── TASKS.md                # UC-k és feladatok lebontása (OP WP-khez igazítva)
│   └── adr/                    # Architecture Decision Records
│       ├── ADR-001-htmx-vs-react.md
│       ├── ADR-002-sonarcloud-vs-sonarqube.md
│       └── ADR-003-copilot-vs-claude-pipeline.md
└── README.md

---

## 6. GitHub Actions Pipeline

### 6.1 CI workflow (`ci.yml`)

**Trigger:** minden `push` és `pull_request` a `main` és `develop` branch-re

```
Lépések:
1.  Checkout
2.  .NET 10 SDK setup
3.  dotnet restore
4.  dotnet format --verify-no-changes
    ↑ FORMÁZÁS: szintaktikai whitespace, indentáció — gyors, helyi ellenőrzés
5.  dotnet build (--no-restore)
    ↑ ROSLYN + STYLECOP: naming rules, null safety, kódstílus — fordítási időben
    → Hiba esetén: error-ként buktatja el a buildet (TreatWarningsAsErrors=true)
6.  dotnet test (Testcontainers PostgreSQL-lel)
    → JUnit XML report + coverage report (Coverlet)
7.  SonarCloud scan
    ↑ SONARCLOUD: code smells, duplications, security hotspots, coverage trend
    → Roslyn/dotnet format NEM helyettesíti: azok szintaxis szintűek,
       SonarCloud szemantikus szintű elemzést végez (logikai hibák, komplexitás)
    → Quality Gate: ha nem teljesül, a pipeline elbukik
8.  Copilot Agent: AI code review
    → Automatikusan kommentál a PR-on (kód problémák, javaslatok)
    → Nem blokkolja a pipeline-t, csak kommentál
9.  Docker image build (api)
10. Docker image push → GHCR (csak main branch-en)
11. Copilot Agent: AI PR description generálás
    → Ha a Quality Gate átment, Copilot agent generálja a PR leírást
12. → OpenProject: build eredmény jelentés (siker vagy kudarc)
```

**Failed build esetén (AI integráció — Copilot / GitHub Models API):**
```
Build log → GitHub Models API (Copilot) elemzés
         → OpenProject: új Work Package létrehozás
           Típus: Bug
           Cím: "[CI FAIL] {branch} — {hiba összefoglalója}"
           Leírás: AI-generált root cause elemzés
           Assignee: az utolsó committer
           Priority: High
```

> ℹ️ Claude API itt **nem** kerül meghívásra. Az automatizált log elemzés a GitHub Models API-n fut (Copilot előfizetés keretein belül, extra költség nélkül).

### 6.2 CD workflow (`cd-staging.yml`)

**Trigger:** merge `main`-re (CI sikeres után)

```
Lépések:
1. Szemantikus verzió meghatározása (utolsó tag + increment)
2. Git tag létrehozása automatikusan (pl. v1.0.1)
3. SSH a LINVDOCK1T-re
4. docker compose pull (friss image-ek)
5. docker compose up -d
6. Health check: GET /api/health (max 30s, retry 5x)
7. AI smoke test (Copilot Agent)
   → Kritikus végpontok automatikus tesztelése (GET /api/todos, POST /api/todos, stb.)
   → Agent összefoglalót generál: mely végpontok válaszoltak, státuszkódok, válaszidők
   → Eredmény kommentként kerül a merge commithoz
8. → OpenProject: deploy eredmény
   - Sikeres: érintett Work Package-ek → "In Progress → Deployed"
   - Sikertelen: új Bug Work Package (Copilot / GitHub Models API elemzéssel)
```

### 6.3 OpenProject integráció workflow (`op-integration.yml`)

**Trigger:** `workflow_call` — más workflow-k hívják

#### Branch névkonvenció

A pipeline az OP Work Package ID-t a branch névből nyeri ki regex-szel. Kötelező formátum:

```
feature/OP-{id}-{rovid-leiras}
bugfix/OP-{id}-{rovid-leiras}
hotfix/OP-{id}-{rovid-leiras}
chore/OP-{id}-{rovid-leiras}
```

Példák:
```
feature/OP-42-todo-create-endpoint
bugfix/OP-57-null-reference-todocontroller
chore/OP-12-docker-compose-setup
```

> ⚠️ Ha a branch neve nem tartalmaz `OP-{id}` mintát, a pipeline **nem** frissít WP-t, de nem bukik el — csak loggol egy figyelmeztetést.

#### Workflow inputok

```yaml
inputs:
  event_type:       # build_failed | build_passed | deployed | deploy_failed
  branch:           # branch neve (OP-{id} kinyeréshez)
  commit_sha:       # commit hash
  log_excerpt:      # hibalog (opcionális, failed esetén)
  work_package_ids: # érintett WP-k (opcionális, explicit override)
```

---

## 7. OpenProject Integráció — Részletes terv

### 7.1 OpenProject projekt struktúra

```
[OP Projekt: todo-ai-demo]
├── Work Packages
│   ├── Type: Feature   (user story-k)
│   ├── Type: Task      (technikai feladatok)
│   ├── Type: Bug       (CI/CD által auto-létrehozott)
│   └── Type: Release   (deploy mérföldkövek)
├── Versions (Milestones)
│   ├── v0.1 — MVP
│   └── v0.2 — AI integráció
└── Members
    ├── Developer (Sanyi)
    └── Product Owner
```

### 7.2 Státuszgép (State Machine)

```
New → In Progress → In Review → Deployed → Closed
 ↑                                            |
 └──────── (Reopened by AI if failed) ────────┘

Bug workflow:
New (AI created) → In Progress → Fixed → Closed
```

### 7.3 API műveletek

#### Build sikertelen → Bug létrehozás

```http
POST /api/v3/projects/todo-ai-demo/work_packages
Authorization: Bearer {OP_API_TOKEN}
Content-Type: application/json

{
  "_type": "WorkPackage",
  "subject": "[CI FAIL] feature/my-branch — NullReferenceException in TodoController",
  "description": {
    "raw": "**AI-generált elemzés (Copilot):**\n\n...(GitHub Models API válasz)..."
  },
  "type": { "href": "/api/v3/types/bug_id" },
  "priority": { "href": "/api/v3/priorities/high_id" },
  "assignee": { "href": "/api/v3/users/{last_committer_id}" },
  "version": { "href": "/api/v3/versions/{current_version_id}" }
}
```

#### Deploy sikeres → WP státusz frissítés

```http
PATCH /api/v3/work_packages/{id}
{
  "status": { "href": "/api/v3/statuses/deployed_id" },
  "comment": {
    "raw": "✅ Deployed to staging — Build #{run_id} — {timestamp}"
  }
}
```

#### AI-alapú feladatkiosztás (PO flow)

```
PO beír természetes nyelvű igényt az OP-be
        ↓
OP webhook → GitHub Actions
        ↓
GitHub Models API (Copilot): elemzi, felbontja task-okra, javasol assignee-t
        ↓
OP REST API: sub-tasks létrehozása, assignálás
        ↓
PO értesítés (OP notification)
```

### 7.4 Secrets (GitHub Actions)

| Secret neve | Tartalma |
|---|---|
| `OP_API_TOKEN` | OpenProject API token |
| `OP_BASE_URL` | `http://172.22.0.131/openproject` (belső URL) |
| `OP_PROJECT_ID` | todo-ai-demo projekt ID |
| `GITHUB_TOKEN` | GitHub Models API hozzáférés (Copilot, automatikus) |
| `GHCR_TOKEN` | GitHub Container Registry token |
| `STAGING_SSH_KEY` | SSH kulcs a LINVDOCK1T-re |
| `STAGING_HOST` | LINVDOCK1T IP/hostname |
| `SONAR_TOKEN` | SonarCloud projekt token |

---

## 8. Copilot / GitHub Models API integráció a pipeline-ban

A pipeline-ban minden automatizált AI hívás a **GitHub Models API**-n keresztül történik, amely a Copilot előfizetés részeként elérhető — külön API kulcs és extra költség nélkül.

### 8.1 Log elemzés (failed build esetén)

```javascript
// .github/scripts/analyze-build-failure.js
const response = await fetch("https://models.inference.ai.azure.com/chat/completions", {
  method: "POST",
  headers: {
    "Authorization": `Bearer ${process.env.GITHUB_TOKEN}`,
    "Content-Type": "application/json"
  },
  body: JSON.stringify({
    model: "gpt-4o",   // GitHub Models-on elérhető modell
    messages: [
      {
        role: "system",
        content: "CI/CD build failure analyzer vagy. Elemezd a build logot és adj: 1) Root cause 1-2 mondatban, 2) Érintett fájl/osztály, 3) Javasolt javítás. Válaszolj magyarul. Legyél tömör."
      },
      {
        role: "user",
        content: `Build log:\n\n${process.env.BUILD_LOG_EXCERPT}`
      }
    ]
  })
});
```

### 8.2 PO feladatkiosztási asszisztens

```javascript
// Természetes nyelvű igény → strukturált WP javaslat
{
  role: "system",
  content: `Projektmenedzsment asszisztens vagy.
Fejlesztési igényből hozz létre strukturált OpenProject Work Package javaslatokat.
Válaszolj KIZÁRÓLAG JSON-ban:
{
  "packages": [
    {
      "subject": "string",
      "type": "Feature|Task|Bug",
      "priority": "Low|Medium|High",
      "estimatedHours": number,
      "suggestedAssignee": "developer|po|devops",
      "description": "string"
    }
  ]
}`
}
```

---

## 9. Docker Compose

### Lokális fejlesztés (`docker-compose.yml`)

```yaml
services:
  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: tododb
      POSTGRES_USER: todo
      POSTGRES_PASSWORD: todo_dev_pw
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  api:
    build:
      context: .
      dockerfile: docker/api.Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Host=db;Database=tododb;Username=todo;Password=todo_dev_pw"
      ASPNETCORE_ENVIRONMENT: Development
    ports:
      - "5000:8080"
    depends_on:
      - db

volumes:
  pgdata:
```

> ℹ️ Külön `web` service nem szükséges — a HTMX frontend az API-val együtt kerül kiszolgálásra (`wwwroot` + Razor partial-ok).

---

## 10. Megvalósítási fázisok

### Fázis 1 — Alap Todo API (MVP)
- [ ] Repository létrehozás (fphgov GitHub org, publikus)
- [ ] `TodoAiDemo.sln` + ASP.NET Core 10 projekt setup (VSA, EF Core, PostgreSQL)
- [ ] CRUD végpontok implementálása
- [ ] HTMX 2.0 + Tailwind CSS frontend (Claude-tervezett UI)
- [ ] xUnit + Testcontainers tesztek (Copilot váz + fejlesztő edge case-ek)
- [ ] Docker Compose (lokális)
- [ ] `CLAUDE.md` megírása (planning-first mód + ADR index)
- [ ] `docs/adr/` mappa + első 3 ADR megírása (főbb architektúra döntések)
- [ ] `TASKS.md` megírása (UC-k lebontása, OP WP-khez igazítva)

### Fázis 2 — CI Pipeline
- [ ] Self-hosted runner setup (LINVDOCK1T — Docker CE telepítés + runner regisztráció)
- [ ] SonarCloud projekt létrehozása, `SONAR_TOKEN` beállítása
- [ ] `ci.yml` workflow (build + Roslyn/StyleCop + dotnet format + test + SonarCloud + Copilot Agent review + Docker build + GHCR push)
- [ ] Copilot Agent: AI code review + PR description automatizálás beállítása
- [ ] CI badge + SonarCloud badge a README-be

### Fázis 3 — CD Pipeline + Tagging
- [ ] `cd-staging.yml` workflow (automatikus Git tag + SSH deploy LINVDOCK1T-re)
- [ ] Health check lépés
- [ ] AI smoke test (Copilot Agent összefoglaló deploy után)
- [ ] Staging Nginx konfiguráció

### Fázis 4 — OpenProject alap integráció
- [ ] OP projekt létrehozása (struktúra, státuszok, típusok)
- [ ] API token + Secrets beállítása
- [ ] `op-integration.yml` reusable workflow
- [ ] Build státusz → OP WP komment

### Fázis 5 — AI-alapú bug task létrehozás
- [ ] `analyze-build-failure.js` script (GitHub Models API / Copilot)
- [ ] GitHub Models API hívás integrálása a CI workflow-ba
- [ ] Sikertelen build → AI-generált Bug WP automatikusan OP-ban

### Fázis 6 — Teljes AI integráció
- [ ] Deploy sikeres → WP státusz frissítés
- [ ] PO feladatkiosztási asszisztens (GitHub Models API)
- [ ] OP webhook → GitHub Actions → GitHub Models API → OP visszaírás

---

## 11. Nem scope (tudatosan kihagyva)

- ❌ Authentikáció / Azure Entra ID (demo projekt, nincs szükség)
- ❌ Éles production deploy (csak staging)
- ❌ SonarQube self-hosted (helyette SonarCloud ingyenes tier)
- ❌ Monitoring / Application Insights részletes setup (külön projekt)
- ❌ Mikroszolgáltatás architektúra
- ❌ Claude API automatikus hívás a pipeline-ban (költségkontroll)

---

## 12. Nyitott kérdések

| # | Kérdés | Döntés |
|---|---|---|
| 1 | LINVDOCK1T-n Docker CE telepítve van-e már? | — |
| 2 | Az OP belső URL elérhető-e LINVDOCK1T-ről? (hálózati szegmentáció) | — |
| 3 | Application Insights: FPH-s Azure subscription-ben legyen az AI resource? | — |
| 4 | SonarCloud: melyik GitHub org alatt legyen a projekt? (fphgov vagy személyes) | — |
| 5 | Szemantikus verzióozás: manuális (`v1.0.0` → `v1.0.1`) vagy conventional commits alapján automatikus? | — |

---

*Ez a SPEC.md élő dokumentum. Minden tervezési döntés ide kerül vissza.*
