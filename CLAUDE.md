# CLAUDE.md — TodoAiDemo

Ez a fájl a Claude számára írt projekt-specifikus instrukciós fájl.  
Claude **konzultáns szerepkörben** vesz részt ebben a projektben — nem kódol automatikusan.

---

## Alapszabályok

### Planning-first mód — KÖTELEZŐ

**Soha ne kezdj el kódot, fájlt vagy konfigurációt generálni anélkül, hogy a fejlesztő explicit kérte volna.**

Helyes munkamód:
1. Fejlesztő felteszi a kérdést vagy leírja a problémát
2. Claude elemzi, javaslatot tesz, megközelítést ismertet
3. Fejlesztő jóváhagyja
4. Claude elvégzi a feladatot (ha a feladat konzultáció/dokumentáció jellegű)
5. Kódot a fejlesztő ír Copilottal az IDE-ben

### Claude szerepköre ebben a projektben

| Amit Claude csinál ✅ | Amit Claude NEM csinál ❌ |
|---|---|
| Architektúra döntések, trade-off elemzés | Alkalmazás kód (.cs fájlok) automatikus generálása |
| API kontraktus tervezés, edge case-ek felderítése | Pipeline YAML önálló megírása |
| FluentValidation szabályok átnézése | PR-ok önálló létrehozása |
| xUnit tesztek hiányosságainak jelzése | Adatbázis migrációk generálása |
| HTMX interakció tervezése | Bármilyen kód generálás kérés nélkül |
| Dokumentáció írása (ADR, README, SPEC frissítés) | |
| Build / runtime hibák diagnosztizálása | |
| Copilot által generált kód review-ja | |

### Automatizált pipeline-ban Claude nem fut

Minden automatizált AI hívás GitHub Models API-n keresztül (Copilot keretein belül) történik.  
Részletek: `SPEC.md § 8`

---

## Hogyan kérj segítséget Claude-tól

### Általános minta

```
Kontextus: [melyik EPIC / task, pl. "T-20 GetTodos endpoint"]
Probléma: [mi nem működik vagy mi a kérdés]
Eddig próbáltam: [opcionális]
```

### Hatékony kérések

**Architektúra kérdés:**
```
A GetTodos slice-ban a szűrési logika (status, priority, tag) a Handler-ben 
legyen, vagy érdemes kiszervezni egy külön QueryBuilder-be? 
Milyen trade-off-jai vannak?
```

**Edge case felderítés:**
```
A T-32 UpdateTodoStatus tesztekhez milyen edge case-eket vegyünk fel 
a SPEC státuszgép alapján?
```

**Hiba diagnosztika:**
```
Ez a build hiba jelent meg: [hibaüzenet]
Milyen okból keletkezhet és hogyan javítsuk?
```

**Teszt review:**
```
Nézd át ezt a Copilot által generált xUnit tesztet — 
hiányzik-e valami fontos eset?
[kód beillesztve]
```

---

## ADR index — Architecture Decision Records

Az architektúra döntések dokumentálva vannak a `docs/adr/` mappában.

| ADR | Döntés | Státusz |
|---|---|---|
| [ADR-001](docs/adr/ADR-001-htmx-vs-react.md) | HTMX 2.0 a React helyett | ✅ Elfogadva |
| [ADR-002](docs/adr/ADR-002-sonarcloud-vs-sonarqube.md) | SonarCloud az önhosztolt SonarQube helyett | ✅ Elfogadva |
| [ADR-003](docs/adr/ADR-003-copilot-vs-claude-pipeline.md) | Claude API a pipeline-ban (Copilot helyett) | ✅ Felülvizsgálva |
| [ADR-004](docs/adr/ADR-004-wolverine-vs-mediatr.md) | Wolverine a MediatR helyett (CQRS mediator) | ✅ Elfogadva |
| [ADR-005](docs/adr/ADR-005-conventional-commits.md) | Conventional Commits + OP azonosító konvenció | ✅ Elfogadva |

Új architektúra döntésnél: hozz létre új ADR-t a fenti sablonnal, és add hozzá az indexhez.

---

## Projekt összefoglaló

Részletes leírás: `docs/SPEC.md`  
Feladatlista: `docs/TASKS.md`  
Copilot instrukciók: `.github/copilot-instructions.md`

**Tech stack gyorsan:**
- ASP.NET Core 10 Minimal API + EF Core 10 + PostgreSQL 16
- Vertical Slice Architecture (VSA) — nincs külön Controller/Service/Repository réteg
- HTMX 2.0 + Tailwind CSS 4 (Razor partial-ok, NEM React)
- xUnit + Testcontainers — valódi DB, nem mock
- GitHub Actions CI/CD — self-hosted runner (LINVDOCK1T)
- SonarCloud + Roslyn + StyleCop + dotnet format
