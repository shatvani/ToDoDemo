# AI-Assisted CI/CD Todo Demo — TASKS.md

> **Verzió:** 1.0  
> **Kapcsolódó:** SPEC.md v1.3  
> **Státusz:** Tervezési fázis  
> **Szerző:** Hatvani Sándor (FPH)

---

## Struktúra magyarázat

```
EPIC   → nagy fejlesztési terület (SPEC.md fázisokhoz igazítva)
  UC   → Use Case, egy felhasználói vagy rendszerszempontú cél
    T  → Task, konkrét implementációs feladat (= OpenProject Work Package)
```

**Branch névkonvenció:** `feature/OP-{T-szám}-{rovid-leiras}`  
**OP Work Package típusok:** Feature (UC), Task (T), Bug (auto-generated)

---

## EPIC-1 — Projekt alapok és infrastruktúra

> Cél: a fejlesztői környezet, repo struktúra és dokumentáció alapjainak lerakása.  
> SPEC.md fázis: **Fázis 1**

---

### UC-1.1 — Repository és solution létrehozása

**Leírás:** A GitHub repository és a Visual Studio solution felállítása, az alapvető projekt struktúrával.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-01 | OP-01 | GitHub repo létrehozás (`fphgov/todo-ai-demo`, publikus) | `chore/OP-01-repo-init` |
| T-02 | OP-02 | `TodoAiDemo.sln` + `TodoApi` projekt létrehozás (ASP.NET Core 10 Minimal API) | `chore/OP-02-solution-setup` |
| T-03 | OP-03 | `TodoApi.Tests` xUnit projekt létrehozás + solution-höz adás | `chore/OP-03-test-project-setup` |
| T-04 | OP-04 | NuGet csomagok: EF Core 10, FluentValidation, Testcontainers, Coverlet, StyleCop, Roslyn | `chore/OP-04-nuget-packages` |
| T-05 | OP-05 | `TreatWarningsAsErrors=true` + `.editorconfig` + `.gitignore` beállítás | `chore/OP-05-code-quality-config` |

**Elfogadási kritérium:** `dotnet build` hibamentesen fut, `dotnet test` lefut (üres tesztekkel).

**Magyarázat:**
- `Directory.Build.props`: Egy MSBuild fájl, amit a .NET build rendszer **automatikusan** megtalál és alkalmaz minden projektfájlra a könyvtárfában. Tehát amit ide írsz, az érvényes mind a TodoApi.csproj-ra, mind a TodoApi.Tests.csproj-ra — nem kell minden projektbe külön beírni.
- A `TreatWarningsAsErrors=true` ezt szigorítja: minden warning automatikusan error lesz, vagyis ha warning van, a build megáll.
- `.editorconfig` — editor szintű formázási szabályok: behúzás mérete, sortörés stílusa, `var` használata, névkonvenciók (pl. private field `_camelCase`). Az IDE és a `dotnet format` ezt olvassa.
- `stylecop.json` — a *StyleCop Analyzers Roslyn* plugin konfigurációja. A StyleCop **fordítási időben** ellenőriz C#-specifikus kódstílus szabályokat: using direktívák sorrendje, fájl fejléc, dokumentáció szabályok, zárójelek elhelyezése. Az SA-val kezdődő rule ID-k mind StyleCop szabályok.

A kettő kiegészíti egymást: `.editorconfig` = szintaktikai formázás, `stylecop.json` = C# kódolási konvenciók.
---

### UC-1.2 — Dokumentáció alapok

**Leírás:** A projekt dokumentációs struktúrájának (CLAUDE.md, ADR-ek, README) elkészítése.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-06 | OP-06 | `CLAUDE.md` megírása (planning-first mód, Claude használati instrukciók, ADR index) | `chore/OP-06-claude-md` |
| T-07 | OP-07 | `docs/adr/ADR-001-htmx-vs-react.md` — döntés dokumentálása | `chore/OP-07-adr-001` |
| T-08 | OP-08 | `docs/adr/ADR-002-sonarcloud-vs-sonarqube.md` — döntés dokumentálása | `chore/OP-08-adr-002` |
| T-09 | OP-09 | `docs/adr/ADR-003-copilot-vs-claude-pipeline.md` — döntés dokumentálása | `chore/OP-09-adr-003` |
| T-10 | OP-10 | `README.md` alap struktúra (projekt leírás, setup instrukciók, badge helyek) | `chore/OP-10-readme` |

