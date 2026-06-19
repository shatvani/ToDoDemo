# ADR-003 — Claude API a pipeline-ban, GitHub Copilot helyett

**Státusz:** Felülvizsgálva  
**Dátum:** 2026-06-12 (módosítva: 2026-06-19)  
**Szerző:** Hatvani Sándor

---

## Kontextus

A CI/CD pipeline-ban automatizált AI hívásokra van szükség:
- Build failure log elemzés → root cause összefoglaló → OpenProject Bug WP létrehozás
- Deploy utáni smoke test összefoglaló
- PR description automatikus generálás
- PO természetes nyelvű igény → strukturált task lebontás

A kérdés: **melyik AI-t hívja a pipeline automatikusan?**

Két jelölt: Claude API (Anthropic) és GitHub Models API (Copilot / Azure AI Inference).

---

## Döntés

A pipeline-ban **Claude API** hív AI-t (Anthropic előfizetés keretein belül).  
GitHub Copilot az IDE-ben marad (kód generálás), a pipeline-ban nem fut.

> **Változás az eredeti döntéshez képest (2026-06-19):** Az FPH Claude előfizetésre tér át Copilot helyett. A demo az éles FPH workflow-t szimulálja, ezért a pipeline is Claude API-t használ.

---

## Következmények

**Pozitív:**
- **Egységes AI stack**: ugyanaz a modell (Claude) fut chatben és a pipeline-ban — konzisztens minőség és viselkedés
- **Magasabb minőségű pipeline output**: Claude Sonnet/Opus jobb log elemzést és PR description-t ad mint GPT-4o GitHub Models-on
- **Reális FPH demo**: az éles FPH workflow-t tükrözi, nem egy Copilot-specifikus megközelítést

**Negatív / korlátok:**
- `ANTHROPIC_API_KEY` secret szükséges a pipeline-ban minden környezetben
- Token-alapú költség: minden CI futás API hívást jelent — promptokat optimalizálni kell (csak diff, ne teljes fájl)
- Előfizetés-függőség: ha az FPH visszavált Copilot-ra, a pipeline-t is át kell írni

**AI szerepkörök összefoglalva:**

| Szereplő | Mikor fut | Hogyan |
|---|---|---|
| **Claude** (chat) | Emberi kezdeményezésre | Konzultáció, tervezés, review, docs |
| **Claude API** | Automatikusan, pipeline-ban | Log elemzés, PR description, smoke test összefoglaló, PO task lebontás |
| **GitHub Copilot** | IDE-ben, fejlesztés közben | Kód generálás, xUnit váz, refactor |

**Kizárt alternatíva — GitHub Models API:**
- Modellválaszték korlátozott (GPT-4o) — Claude nem elérhető rajta
- Ha az FPH nem használ Copilot előfizetést, a GitHub Models API sem elérhető
