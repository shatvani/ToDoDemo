# Fejlesztési Workflow — 2. lépés: Projekt inicializálás

> **Típus:** Általános FPH fejlesztési szabvány  
> **Alkalmazható:** Minden új FPH projekt indulásakor, a tervezési fázis előtt  
> **Szerző:** Hatvani Sándor (FPH)  
> **Verzió:** 1.0

---

## Áttekintés

A projekt inicializálás a tervezési fázis **előfeltétele**. Célja, hogy az infrastruktúra (GitHub repo, OpenProject projekt, branch stratégia) már a SPEC.md és TASKS.md megírása előtt a helyén legyen.

> Ha az infrastruktúra nincs kész a tervezés előtt, a TASKS.md OP Work Package ID-k (OP-01, OP-02, …) nem létező WP-kre mutatnak, a branch névkonvenció (`feature/OP-{id}-{leiras}`) nem köthető valódi task-okhoz, és az egész pipeline-integráció csak formálisan létezik.

A fázis elvégzése után a fejlesztő:

- rendelkezik GitHub repóval és helyes branch struktúrával,
- rendelkezik OpenProject projekttel és a Task lebontáshoz szükséges alapstruktúrával,
- bekötötte a GitHub–OpenProject összekötést,
- készen áll a tervezési fázis (SPEC.md, TASKS.md) megírására — valódi OP WP ID-kkel.

---

## Szerepkörök

| Szerepkör | Felelősség |
|---|---|
| **Fejlesztő / Architekt** | Minden lépést manuálisan hajt végre |
| **Claude** | Segít a lépések ellenőrzésében, a struktúra kialakításában, ha kérdés merül fel |

> ⚠️ Ez a fázis alapvetően manuális, UI-alapú munka — GitHub és OpenProject webes felületen. Claude nem tudja ezeket elvégezni helyetted.

---

## A projekt inicializálás lépései

```
1. GitHub repository létrehozás
        ↓
2. Alap branch struktúra (main + develop)
        ↓
3. Helyi repo klónozás + első commit
        ↓
4. OpenProject projekt létrehozás
        ↓
5. OpenProject EPIC-ek és Work Package-ek létrehozása
        ↓
6. GitHub–OpenProject összekötés (OP_API_TOKEN)
        ↓
7. GitHub Secrets beállítása
        ↓
8. Fázis lezárása — átadás a tervezési fázisnak
```

---

## 1. GitHub repository létrehozás

### Hol

