# ADR-001 — HTMX 2.0 a React helyett

**Státusz:** Elfogadva  
**Dátum:** 2026-06-12  
**Szerző:** Hatvani Sándor

---

## Kontextus

A Todo alkalmazás frontend-jéhez technológiát kellett választani. A két fő jelölt a React (SPA megközelítés) és a HTMX 2.0 (hypermedia megközelítés) volt.

A projekt elsődleges célja az **AI-támogatott fejlesztői workflow bemutatása**, nem egy komplex frontend alkalmazás építése. A frontend funkcionalitás: lista nézet, create/edit form, státusz toggle, szűrés.

**Szempontok:**
- A projekt demo jellegű — a frontend komplexitása nem cél
- A CI/CD pipeline-t egyszerűen kell tartani
- A backend ASP.NET Core Minimal API + Razor — szerver-side rendering természetes
- Az FPH csapatban nincs dedikált frontend fejlesztő

---

## Döntés

**HTMX 2.0** a Tailwind CSS 4-gyel, Razor partial nézetekkel.

Nincs Node.js build step, nincs `npm install` a pipeline-ban, nincs React bundle. A `htmx.min.js` statikus fájlként kerül a `wwwroot`-ba.

---

## Következmények

**Pozitív:**
- Nincs Node.js függőség — a CI pipeline egyszerűbb (nincs `npm install` / `npm run build` lépés)
- A frontend kód Razor partial-okban él, a backend fejlesztő írja C#-ban
- A szerver-side rendering és az API válaszok (JSON vs HTML) egyértelműen elkülönülnek: HTMX kérésekre HTML partial, API kérésekre JSON
- Könnyen demonstrálható a hypermedia megközelítés előnye (kevesebb JS, egyszerűbb state kezelés)

**Negatív / korlátok:**
- Komplex interaktivitás (pl. drag & drop, real-time frissítés) nehézkesebb — de ez a demo hatókörén kívül van
- A Tailwind CSS 4 JIT compile-hoz (ha szükséges) Node.js kellene; jelen projektben a teljes Tailwind CSS CDN / statikus fájl elegendő
- HTMX kevésbé ismert technológia az FPH csapatban — tanulási görbe

**Kizárt alternatívák:**
- React: Node.js build step, npm függőségek, külön frontend projekt szükséges — ez a demo szempontjából felesleges komplexitás
- Blazor: .NET-natív, de WebAssembly overhead és más programozási modell
