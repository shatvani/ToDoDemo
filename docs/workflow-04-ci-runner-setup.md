# Fejlesztési Workflow — GitHub Actions Self-Hosted Runner telepítése

> **Típus:** Infrastruktúra / CI/CD setup  
> **Alkalmazható:** Minden FPH projekt CI pipeline beállításakor  
> **Verzió:** 1.0  
> **Szerver:** LINVDOCK1T (`adminhs@linvdock1t`)

---

## Áttekintés

A GitHub Actions self-hosted runner egy háttérben futó service a szerveren, ami figyeli a GitHub-ot és futtatja a CI workflow lépéseket. Az FPH projektekben self-hosted runnert használunk, mert:

- A szerver a belső hálózaton van (eléri az OpenProject-et)
- A Docker-in-Docker (DinD) képesség szükséges az image build lépésekhez
- GitHub-hosted runner nem fér hozzá a belső infrastruktúrához

---

## Előfeltételek

- Docker CE telepítve (`docker --version` ellenőrzés)
- Az admin felhasználó tagja a `docker` csoportnak (`id` parancs ellenőrzés)
- GitHub repo létezik és admin jogosultság megvan a Settings eléréséhez

---

## 1. Runner token generálás

GitHub webes felületen:

1. **Repo → Settings → Actions → Runners → New self-hosted runner**
2. Válaszd: **Linux**, **x64**
3. A GitHub generál egy egyszeri tokent — csak rövid ideig érvényes, azonnal használd

---

## 2. Runner letöltés és kicsomagolás

```bash
# A szerveren (SSH):
mkdir /actions-runner && cd /actions-runner
curl -o actions-runner-linux-x64-2.335.1.tar.gz -L \
  https://github.com/actions/runner/releases/download/v2.335.1/actions-runner-linux-x64-2.335.1.tar.gz
tar xzf ./actions-runner-linux-x64-2.335.1.tar.gz
```

> **Verzió:** Mindig a GitHub által javasolt verziót töltsd le — az oldal mutatja az aktuálisat.

---

## 3. Jogosultság beállítás

Ha a mappát root-ként hoztad létre, add át a saját felhasználódnak:

```bash
sudo chown -R [admin]:sudo /actions-runner
```

---

## 4. Runner konfigurálás

```bash
cd /actions-runner
./config.sh --url https://github.com/{github-user}/{repo-neve} --token {GENERALT_TOKEN}
```

A konfiguráció interaktívan kérdez:
- **Runner group:** Enter (default)
- **Runner name:** Enter (alapértelmezetten a hostname, pl. `linvdock1t`)
- **Additional labels:** Enter (opcionális, pl. `dotnet,docker`)
- **Work folder:** Enter (default: `_work`)

---

## 5. Service-ként való regisztrálás

Az `./run.sh` csak manuálisan futtatja a runnert. Service-ként való regisztrálással a szerver újraindítása után automatikusan elindul:

```bash
sudo ./svc.sh install
sudo ./svc.sh start
sudo ./svc.sh status
```

Sikeres eredmény:
```
Active: active (running)
√ Connected to GitHub
Listening for Jobs
```

---

## 6. Docker-in-Docker ellenőrzés

A CI pipeline Docker image build lépéseihez szükséges, hogy a runner hozzáférjen a Docker daemon-hoz:

```bash
docker run --rm docker:dind docker --version
```

Ha visszaad egy Docker verziót, a DinD működik.

---

## 7. Ellenőrzőlista

```
[ ] Docker CE telepítve és fut
[ ] /actions-runner mappa létrehozva, jogosultság helyes (adminhs:sudo)
[ ] Runner letöltve és kicsomagolva
[ ] config.sh lefutott hibamentesen
[ ] Service telepítve: sudo ./svc.sh install
[ ] Service fut: sudo ./svc.sh start
[ ] GitHub repo Settings → Actions → Runners: runner "Online" státuszban
[ ] DinD teszt sikeres: docker run --rm docker:dind docker --version
```

---

## Hibaelhárítás

### `Permission denied` a `config.sh` futtatásakor

```bash
sudo chown -R $USER:sudo /actions-runner
```

### Runner offline a GitHub-on

Ellenőrizd a service státuszát:
```bash
sudo ./svc.sh status
```

Ha leállt, indítsd újra:
```bash
sudo ./svc.sh start
```

### Token lejárt

A GitHub által generált token csak ~1 óráig érvényes. Ha lejárt, generálj újat: **Settings → Actions → Runners → New self-hosted runner**.

---

## Kapcsolódó workflow lépések

```
  0. Követelmény feltárás
  1. Projekt inicializálás
  2. Tervezés
  3. Implementáció
→ 4. CI runner setup (ez a dokumentum)
  5. CI pipeline (ci.yml)
  6. CD pipeline
  7. OpenProject integráció
```

---

*Ez a dokumentum az FPH általános fejlesztési workflow CI runner telepítési lépésének leírása.*