**Elfogadási kritérium:** Minden ADR tartalmazza: kontextus, döntés, következmények. CLAUDE.md-ben planning-first workflow le van írva.

---

### UC-1.3 — Lokális Docker Compose környezet

**Leírás:** A helyi fejlesztéshez szükséges Docker Compose stack felállítása.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-11 | OP-11 | `docker/api.Dockerfile` multi-stage build (build + runtime stage) | `chore/OP-11-dockerfile` |
| T-12 | OP-12 | `docker-compose.yml` (api + PostgreSQL 16) | `chore/OP-12-docker-compose` |
| T-13 | OP-13 | `docker-compose.staging.yml` (staging specifikus konfig, Nginx) | `chore/OP-13-docker-compose-staging` |
| T-14 | OP-14 | `docker/nginx.conf` reverse proxy konfiguráció | `chore/OP-14-nginx-config` |

**Elfogadási kritérium:** `docker compose up` után az API elérhető `http://localhost:5000`-en, PostgreSQL kapcsolat működik.

---

## EPIC-2 — Todo alkalmazás backend

> Cél: a Todo CRUD API implementálása Vertical Slice Architecture-ben.  
> SPEC.md fázis: **Fázis 1**

---

### UC-2.1 — Adatbázis és infrastruktúra réteg

**Leírás:** EF Core DbContext, entitás, migration és repository alap felállítása.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-15 | OP-15 | `Status` és `Priority` enum-ok definiálása | `feature/OP-15-enums` |
| T-16 | OP-16 | `TodoItem` entitás létrehozása (SPEC.md 3.1 alapján, összes mezővel) | `feature/OP-16-todoitem-entity` |
| T-17 | OP-17 | `TodoDbContext` + EF Core konfiguráció (Fluent API, UTC timestamp konvenciók) | `feature/OP-17-dbcontext` |
| T-18 | OP-18 | Initial migration létrehozása + `Program.cs`-ben auto-migrate | `feature/OP-18-initial-migration` |
| T-19 | OP-19 | Connection string konfiguráció (`appsettings.json` + environment variable) | `chore/OP-19-connection-string` |

**Elfogadási kritérium:** `dotnet ef migrations list` mutatja az initial migration-t, `docker compose up` után a tábla létrejön.

---

### UC-2.2 — Todo lista lekérdezése

**Leírás:** Felhasználó le tudja kérdezni az összes Todo elemet, szűréssel.

**Aktor:** Fejlesztő / HTMX frontend  
**Végpont:** `GET /api/todos`

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-20 | OP-20 | `GetTodos` feature slice: query handler, endpoint regisztráció | `feature/OP-20-get-todos` |
| T-21 | OP-21 | Szűrés implementálása: `status`, `priority`, `tag` query paraméterek | `feature/OP-21-todos-filter` |
| T-22 | OP-22 | xUnit integrácios teszt: lista lekérdezés, szűrés edge case-ek (Copilot váz + fejlesztő írja) | `feature/OP-22-get-todos-tests` |

**Elfogadási kritérium:** `GET /api/todos?status=Open&priority=High` visszaadja a szűrt listát. Üres lista esetén `200 OK` üres tömbbel tér vissza (nem 404).

---

### UC-2.3 — Todo elem lekérdezése

**Leírás:** Felhasználó le tud kérdezni egyetlen Todo elemet ID alapján.

**Aktor:** Fejlesztő / HTMX frontend  
**Végpont:** `GET /api/todos/{id}`

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-23 | OP-23 | `GetTodoById` feature slice: query handler, endpoint regisztráció | `feature/OP-23-get-todo-by-id` |
| T-24 | OP-24 | xUnit teszt: létező ID, nem létező ID (404), érvénytelen GUID formátum (400) | `feature/OP-24-get-todo-by-id-tests` |

**Elfogadási kritérium:** Nem létező ID esetén `404 Not Found` + problem details válasz. Érvénytelen GUID esetén `400 Bad Request`.

---

### UC-2.4 — Todo elem létrehozása

**Leírás:** Felhasználó új Todo elemet tud létrehozni.

