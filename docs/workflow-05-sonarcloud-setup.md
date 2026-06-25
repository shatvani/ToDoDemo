# Fejlesztési Workflow — SonarCloud beállítása

> **Típus:** Infrastruktúra / CI/CD setup  
> **Alkalmazható:** Minden FPH projekt CI pipeline beállításakor  
> **Verzió:** 1.0  
> **Kapcsolódó task-ok:** T-51, T-52, T-53, T-58

---

## Áttekintés

A SonarCloud egy felhőalapú kódminőség-elemző eszköz, amely statikus analízist végez a forráskódon, és a CI pipeline részeként Quality Gate-et alkalmaz. Az FPH projektekben azért használjuk, mert:

- Publikus GitHub repókhoz ingyenes (free tier)
- GitHub Actions-szel natívan integrálható
- Kód coverage, code smells, security vulnerabilities, duplicációk vizuálisan követhetők
- Quality Gate blokkolhatja a PR merge-ét, ha a kód nem felel meg a minőségi kritériumoknak

---

## Előfeltételek

- GitHub repo létezik és publikus
- GitHub Actions CI pipeline alapja kész (`ci.yml` alap struktúra megvan)
- Admin jogosultság a GitHub repón (Secrets beállításhoz)

---

## 1. Bejelentkezés SonarCloud-ba

