# ADR-002 — SonarCloud az önhosztolt SonarQube helyett

**Státusz:** Elfogadva  
**Dátum:** 2026-06-12  
**Szerző:** Hatvani Sándor

---

## Kontextus

A projekt kódminőség-elemzéshez statikus analízis eszközt igényel, amely:
- Integrálható a GitHub Actions CI pipeline-ba
- Képes C# / .NET kódot elemezni (code smells, security hotspots, duplications, coverage trend)
- Quality Gate-et tud beállítani, ami blokkol PR merge előtt

Két alternatíva merült fel: SonarQube Community Edition (self-hosted) és SonarCloud (SaaS).

**Megjegyzés:** A Roslyn Analyzers + StyleCop + `dotnet format` már a build pipeline részét képezi. Ezek **nem helyettesítik** a SonarCloudot: a Roslyn szintaxis szintű ellenőrzést végez, a SonarCloud szemantikus szintű elemzést (logikai hibák, komplexitás, duplikációk) — a két eszköz kiegészíti egymást.

---

## Döntés

**SonarCloud** (ingyenes tier, publikus repo).

---

## Következmények

**Pozitív:**
- Ingyenes publikus repóhoz — nincs szerver és licenc költség
- Nincs infrastruktúra overhead: nem kell SonarQube szervert fenntartani, frissíteni, backuppolni
- GitHub Actions integráció natív és jól dokumentált (`SonarSource/sonarcloud-github-action`)
- Quality Gate eredménye megjelenik közvetlenül a GitHub PR-on
- Automatikusan skálázódik, nem kell kapacitást tervezni

**Negatív / korlátok:**
- Internet-hozzáférés szükséges a CI agent-ről SonarCloud felé (LINVDOCK1T-ről ki kell menni)
- Publikus repo esetén a kód látható a SonarCloud dashboardon — ez tudatos döntés (demo projekt)
- Privát repo esetén fizetős tier kellene

**Kizárt alternatíva — SonarQube Community self-hosted:**
- Szerver fenntartás szükséges (LINVDOCK1-en vagy külön VM-en)
- Community Edition-ben nincs branch analízis (csak `main` branch elemzése)
- Frissítések manuálisak
- A demo projekt komplexitásához aránytalanul nagy overhead