**Aktor:** Fejlesztő / HTMX frontend  
**Végpont:** `POST /api/todos`

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-25 | OP-25 | `CreateTodo` feature slice: command, handler, FluentValidation validator | `feature/OP-25-create-todo` |
| T-26 | OP-26 | Endpoint regisztráció, `201 Created` + Location header visszaadása | `feature/OP-26-create-todo-endpoint` |
| T-27 | OP-27 | xUnit teszt: sikeres létrehozás, validáció hibák (üres Title, túl hosszú Title), duplicate kezelés | `feature/OP-27-create-todo-tests` |

**Elfogadási kritérium:** Sikeres létrehozás után `201 Created` + `Location: /api/todos/{id}`. Hiányzó Title esetén `400 Bad Request` + validációs hibák listája.

---

### UC-2.5 — Todo elem módosítása

**Leírás:** Felhasználó módosítani tudja egy meglévő Todo elem összes adatát.

**Aktor:** Fejlesztő / HTMX frontend  
**Végpont:** `PUT /api/todos/{id}`

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-28 | OP-28 | `UpdateTodo` feature slice: command, handler, FluentValidation validator | `feature/OP-28-update-todo` |
| T-29 | OP-29 | `UpdatedAt` automatikus UTC frissítés az EF Core interceptorral | `feature/OP-29-updated-at-interceptor` |
| T-30 | OP-30 | xUnit teszt: sikeres módosítás, nem létező ID, validációs hibák, optimistic concurrency | `feature/OP-30-update-todo-tests` |

**Elfogadási kritérium:** `200 OK` + frissített entitás visszaadása. `UpdatedAt` mindig frissül. Nem létező ID esetén `404`.

---

### UC-2.6 — Todo státusz frissítése

**Leírás:** Felhasználó csak a Todo státuszát tudja frissíteni (részleges módosítás).

**Aktor:** Fejlesztő / HTMX frontend  
**Végpont:** `PATCH /api/todos/{id}/status`

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-31 | OP-31 | `UpdateTodoStatus` feature slice: command, handler, státuszgép validáció | `feature/OP-31-update-todo-status` |
| T-32 | OP-32 | xUnit teszt: érvényes státuszátmenetek, érvénytelen átmenet (pl. `Done → Open`) | `feature/OP-32-update-status-tests` |

**Elfogadási kritérium:** Érvénytelen státuszátmenet esetén `422 Unprocessable Entity` + hibaüzenet. Érvényes átmenet: `200 OK`.

---

### UC-2.7 — Todo elem törlése

**Leírás:** Felhasználó törölni tud egy Todo elemet.

**Aktor:** Fejlesztő / HTMX frontend  
**Végpont:** `DELETE /api/todos/{id}`

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-33 | OP-33 | `DeleteTodo` feature slice: command, handler | `feature/OP-33-delete-todo` |
| T-34 | OP-34 | xUnit teszt: sikeres törlés, nem létező ID, már törölt elem | `feature/OP-34-delete-todo-tests` |

**Elfogadási kritérium:** Sikeres törlés: `204 No Content`. Nem létező ID: `404 Not Found`.

---

### UC-2.8 — Health check endpoint

**Leírás:** A CI/CD pipeline és a staging deploy health checkhez szükséges endpoint.

**Végpont:** `GET /api/health`

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-35 | OP-35 | Health check endpoint implementálása (`Microsoft.Extensions.Diagnostics.HealthChecks`) | `feature/OP-35-health-check` |
| T-36 | OP-36 | MSSQL health check hozzáadása (DB kapcsolat ellenőrzés) | `feature/OP-36-db-health-check` |

**Elfogadási kritérium:** `GET /api/health` → `200 OK` `{"status":"Healthy"}`. DB leállás esetén `503 Service Unavailable`.

---

## EPIC-3 — HTMX Frontend

> Cél: a Todo alkalmazás HTMX 2.0 + Tailwind CSS 4 alapú frontend-jének implementálása.  
> SPEC.md fázis: **Fázis 1**

---

### UC-3.1 — UI design és alap layout

**Leírás:** A Claude által tervezett UI alap struktúrájának implementálása.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-37 | OP-37 | Tailwind CSS 4 + `htmx.min.js` beállítása a `wwwroot`-ban | `chore/OP-37-frontend-setup` |
| T-38 | OP-38 | Alap layout Razor view (`_Layout.cshtml`) — navigáció, header, footer | `feature/OP-38-base-layout` |
| T-39 | OP-39 | Claude-tervezett UI design implementálása (szín paletta, tipográfia, kártya komponensek) | `feature/OP-39-ui-design` |

