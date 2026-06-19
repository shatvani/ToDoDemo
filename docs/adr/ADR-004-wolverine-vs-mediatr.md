# ADR-004 — Wolverine a MediatR helyett

**Státusz:** Elfogadva  
**Dátum:** 2026-06-17  
**Szerző:** Hatvani Sándor

---

## Kontextus

A Vertical Slice Architecture megközelítésben a feature slice-ok közötti kommunikációhoz és a CQRS pattern megvalósításához közvetítő (mediator) könyvtárat kell választani.

A két fő jelölt:
- **MediatR** — de facto standard a .NET ökoszisztémában, széles körben ismert
- **Wolverine** — modern .NET messaging framework, konvención alapuló handler discovery

**Döntési szempont:** az FPH cégnél a Wolverine-t használjuk éles projektekben, ezért a demo projektben is ezt célszerű alkalmazni — így a tanulási befektetés közvetlenül hasznosul a vállalati munkában.

---

## Döntés

**Wolverine** a CQRS mediator réteghez.

A handler discovery konvenció alapján működik: a `Handle` nevű metódus automatikusan megtalálódik, nem szükséges `IRequestHandler<TRequest, TResponse>` interfészt implementálni.

---

## Következmények

**Pozitív:**
- Az FPH éles projektekben is Wolverine fut — a fejlesztő közvetlen tapasztalatot szerez a vállalati stack-kel
- Konvenció alapú handler discovery — kevesebb boilerplate, nincs interface implementáció
- Beépített support: messaging, saga, retry policy — később bővíthető
- Aktívan fejlesztett, .NET-natív projekt

**Negatív / korlátok:**
- Kevesebb Stack Overflow / tutorial anyag mint MediatR-hez
- A Copilot által generált kód alapértelmezetten MediatR mintát követ — a fejlesztőnek át kell írni Wolverine konvencióra
- Nagyobb függőség (Wolverine több mint egy egyszerű mediator)

**Kizárt alternatívák:**
- MediatR: iparági standard, de az FPH-nál nem használt — a demo célja a vállalati stack bemutatása
- Közvetlen DI (interface nélkül): egyszerűbb, de elvész a slice-ok közötti laza csatolás