GitHub webes felület: [github.com/new](https://github.com/new)

### Beállítások

| Mező | Érték |
|---|---|
| **Owner** | Személyes fiók (ha a szervezeti fiókban nincs elegendő jogosultság) |
| **Repository name** | `{projekt-neve}` — kisbetűs, kötőjeles, pl. `todo-ai-demo` |
| **Visibility** | Public (SonarCloud free tier csak publikus repóhoz) |
| **Initialize** | ✅ Add a README file |
| **Add .gitignore** | ✅ `.NET` template |
| **License** | MIT (opcionális, de ajánlott) |

> **Miért ne hozd létre üres repoként?** Ha README-vel inicializálod, rögtön klónolható és van egy `main` branch.

---

## 2. Alap branch struktúra

A GitHub **Branch protection rules** UI-on (`Settings → Branches`):

### `main` branch szabályok

- ✅ Require a pull request before merging
- ✅ Require approvals: 1 (saját projektnél 0 is elfogadható)
- ✅ Require status checks to pass before merging
  - Add: `ci` (a CI workflow job neve)
- ✅ Do not allow bypassing the above settings

### `develop` branch létrehozása

```bash
git checkout -b develop
git push -u origin develop
```

`develop`-ra szintén érdemes branch protection-t beállítani — legalább a CI check legyen kötelező.

### Branch névkonvenció (emlékeztető)

```
feature/OP-{id}-{rovid-leiras}   # új funkció
bugfix/OP-{id}-{rovid-leiras}    # hibajavítás
chore/OP-{id}-{rovid-leiras}     # infrastruktúra, konfig
hotfix/OP-{id}-{rovid-leiras}    # éles gyorsjavítás
```

> A `{id}` az OpenProject Work Package sorszáma. Branch nevet **ne adj** mielőtt a WP létezik OP-ban!

---

## 3. Helyi repo klónozás + első commit

```bash
git clone https://github.com/{github-user}/{projekt-neve}.git
cd {projekt-neve}

# develop branch-re váltás
git checkout develop
```

Ha már van helyi kód (pl. a tervezési fázisból), most kell push-olni:

```bash
# Meglévő helyi könyvtárból:
git remote add origin https://github.com/{github-user}/{projekt-neve}.git
git branch -M main
git push -u origin main
git checkout -b develop
git push -u origin develop
```

---

## 4. OpenProject projekt létrehozás

### Előfeltétel

OpenProject fut a szerveren (LINVDOCK1: `http://172.22.0.131`). Ha nem fut:

```bash
# LINVDOCK1-en (SSH):
cd /opt/openproject  # vagy ahol telepítve van
docker compose up -d
```

### Projekt létrehozás (webes UI)

1. Bejelentkezés: `http://172.22.0.131`
2. **Projects → New project**
3. Beállítások:

| Mező | Érték |
|---|---|
| **Name** | `{Projekt neve}` — pl. `Todo AI Demo` |
| **Identifier** | `todo-ai-demo` (URL slug, automatikusan generálódik) |
| **Modules** | ✅ Work packages, ✅ Backlogs (opcionális) |

---

## 5. OpenProject EPIC-ek és Work Package-ek létrehozása

### EPIC-ek létrehozása

OpenProjectben az EPIC-eknek a **Phase** típusú Work Package felel meg.

Minden EPIC-hez hozz létre egy Phase WP-t, alá pedig **Task** típusú WP-ket a TASKS.md alapján.

Az alá rendeléshez a Task WP-n belül a „Parent" mezőbe kell beállítani az EPIC Phase WP-t.

### A WP sorszám és a TASKS.md összekötése

A TASKS.md-ben minden task tartalmaz egy `OP WP` oszlopot (`OP-01`, `OP-02`, …). Ezek az OP-ban automatikusan generált WP-azonosítókra mutatnak. Ha az OP más sorszámot generál mint várták, a TASKS.md-t frissíteni kell.

> **Tipp:** Az OP API-val batch-ben is létre lehet hozni WP-ket. Ha sok task van, érdemes ezt megvizsgálni a manuális rögzítés helyett. Claude segíthet az API hívások összeállításában, ha kéred.

---

## 6. GitHub–OpenProject összekötés

Az összekötés célja: a CI pipeline automatikusan frissíti az OP Work Package státuszát, amikor egy branch push-olódik vagy egy PR merge-elődik.

### OP API token generálás

1. OpenProject → **My account → Access tokens**
2. **Generate new token**
3. Scope: `api_v3`
4. Másold ki a tokent — csak egyszer látható!

---

## 7. GitHub Secrets beállítása

**GitHub repo → Settings → Secrets and variables → Actions → New repository secret**

| Secret neve | Értéke |
|---|---|
| `OP_API_TOKEN` | Az OpenProject API token (6. lépésből) |
| `OP_BASE_URL` | `http://172.22.0.131` (vagy a szerver nyilvános URL-je) |

> ⚠️ Ha a GitHub Actions self-hosted runneren fut (LINVDOCK1T), az `OP_BASE_URL` belső IP-vel is működik, mert a runner és az OP ugyanazon a hálózaton van. Public GitHub-hosted runner esetén az OP-nek kívülről elérhetőnek kell lennie.

---

## 8. Fázis lezárása — átadás a tervezési fázisnak

### Mit tudunk most, amit a tervezés előtt nem tudtunk

- A GitHub repo URL-je → bekerül a `README.md`-be és a `SPEC.md`-be
- Az OP Work Package sorszámok (OP-01…OP-N) → bekerülnek a `TASKS.md`-be
- A branch névkonvenció élő, mert az OP WP-k léteznek

### Ellenőrzőlista

```
GitHub
[ ] Repo létrehozva (publikus, .gitignore .NET, README)
[ ] main branch protection beállítva (CI check kötelező)
[ ] develop branch létrehozva és push-olva
[ ] Helyi klón működik, develop branch aktív

OpenProject
[ ] OP szerver fut és elérhető
[ ] Projekt létrehozva
[ ] EPIC-ek létrehozva (Phase WP-k)
[ ] Összes task WP létrehozva
[ ] WP sorszámok egyeznek a tervezett TASKS.md ID-kkel

Összekötés
[ ] OP API token generálva
[ ] OP_API_TOKEN beállítva GitHub Secrets-ben
[ ] OP_BASE_URL beállítva GitHub Secrets-ben
[ ] Self-hosted runner hálózati elérés ellenőrizve

Átadás
[ ] Átadva a tervezési fázisnak (SPEC.md, TASKS.md)
[ ] A tervezési fázisban a tényleges OP WP ID-k szerepelnek
```

---

## Kapcsolódó workflow lépések

```
  0. Követelmény feltárás
→ 1. Projekt inicializálás (ez a dokumentum — GitHub repo, OP projekt, branch stratégia)
  2. Tervezés (SPEC.md, TASKS.md, copilot-instructions.md, CLAUDE.md, README.md)
  3. Implementáció
  4. CI (build, teszt, kódminőség)
  5. CD (staging deploy, smoke test)
  6. OpenProject integráció
```

---

*Ez a dokumentum az FPH általános fejlesztési workflow második lépésének leírása.  
A teljes workflow dokumentáció a TodoAiDemo projekt tapasztalatai alapján készül el.*