[sonarcloud.io](https://sonarcloud.io) → **Log in with GitHub**

Ha először jelentkezel be, SonarCloud automatikusan létrehozza a fiókot a GitHub azonosítód alapján. Külön regisztráció nem szükséges.

---

## 2. GitHub szervezet összekapcsolása

Bejelentkezés után SonarCloud megkérdezi, melyik GitHub szervezet vagy személyes fiók alatt szeretnél projektet létrehozni.

1. Válaszd ki a megfelelő GitHub fiókot / szervezetet
2. SonarCloud kéri a **GitHub App** telepítését — ez szükséges ahhoz, hogy a repót elérje
3. Az App telepítésekor kiválaszthatod, hogy **minden repóra** vagy csak **kiválasztott repókra** adod meg a hozzáférést — utóbbi ajánlott

> Ha a GitHub App már telepítve van (pl. korábban más projektnél), csak az új repót kell hozzáadni az App beállításainál: **GitHub → Settings → Applications → SonarCloud → Configure → Repository access**.

---

## 3. Projekt importálása

**SonarCloud → My Projects → Analyze new project → Import from GitHub**

1. Keress rá a repo nevére (pl. `ToDoDemo`)
2. Jelöld be a repót
3. Kattints a **Set Up** gombra

> Ha a repo nem jelenik meg a listában, ellenőrizd a GitHub App hozzáférési beállításait (2. lépés).

---

## 4. Automatic analysis kikapcsolása

SonarCloud alapértelmezetten **Automatic analysis** módban elemzi a kódot — ez azt jelenti, hogy ő maga futtatja az elemzést GitHub Actions nélkül.

Ha GitHub Actions-szel akarjuk futtatni (ami az FPH standard), ezt **ki kell kapcsolni**:

**SonarCloud projekt → Administration → Analysis Method**

- Kapcsold ki az **Automatic analysis** toggle-t

> ⚠️ Ha nem kapcsolod ki, a GitHub Actions-ből indított analízis és az automatikus analízis ütközni fog, és az elemzés hibás eredményt adhat.

---

## 5. SONAR_TOKEN generálása

Az **Analysis Method** oldalon, miután kikapcsoltad az Automatic analysis-t, SonarCloud megmutatja a GitHub Actions beállítási útmutatót. Ezen az oldalon:

- A **2. Create a GitHub Secret** szekcióban látható a generált token értéke
- Ez az érték csak egyszer látható — **azonnal mentsd el**

Ha később kell új token:
**SonarCloud → My Account → Security → Generate Tokens**

---

## 6. SONAR_TOKEN beállítása GitHub Secrets-ben

**GitHub repo → Settings → Secrets and variables → Actions → New repository secret**

| Mező | Érték |
|---|---|
| **Name** | `SONAR_TOKEN` |
| **Value** | A SonarCloud által generált token |

---

## 7. sonar-project.properties létrehozása

A repo gyökerében hozd létre a `sonar-project.properties` fájlt. A **Project Key** és **Organization** értékek a SonarCloud Analysis Method oldalán láthatók.

```properties
sonar.projectKey={project-key}
sonar.organization={organization}
```

Konkrét példa:
```properties
sonar.projectKey=shatvani_ToDoDemo
sonar.organization=shatvani
```

> A `sonar.projectKey` általában `{github-felhasználónév}_{repo-neve}` formátumú. Az `organization` a SonarCloud szervezet neve (általában a GitHub username vagy org slug).

> **Fontos:** A tokent és a branch információt **ne** tedd bele a `sonar-project.properties` fájlba — ezek a CI workflow-ban kerülnek átadásra futásidőben.

---

## 8. CI pipeline integráció (T-58)

A SonarCloud elemzés a `ci.yml`-ben a build és teszt lépések **után** fut. A scanner a build és teszt eredményeket (coverage riport) is elemzi, ezért sorrendben kell elhelyezni.

> ⚠️ A SonarCloud-hoz generált alapértelmezett GitHub Actions YAML (`windows-latest`, `shell: powershell`) **nem kompatibilis** self-hosted Linux runnerrel. Az alábbi az FPH self-hosted runner konfigurációhoz igazított változat.

A `ci.yml` SonarCloud lépései:

```yaml
      - name: Install SonarCloud scanner
        run: dotnet tool install --global dotnet-sonarscanner

      - name: SonarCloud begin
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: |
          dotnet sonarscanner begin \
            /k:"${{ secrets.SONAR_PROJECT_KEY }}" \
            /o:"${{ secrets.SONAR_ORGANIZATION }}" \
            /d:sonar.token="${{ secrets.SONAR_TOKEN }}" \
            /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

      - name: dotnet build
        run: dotnet build --no-incremental

      - name: dotnet test
        run: dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

      - name: SonarCloud end
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
```

> **Megjegyzés:** A SonarCloud scanner a `begin` és `end` parancsok közötti build és teszt lépéseket elemzi. Ha a `begin` előtt vagy az `end` után futnak a lépések, azok eredménye nem kerül be az analízisbe.

---

## 9. Quality Gate

A SonarCloud Quality Gate határozza meg, mikor tekinthető elfogadhatónak a kód minősége. Alapértelmezett feltételek:

- Coverage ≥ 80% az új kódon
- 0 új Security Hotspot
- 0 új Bug
- Duplicáció < 3%

A Quality Gate státusza megjelenik a PR-on GitHub kommentként, és a CI lépés `exit code`-ja alapján a pipeline zöld/piros lesz.

---

## 10. Ellenőrzőlista

```
SonarCloud projekt
[ ] Bejelentkezés GitHub fiókkal
[ ] GitHub App telepítve a repóra
[ ] Projekt importálva (Set Up)
[ ] Automatic analysis kikapcsolva (Administration → Analysis Method)

GitHub
[ ] SONAR_TOKEN beállítva GitHub Secrets-ben

Kód
[ ] sonar-project.properties létrehozva a repo gyökerében
[ ] sonar.projectKey és sonar.organization értékek helyesek

CI pipeline
[ ] ci.yml SonarCloud lépések hozzáadva (T-58)
[ ] fetch-depth: 0 be van állítva a Checkout lépésnél (kötelező SonarCloud-hoz)
[ ] SonarCloud dashboard-on megjelent az első sikeres analízis
```

---

## Hibaelhárítás

### A repo nem jelenik meg az importálásnál

A GitHub App nem fér hozzá a repóhoz. Megoldás: **GitHub → Settings → Applications → SonarCloud → Configure → Repository access** — add hozzá a repót.

### `ERROR: Invalid value for sonar.projectKey`

A `sonar-project.properties`-ben lévő `sonar.projectKey` nem egyezik a SonarCloud-on regisztrált projekt kulcsával. Ellenőrizd a SonarCloud projekt oldalán: **Administration → Update Key**.

### `Automatic Analysis and import of external issues are mutually exclusive`

Az Automatic analysis nincs kikapcsolva. Lépj a **Administration → Analysis Method** oldalra és kapcsold ki.

### `fetch-depth: 0` hiányzik

Ha a Checkout lépésnél `fetch-depth: 0` nincs beállítva, a SonarCloud nem tud megfelelő blame analízist végezni, és a relevancia értékek pontatlanok lesznek. Hibaüzenet: `Shallow clone detected`.

### SonarCloud scanner nem található (`dotnet sonarscanner: command not found`)

A `dotnet tool install --global dotnet-sonarscanner` lépés nem futott le, vagy a PATH nem tartalmazza a global tools könyvtárat. Ellenőrizd:
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet sonarscanner --version
```

---

## Kapcsolódó workflow lépések

```
  0. Követelmény feltárás
  1. Projekt inicializálás
  2. Tervezés
  3. Implementáció
  4. CI runner setup
→ 5. SonarCloud setup (ez a dokumentum)
  6. CI pipeline (ci.yml) — T-58 SonarCloud lépés
  7. CD pipeline
  8. OpenProject integráció
```

---

*Ez a dokumentum az FPH általános fejlesztési workflow SonarCloud beállítási lépésének leírása.*
