# Fejlesztési Workflow — 3. lépés: Implementáció

> **Típus:** Általános FPH fejlesztési szabvány  
> **Alkalmazható:** Minden új FPH projekt implementációs fázisában  
> **Szerző:** Hatvani Sándor (FPH)  
> **Verzió:** 0.1 (folyamatosan bővül a TodoAiDemo projekt tapasztalatai alapján)

---

## Áttekintés

Az implementációs fázis a tervezési dokumentumok (SPEC.md, TASKS.md) alapján zajlik. Minden task egy OpenProject Work Package-hez kötődik, saját branchen valósul meg, és PR-on keresztül kerül a `develop` branch-re.

---

## 1. Git munkafolyamat

### Branch stratégia

```
feature/OP-{id}-{rovid-leiras}   # új funkció
bugfix/OP-{id}-{rovid-leiras}    # hibajavítás
chore/OP-{id}-{rovid-leiras}     # infrastruktúra, konfig
hotfix/OP-{id}-{rovid-leiras}    # éles gyorsjavítás
```

A `{id}` az OpenProject Work Package sorszáma. Branch nevet ne adj mielőtt a WP létezik OP-ban.

### Egy task életciklusa

```
1. OP-ban a WP státusza: In Progress
        ↓
2. git checkout develop && git pull
        ↓
3. git checkout -b chore/OP-{id}-{leiras}
        ↓
4. Implementáció (kód / konfig / docs)
        ↓
5. git add . && git commit -m "chore(OP-{id}): leírás"
        ↓
6. git push -u origin chore/OP-{id}-{leiras}
        ↓
7. PR nyitás develop-ra GitHubon
        ↓
8. PR merge
        ↓
9. OP-ban a WP státusza: Closed
```

> ⚠️ A 9. lépés (OP státusz frissítés) jelenleg manuális. Az EPIC-5 CI/CD pipeline automatizálja.

---

## 2. Commit konvenciók

Az FPH projektek a [Conventional Commits](https://www.conventionalcommits.org/) szabványt követik.

### Commit üzenet formátuma

```
{típus}(OP-{id}): rövid leírás
```

Példák:
```
chore(OP-11): add multi-stage api.Dockerfile
feat(OP-25): add CreateTodo feature slice
fix(OP-57): handle null reference in GetTodoById
docs(OP-06): add CLAUDE.md planning-first instructions
test(OP-22): add GetTodos integration tests
ci(OP-66): add GitHub Actions CI workflow
```

### Commit típusok

| Típus | Mikor használjuk |
|---|---|
| `feat` | Új funkció (endpoint, UI elem, feature slice) |
| `fix` | Hibajavítás |
| `chore` | Infrastruktúra, konfig, tooling — nem befolyásolja az alkalmazás logikáját |
| `docs` | Csak dokumentáció változott |
| `test` | Teszt hozzáadás vagy módosítás |
| `refactor` | Kód átírás funkció változtatás nélkül |
| `ci` | CI/CD pipeline változás |

### Miért fontos

- A changelog automatikusan generálható a commit típusokból
- A CI pipeline típus szerint különbözően reagálhat (pl. `chore` commit nem triggerel release-t)
- Az `OP-{id}` a commit üzenetben összeköti a változást az OpenProject Work Package-dzsel

---

## 3. PR folyamat

### Kötelező szabályok

- A PR célja mindig **`develop`** — soha nem `main`
- GitHubon PR nyitásakor a **"base" dropdown**-ban ellenőrizd: `develop` legyen kiválasztva
- `main`-re csak `develop`-ból kerülhet kód, release PR-on keresztül

### Gyakori hiba — PR `main`-re megy `develop` helyett

Ha már merge-elted a PR-t `main`-re, a `develop` branch lemarad. Helyreállítás:

```bash
git fetch origin
git checkout develop
git merge origin/main
git push
```

> ⚠️ `git merge main` (local main) helyett mindig `git merge origin/main` (remote main) — a local main elavult lehet.

### `git stash` használata merge előtt

Ha módosított fájlok vannak és merge-elni kell:

```bash
git stash                  # félreteszi a változásokat
git merge origin/main      # merge
git push
git stash pop              # visszahozza a változásokat
```

Ha `stash pop` után conflict van, de `findstr "<<<" .gitignore` nem mutat markert, a fájl valójában rendben van:

```bash
git add .gitignore
git commit -m "chore: resolve gitignore merge conflict"
git stash drop
```

---

## 4. Copilot használat az IDE-ben

*(Bővítés folyamatban — EPIC-2 tapasztalatai alapján)*

---

## 5. Claude konzultáció az implementáció során

*(Bővítés folyamatban — EPIC-2 tapasztalatai alapján)*

---

## 6. EPIC lezárás

Minden EPIC befejezése előtt el kell végezni az alábbi ellenőrzéseket. Az EPIC csak akkor zárható le, ha mind teljesül.

### Általános EPIC lezárási checklist

```
Kód
[ ] Minden task PR-ja merge-elve develop-ra
[ ] dotnet build hibátlan (0 warning, 0 error)
[ ] dotnet format --verify-no-changes sikeres

Tesztek
[ ] dotnet test — minden teszt zöld
[ ] Új funkciókhoz tartozó tesztek megírva

Futtathatóság
[ ] Az alkalmazás elindul (lokálisan vagy Docker-ben)
[ ] Health check endpoint válaszol: GET /api/health → 200 Healthy

OpenProject
[ ] Minden EPIC-hez tartozó WP státusza: Closed
[ ] Esetleges bugok, hiányzó részek új WP-ként felvéve

Dokumentáció
[ ] SPEC.md és TASKS.md naprakész, ha változás történt
[ ] Új architektúra döntés esetén ADR létrehozva
```

### EPIC-specifikus smoke tesztek

Az általános checklist mellé minden EPIC-nek saját smoke tesztje van — ezeket a projekt `TASKS.md`-je tartalmazza az adott EPIC lezárásánál.

**Példa — EPIC-1 (Projekt setup) smoke teszt:**
```bash
# Docker build
docker build -f ./docker/api.Dockerfile -t todo-api .

# Teljes dev stack indítása
docker compose up -d

# Health check
curl http://localhost:5000/api/health
# Elvárt válasz: Healthy
```

---

## Kapcsolódó workflow lépések

```
  0. Követelmény feltárás
  1. Projekt inicializálás
  2. Tervezés
→ 3. Implementáció (ez a dokumentum)
  4. CI (build, teszt, kódminőség)
  5. CD (staging deploy, smoke test)
  6. OpenProject integráció
```

---

*Ez a dokumentum folyamatosan bővül a TodoAiDemo projekt implementációs tapasztalatai alapján.*