**Elfogadási kritérium:** Az alkalmazás főoldala betölt, a layout konzisztens, mobilon is használható.

---

### UC-3.2 — Todo lista nézet

**Leírás:** Felhasználó látja az összes Todo elemet listában, szűrési lehetőséggel.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-40 | OP-40 | `GET /todos` Razor page + HTMX lista partial (`_TodoList.cshtml`) | `feature/OP-40-todo-list-view` |
| T-41 | OP-41 | Szűrő form (státusz, prioritás) HTMX `hx-get` + `hx-target` implementálása | `feature/OP-41-todo-filter-ui` |
| T-42 | OP-42 | Prioritás és státusz badge-ek vizuális megjelenítése (Tailwind színkódokkal) | `feature/OP-42-todo-badges` |

**Elfogadási kritérium:** Szűrő változtatásakor a lista oldal-újratöltés nélkül frissül (HTMX). Üres lista esetén informatív üzenet jelenik meg.

---

### UC-3.3 — Todo létrehozása és szerkesztése

**Leírás:** Felhasználó új Todo elemet tud létrehozni és meglévőt szerkeszteni, modális formban.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-43 | OP-43 | Create form partial (`_TodoForm.cshtml`) HTMX `hx-post` implementálással | `feature/OP-43-create-todo-form` |
| T-44 | OP-44 | Edit form partial: adatok betöltése (`hx-get`), mentés (`hx-put`) | `feature/OP-44-edit-todo-form` |
| T-45 | OP-45 | Validációs hibák inline megjelenítése HTMX-szel (szerver-side, no JS) | `feature/OP-45-form-validation-ui` |

**Elfogadási kritérium:** Sikeres mentés után a lista automatikusan frissül (`hx-target`, `hx-swap`). Validációs hiba esetén a form megmarad a hibák megjelenítésével.

---

### UC-3.4 — Státusz toggle

**Leírás:** Felhasználó egy kattintással tudja változtatni a Todo státuszát a listából.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-46 | OP-46 | Státusz toggle gomb HTMX `hx-patch` + optimista UI frissítés | `feature/OP-46-status-toggle` |
| T-47 | OP-47 | Törlés gomb HTMX `hx-delete` + megerősítő dialog (`hx-confirm`) | `feature/OP-47-delete-ui` |

**Elfogadási kritérium:** Státusz váltás azonnal látható a UI-ban (HTMX swap), oldal újratöltés nélkül.

---

## EPIC-4 — CI Pipeline

> Cél: a GitHub Actions CI workflow felállítása kódminőség-ellenőrzéssel és SonarCloud integrációval.  
> SPEC.md fázis: **Fázis 2**

---

### UC-4.1 — Self-hosted runner telepítése

**Leírás:** A LINVDOCK1T szerveren a GitHub Actions self-hosted runner üzembe helyezése.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-48 | OP-48 | Docker CE telepítése LINVDOCK1T-re (ha még nincs) | `chore/OP-48-docker-ce-install` |
| T-49 | OP-49 | GitHub Actions self-hosted runner regisztrálása (`fphgov/todo-ai-demo` repo-hoz) | `chore/OP-49-runner-registration` |
| T-50 | OP-50 | Runner Docker-in-Docker (DinD) képesség ellenőrzése + tesztelése | `chore/OP-50-runner-dind-test` |

**Elfogadási kritérium:** A runner online állapotban látható a GitHub repo → Settings → Actions → Runners oldalon.

---

### UC-4.2 — SonarCloud projekt beállítása

**Leírás:** SonarCloud projekt létrehozása és GitHub Actions integrációja.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-51 | OP-51 | SonarCloud projekt létrehozása (GitHub org alatt, publikus) | `chore/OP-51-sonarcloud-setup` |
| T-52 | OP-52 | `SONAR_TOKEN` GitHub Secret beállítása | `chore/OP-52-sonarcloud-secret` |
| T-53 | OP-53 | `sonar-project.properties` fájl létrehozása a repo gyökerében | `chore/OP-53-sonar-properties` |

**Elfogadási kritérium:** SonarCloud dashboard-on megjelenik a projekt, első analízis sikeres.

---

### UC-4.3 — CI workflow implementálása

