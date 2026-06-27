# ADR-005 — Conventional Commits elnevezési konvenció

**Státusz:** Elfogadva  
**Dátum:** 2026-06-28  
**Szerző:** Hatvani Sándor

---

## Kontextus

A projekt branch- és commit-elnevezése kezdetben ad hoc volt. A CI/CD pipeline fejlesztése során (EPIC-4, T-65) felmerült az igény egy egységes, gépileg is értelmezhető konvencióra, amely:

- egyértelműen jelzi a változás típusát (`feat`, `fix`, `chore`, stb.)
- tartalmazza az OpenProject task azonosítót (`OP-{id}`)
- kompatibilis a Conventional Commits szabvánnyal, amely széles körben elfogadott

A projekt jelenleg nem használ automatikus changelog-generálást vagy szemantikus verzióváltást commit üzenet alapján — a verziózás a CD pipeline-ban shell scripttel történik (patch bump, ADR-005-től független). A konvenció tehát elsősorban **olvashatósági és nyomon-követhetőségi** célt szolgál.

---

## Döntés

A projekt a **Conventional Commits 1.0** formátumot követi, kiegészítve az OpenProject task azonosítóval:

```
<típus>(OP-<id>): <rövid leírás>
```

**Elfogadott típusok:**

| Típus | Mikor használandó |
|---|---|
| `feat` | Új funkció (SPEC szerinti feature) |
| `fix` | Hibajavítás |
| `chore` | Build, CI/CD, konfiguráció — nem termékkód |
| `docs` | Csak dokumentáció változott (ADR, README, SPEC) |
| `test` | Tesztek hozzáadása vagy módosítása |
| `refactor` | Kód átírás, viselkedés változtatása nélkül |
| `style` | Formázás, whitespace, dotnet format |

**Példák:**

```
feat(OP-20): GetTodos endpoint szűrési logika
fix(OP-76): sonar taint warnings in log statements
chore(OP-65): szemantikus verzióváltás CD pipeline-ban
docs(OP-65): ADR-005 conventional commits konvenció
test(OP-32): UpdateTodoStatus edge case-ek
```

**Branch elnevezés** (párhuzamos konvenció):

```
<típus>/OP-<id>-<rövid-leírás-kebab-case>
```

Példa: `feature/OP-20-get-todos-endpoint`, `chore/OP-65-semantic-versioning`

---

## Következmények

**Pozitív:**
- A commit üzenetből azonnal látható a változás típusa és az érintett task
- Az OpenProject task azonosító alapján a commit visszakereshető a projekt managementben
- Kompatibilis a Conventional Commits ökoszisztémával (commitlint, semantic-release) — ha később szükség van rá, könnyű bevezetni
- A CI pipeline `op-integration.yml`-ben a branch névből kinyert `OP-{id}` alapján automatikusan kommentál az OP WP-re

**Negatív / korlátok:**
- Nincs gépi érvényesítés (nincs commitlint hook) — a konvenció betartása fejlesztői fegyelem kérdése
- A `scope` zárójelben az OP azonosító eltér a hagyományos Conventional Commits `scope`-tól (ahol általában a kódmodul neve áll) — ez tudatos kompromisszum a projekt management integráció javára

**Kizárt alternatívák:**
- Szabad formátumú commit üzenet: nem ad gépileg feldolgozható információt, nehéz visszakeresni
- Pure Conventional Commits (OP azonosító nélkül): elveszne a PM rendszerrel való közvetlen kapcsolat
- GitHub Issue szám használata OP azonosító helyett: a projekt OpenProject-et használ, nem GitHub Issues-t

---

## Hivatkozások

- [Conventional Commits 1.0 specifikáció](https://www.conventionalcommits.org/en/v1.0.0/)
- CD pipeline szemantikus verziózás: `cd-staging.yml` — patch bump shell script (nem függ a commit üzenet típusától)
- OP integráció: `.github/workflows/op-integration.yml` — branch névből `OP-\K[0-9]+` regex
