# ADR-003 — GitHub Copilot / Models API a pipeline-ban, Claude nem

**Státusz:** Elfogadva  
**Dátum:** 2026-06-12  
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

A pipeline-ban **kizárólag GitHub Models API** hív AI-t (Copilot előfizetés keretein belül).  
**Claude nem fut automatikusan** — csak emberi kezdeményezésre, chatben.

---

## Következmények

**Pozitív:**
- **Nulla extra költség**: a GitHub Models API a Copilot előfizetés részeként elérhető, külön API kulcs és billing nélkül. A `GITHUB_TOKEN` automatikusan elérhető minden Actions workflow-ban
- **Egyszerű autentikáció**: nem kell `ANTHROPIC_API_KEY` secret kezelés a pipeline-ban
- **Kontrolált Claude használat**: Claude drágább és erőteljesebb — az on-demand, emberi konzultációra van fenntartva, ahol a kontextus és a gondolkodás minősége számít
- **Auditálhatóság**: a pipeline AI hívások naplózhatók, a GitHub Actions log tartalmazza az összes lépést

**Negatív / korlátok:**
- A GitHub Models API modellválasztéka korlátozottabb (GPT-4o és néhány más modell) — Claude Sonnet/Opus nem elérhető itt
- A pipeline AI válaszok minősége elmaradhat a Claude-tól — ez elfogadható, mert a pipeline feladatok (log elemzés, rövid összefoglalók) nem igényelnek mély kontextust

**AI szerepkörök összefoglalva:**

| Szereplő | Mikor fut | Hogyan |
|---|---|---|
| **Claude** | Emberi kezdeményezésre, chatben | Konzultáció, tervezés, review, docs |
| **GitHub Copilot** | IDE-ben, fejlesztés közben | Kód generálás, xUnit váz, refactor |
| **GitHub Models API** | Automatikusan, pipeline-ban | Log elemzés, PR description, smoke test összefoglaló, PO task lebontás |

**Kizárt alternatíva — Claude API a pipeline-ban:**
- Token-alapú árazás: minden CI futás Claude API hívást jelent → kiszámíthatatlan havi költség
- `ANTHROPIC_API_KEY` secret kezelés szükséges minden környezetben
- Overkill a pipeline feladatokhoz (log elemzés, rövid összefoglalók)