**Leírás:** A teljes `ci.yml` workflow implementálása a SPEC.md 6.1-es pont alapján.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-54 | OP-54 | `ci.yml` alap struktúra: trigger, job, checkout, .NET setup | `chore/OP-54-ci-base` |
| T-55 | OP-55 | `dotnet format --verify-no-changes` lépés + hiba esetén értesítés | `chore/OP-55-ci-dotnet-format` |
| T-56 | OP-56 | `dotnet build` lépés Roslyn + StyleCop analyzer kimenettel | `chore/OP-56-ci-build` |
| T-57 | OP-57 | `dotnet test` lépés Testcontainers MSSQL-lel + JUnit XML + Coverlet coverage | `chore/OP-57-ci-test` |
| T-58 | OP-58 | SonarCloud scan lépés + Quality Gate ellenőrzés | `chore/OP-58-ci-sonarcloud` |
| T-59 | OP-59 | Copilot Agent: AI code review lépés beállítása (PR komment) | `chore/OP-59-ci-copilot-review` |
| T-60 | OP-60 | Docker image build + GHCR push lépés (csak `main` branch-en) | `chore/OP-60-ci-docker-push` |
| T-61 | OP-61 | Copilot Agent: AI PR description generálás (Quality Gate átmenet után) | `chore/OP-61-ci-pr-description` |
| T-62 | OP-62 | CI badge + SonarCloud badge hozzáadása `README.md`-be | `chore/OP-62-ci-badges` |

**Elfogadási kritérium:** Minden push-ra lefut a CI. Failed build esetén a pipeline piros. Quality Gate bukás esetén a PR blokkolva.

---

## EPIC-5 — CD Pipeline és tagging

> Cél: automatikus staging deploy LINVDOCK1T-re, Git tagging és AI smoke test.  
> SPEC.md fázis: **Fázis 3**

---

### UC-5.1 — CD workflow implementálása

**Leírás:** A `cd-staging.yml` workflow implementálása automatikus tagging-gel és smoke test-tel.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-63 | OP-63 | GitHub Secrets beállítása: `STAGING_SSH_KEY`, `STAGING_HOST` | `chore/OP-63-cd-secrets` |
| T-64 | OP-64 | `cd-staging.yml` alap struktúra: trigger (`main` push), job | `chore/OP-64-cd-base` |
| T-65 | OP-65 | Szemantikus verzió meghatározása + automatikus Git tag létrehozása | `chore/OP-65-cd-tagging` |
| T-66 | OP-66 | SSH deploy lépés: `docker compose pull` + `docker compose up -d` | `chore/OP-66-cd-deploy` |
| T-67 | OP-67 | Health check lépés: `GET /api/health` retry logikával (max 30s, 5x) | `chore/OP-67-cd-healthcheck` |
| T-68 | OP-68 | AI smoke test: Copilot Agent végpontokat tesztel, összefoglalót generál | `chore/OP-68-cd-smoke-test` |

**Elfogadási kritérium:** `main`-re merge után automatikusan létrejön a Git tag, a staging deploy lefut, a smoke test összefoglaló megjelenik a commit kommentjében.

---

## EPIC-6 — OpenProject integráció

> Cél: a CI/CD pipeline és OpenProject kétirányú integrációja AI-alapú task kezeléssel.  
> SPEC.md fázis: **Fázis 4, 5, 6**

---

### UC-6.1 — OpenProject projekt előkészítése

**Leírás:** Az OP projekt struktúra, státuszok és típusok beállítása a CI/CD integrációhoz.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-69 | OP-69 | OP projekt létrehozása: `todo-ai-demo`, verziók (v0.1, v0.2), member-ek | `chore/OP-69-op-project-setup` |
| T-70 | OP-70 | OP Work Package típusok: Feature, Task, Bug, Release | `chore/OP-70-op-wp-types` |
| T-71 | OP-71 | OP státuszgép beállítása: New → In Progress → In Review → Deployed → Closed | `chore/OP-71-op-status-workflow` |
| T-72 | OP-72 | OP API token generálás + `OP_API_TOKEN`, `OP_BASE_URL`, `OP_PROJECT_ID` Secrets beállítása | `chore/OP-72-op-secrets` |

**Elfogadási kritérium:** Az OP projektben a státuszgép helyesen működik, az API token-nel CRUD műveletek elvégezhetők.

---

### UC-6.2 — CI/CD → OpenProject alap integráció

**Leírás:** Build és deploy eredmények automatikus visszaírása OpenProjectbe.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-73 | OP-73 | `op-integration.yml` reusable workflow alap struktúra (inputok, OP REST API hívás) | `feature/OP-73-op-workflow-base` |
| T-74 | OP-74 | Build eredmény → OP WP komment (sikeres / sikertelen) | `feature/OP-74-op-build-comment` |
| T-75 | OP-75 | Deploy sikeres → OP WP státusz frissítés (`In Progress → Deployed`) | `feature/OP-75-op-deploy-status` |

**Elfogadási kritérium:** Build futás után az érintett OP Work Package-en megjelenik a build státusz komment.

---

### UC-6.3 — AI-alapú bug task létrehozás

**Leírás:** Sikertelen build esetén a CI pipeline automatikusan létrehoz egy Bug Work Package-et AI-elemzéssel.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-76 | OP-76 | `.github/scripts/analyze-build-failure.js` script (GitHub Models API / Copilot) | `feature/OP-76-build-failure-script` |
| T-77 | OP-77 | CI workflow integrálása: failed build log → script → OP Bug WP létrehozás | `feature/OP-77-ci-bug-creation` |
| T-78 | OP-78 | Bug WP mezők: cím, AI-generált leírás, assignee (utolsó committer), priority: High | `feature/OP-78-bug-wp-fields` |

**Elfogadási kritérium:** Szándékosan elrontott build esetén automatikusan létrejön az OP Bug WP AI-generált root cause elemzéssel, az utolsó committer-hez rendelve.

---

### UC-6.4 — PO feladatkiosztási asszisztens

**Leírás:** PO természetes nyelvű igényéből AI automatikusan létrehozza és assignálja az OP Work Package-eket.

| Task | OP WP | Leírás | Branch |
|---|---|---|---|
| T-79 | OP-79 | OP webhook konfiguráció: új WP létrehozás esemény → GitHub Actions trigger | `feature/OP-79-op-webhook` |
| T-80 | OP-80 | `.github/scripts/po-task-assistant.js` script (GitHub Models API, JSON válasz) | `feature/OP-80-po-assistant-script` |
| T-81 | OP-81 | GitHub Actions workflow: webhook fogadás → Copilot elemzés → OP sub-tasks visszaírás | `feature/OP-81-po-workflow` |
| T-82 | OP-82 | PO értesítés: OP notification a létrehozott sub-task-okról | `feature/OP-82-po-notification` |

**Elfogadási kritérium:** PO egy természetes nyelvű igényt ír be az OP-be → automatikusan létrejönnek a lebontott task-ok, az ajánlott assignee-vel.

---

## Task összesítő

| EPIC | UC-k | Task-ok | SPEC fázis |
|---|---|---|---|
| EPIC-1 Projekt alapok | UC-1.1 — UC-1.3 | T-01 — T-14 | Fázis 1 |
| EPIC-2 Backend | UC-2.1 — UC-2.8 | T-15 — T-36 | Fázis 1 |
| EPIC-3 Frontend | UC-3.1 — UC-3.4 | T-37 — T-47 | Fázis 1 |
| EPIC-4 CI Pipeline | UC-4.1 — UC-4.3 | T-48 — T-62 | Fázis 2 |
| EPIC-5 CD Pipeline | UC-5.1 | T-63 — T-68 | Fázis 3 |
| EPIC-6 OP Integráció | UC-6.1 — UC-6.4 | T-69 — T-82 | Fázis 4-6 |
| **Összesen** | **20 UC** | **82 Task** | — |

---

## OpenProject WP létrehozási sorrend

Az alábbi sorrend ajánlott az OP-ban a Work Package-ek felvitelénél, a függőségek miatt:

```
1. EPIC-1 (T-01..T-14) — nincs függőség, elsőként
2. EPIC-2 (T-15..T-36) — EPIC-1 után (solution kell)
3. EPIC-3 (T-37..T-47) — EPIC-2 párhuzamosan vagy után
4. EPIC-4 (T-48..T-62) — EPIC-2 és EPIC-3 után (van mit tesztelni)
5. EPIC-5 (T-63..T-68) — EPIC-4 után (CI kell a CD-hez)
6. EPIC-6 (T-69..T-82) — EPIC-5 után (staging kell az integrációhoz)
```

---

*Ez a TASKS.md a SPEC.md-ből levezetett, élő dokumentum. Minden új UC vagy task visszakerül ide, és az OpenProject WP-kkel szinkronban marad.*
