# Fejlesztési Workflow — 0. lépés: Követelmény feltárás

> **Típus:** Általános FPH fejlesztési szabvány  
> **Alkalmazható:** Minden új FPH projekt indulásakor  
> **Szerző:** Hatvani Sándor (FPH)  
> **Verzió:** 1.0

---

## Áttekintés

A követelmény feltárás a fejlesztési workflow **legelső és legkritikusabb lépése**. Célja, hogy az összes érintett fél igényeit, elvárásait és korlátait megismerjük és dokumentáljuk, mielőtt bármilyen tervezési vagy implementációs döntés születik.

> „Ha nem tudod, mit kell építeni, nem számít, milyen jól építed meg."

A követelmény feltárás **nem technikai feladat** — emberekkel való kommunikáció, amelynek eredménye alapozza meg a teljes projektet. Egy rosszul feltárt követelmény rendszer-újraírást eredményezhet. Egy jól feltárt követelmény megspórol hónapokat.

---

## Szerepkörök

| Szerepkör | Felelősség |
|---|---|
| **Fejlesztő / Architekt** | Interjúk levezetése, technikai korlátok tisztázása, dokumentálás |
| **Product Owner (PO)** | Üzleti célok és prioritások meghatározása |
| **Leendő felhasználók** | Valós igények, fájdalompontok, elvárások közlése |
| **Claude** | Interjú kérdések összeállítása, összefoglalók készítése, ellentmondások jelzése, user story-k megfogalmazása |

---

## A követelmény feltárás lépései

```
1. Érintettek azonosítása (stakeholder mapping)
        ↓
2. PO / megrendelő interjú (üzleti célok)
        ↓
3. Felhasználói interjúk (valós igények)
        ↓
4. Meglévő rendszer elemzése (ha van)
        ↓
5. Követelmények összesítése és prioritizálása
        ↓
6. Jóváhagyás és átadás a tervezési fázisnak
```

---

## 1. Érintettek azonosítása

Mielőtt az interjúk elkezdődnek, azonosítani kell mindenkit, akit az új rendszer érint.

### Érintett típusok

| Típus | Kik ők | Mit kell tőlük megtudni |
|---|---|---|
| **Elsődleges felhasználók** | Akik nap mint nap használják | Napi munkafolyamat, fájdalompontok |
| **Másodlagos felhasználók** | Akik ritkábban használják | Speciális igények, ritkább műveletek |
| **Adminisztrátorok** | Akik kezelik a rendszert | Jogosultságok, karbantartás, riportok |
| **Döntéshozók / PO** | Akik megrendelik | Üzleti célok, siker mérőszámai |
| **IT / Sysadmin** | Akik üzemeltetik | Infrastruktúra korlátok, biztonság |
| **Kapcsolódó rendszerek tulajdonosai** | Akikkel integrálni kell | API-k, adatformátumok, korlátok |

### Claude segítségével

Claude segíthet az érintetti lista összeállításában, ha megadod a projekt rövid leírását:

> „Ez egy {projekt típusa} alkalmazás, amelyet {szervezet} {osztálya} fog használni {cél} céljából. Kik lehetnek az érintettjeink?"

---

## 2. PO / Megrendelő interjú

### Cél

Az üzleti kontextus, a projekt indoka és a siker kritériumainak megértése.

### Ajánlott kérdések

**Üzleti cél**
- Mi a projekt elsődleges célja? Milyen problémát old meg?
- Mi történik, ha ezt a rendszert nem építjük meg? (status quo)
- Hogyan illeszkedik ez a szervezet stratégiájába?

**Siker mérőszámai**
- Mikor tekintjük sikeresnek a projektet?
- Van-e konkrét mérőszám? (pl. feldolgozási idő csökkenése, hibaszám csökkenése)
- Ki dönt arról, hogy a projekt sikeres-e?

**Prioritások és MVP**
- Mi az a minimális funkcionalitás, amellyel el lehet indulni? (MVP)
- Mi az, ami fontos, de várhat a második verzióra?
- Mi az, ami „jó lenne, de nem kritikus"?

**Korlátok**
- Mik a határidők?
- Mik a költségkorlátok?
- Vannak-e kötelező technológiai vagy biztonsági elvárások?
- Vannak-e jogszabályi, adatvédelmi követelmények (pl. GDPR)?

**Integrációk**
- Milyen meglévő rendszerekkel kell kommunikálnia?
- Vannak-e kötelező adatformátumok vagy API szabványok?

### Claude segítségével

Az interjú után az összefoglalót Claude-dal lehet elkészíteni:

> „Összefoglalom a PO interjú eredményeit, majd kérem, hogy fogalmazd meg belőle az üzleti követelményeket strukturált formában."

---

## 3. Felhasználói interjúk

### Cél

A valós napi munkafolyamat, a jelenlegi fájdalompontok és az elvárások megértése a tényleges felhasználóktól.

### Fontos alapelvek

- **Ne a megoldásról kérdezz, hanem a problémáról.** Ne azt kérdezd: „Szeretnél-e egy ilyen gombot?", hanem: „Mit csinálsz most, ha X történik?"
- **A felhasználó nem tudja, mit akar** — de pontosan tudja, mi zavar. A fejlesztő feladata ebből kihozni a valós igényt.
- **Legalább 3-5 felhasználót** érdemes megkérdezni — egy ember véleménye nem reprezentatív.
- **Különböző tapasztalati szintű** felhasználókat vonj be (kezdő és haladó egyaránt).

### Ajánlott kérdések

**Jelenlegi helyzet feltérképezése**
- Mesélj el egy tipikus munkanapod! Hogyan végzed el a {feladat típusa} feladatot?
- Milyen eszközöket, rendszereket használsz most ehhez?
- Mennyi időt töltesz el ezzel naponta / hetente?

**Fájdalompontok**
- Mi az, ami a legjobban zavar a jelenlegi folyamatban?
- Mi az, amire a legtöbbet kell várnod?
- Volt már olyan, hogy valami hiba miatt újra kellett kezdened a munkát?
- Mi az, amit mindig elfelejtesz, vagy amit könnyű elrontani?

**Elvárások az új rendszertől**
- Ha ez a rendszer tökéletesen működne, mi változna a munkádban?
- Mi az, ami nélkül biztosan nem tudnád használni?
- Mi az, ami jó lenne, de nem kritikus?
- Milyen eszközön használnád? (asztali gép, laptop, tablet, telefon)

**Használati szokások**
- Hány emberrel dolgozol együtt ezen a feladaton?
- Vannak-e csúcsidőszakok, amikor különösen fontos, hogy működjön?
- Milyen adatokat kell látnia mások is, és mi az, ami csak neked szól?

**Nyitott kérdés**
- Van valami, amit nem kérdeztem meg, de fontosnak tartasz?

### Claude segítségével

Claude segíthet az interjú kérdések testre szabásában:

> „Egy {projekt típusa} alkalmazást tervezünk, amelyet {felhasználó típusa} fog használni {kontextus}-ban. Milyen specifikus kérdéseket tegyünk fel az interjún?"

Az interjúk után Claude segíthet összefoglalni és strukturálni az eredményeket:

> „Az alábbi interjú jegyzetek alapján fogalmazd meg a user story-kat és azonosítsd a közös fájdalompontokat."

---

## 4. Meglévő rendszer elemzése

Ha az új rendszer egy meglévőt vált ki vagy egészít ki:

### Amit meg kell vizsgálni

**Funkcionális elemzés**
- Milyen funkciókat használnak valójában? (nem mindet, amit a rendszer tud)
- Milyen funkciókat hiányolnak?
- Milyen „kerülő utakat" alakítottak ki a felhasználók? (ezek rejtett igények)

**Adatok**
- Milyen adatokat tárol a jelenlegi rendszer?
- Ezeket át kell-e vinni az új rendszerbe? (adatmigráció)
- Milyen adatformátumokban érhetők el?

**Integrációk**
- Milyen más rendszerekkel kommunikál?
- Kik hívják az API-ját, vagy kinek az API-ját hívja?

**Technikai korlátok**
- Mik a teljesítmény elvárások? (válaszidő, egyidejű felhasználók)
- Vannak-e biztonsági követelmények?

---

## 5. Követelmények összesítése és prioritizálása

### User story-k megfogalmazása

Az interjúk eredményéből **user story-kat** kell írni. Ezek lesznek a TASKS.md UC-jainak alapjai.

**Sablon:**
```
Mint {felhasználó típusa},
szeretném {mit szeretne csinálni},
hogy {miért, mi az üzleti érték}.
```

**Példák:**
```
Mint ügyintéző,
szeretném látni az összes nyitott kérelmet egy helyen,
hogy ne kelljen több rendszerben keresnem.

Mint vezető,
szeretném exportálni a havi statisztikákat,
hogy a jelentéseimet gyorsan el tudjam készíteni.
```

### MoSCoW prioritizálás

Minden követelményt be kell sorolni:

| Kategória | Jelentés | Magyar |
|---|---|---|
| **Must have** | Nélküle nem lehet elindulni | Kötelező — MVP |
| **Should have** | Fontos, de van kerülő út | Fontos — v1.0 |
| **Could have** | Jó lenne, de várhat | Opcionális — v2.0 |
| **Won't have** | Tudatosan kihagyva | Nem scope |

### Ellentmondások kezelése

Ha különböző felhasználók vagy a PO és a felhasználók között ellentmondás van:

1. Dokumentáld az ellentmondást
2. Hozd össze az érintetteket, és döntsetek közösen
3. A döntést rögzítsd — ez is lehet ADR tárgya

Claude segíthet az ellentmondások azonosításában:

> „Az alábbi összesített interjú eredményekben azonosíts ellentmondásokat és hiányzó információkat."

---

## 6. Jóváhagyás és átadás

### Kimeneti dokumentum — Requirements.md

A feltárási fázis egyetlen kimeneti dokumentuma a `Requirements.md`, amelyet a tervezési fázis `SPEC.md`-je alapjául szolgál.

```markdown
# {Projekt neve} — Requirements.md

## Érintettek
{Ki vesz részt, milyen szerepben}

## Üzleti célok
{PO interjú eredménye — mit old meg, miért}

## Siker mérőszámai
{Mikor sikeres a projekt}

## User story-k (MoSCoW szerint)

### Must have
- Mint ..., szeretném ..., hogy ...

### Should have
- ...

### Could have
- ...

### Won't have (és miért)
- ...

## Fájdalompontok összesítése
{Közös témák az interjúkból}

## Nyitott kérdések
{Ami még nem tisztázott}

## Integrációs követelmények
{Más rendszerekkel való kapcsolat}

## Nem funkcionális követelmények
{Teljesítmény, biztonság, jogszabályi elvárások}
```

### Jóváhagyási folyamat

```
Fejlesztő elkészíti a Requirements.md-t
        ↓
PO átnézi és jóváhagyja az üzleti célokat
        ↓
Felhasználó képviselők átnézik a user story-kat
        ↓
Mindenki aláírja (vagy e-mailben jóváhagyja)
        ↓
Átadás a tervezési fázisnak (1. lépés)
```

> A jóváhagyott `Requirements.md` commitolva kerül a repository-ba, és a tervezési fázis során nem változik — ha új követelmény merül fel, azt változáskezelési folyamaton kell átvezetni.

---

## Ellenőrzőlista

```
Érintettek
[ ] Minden érintett csoport azonosítva
[ ] Legalább 3-5 felhasználói interjú elvégezve
[ ] PO / megrendelő interjú elvégezve
[ ] IT / Sysadmin korlátok felmérve

Dokumentáció
[ ] Requirements.md elkészült
[ ] User story-k MoSCoW szerint prioritizálva
[ ] Ellentmondások feloldva
[ ] Nyitott kérdések listája üres

Jóváhagyás
[ ] PO jóváhagyta az üzleti célokat
[ ] Felhasználók jóváhagyták a user story-kat
[ ] Requirements.md commitolva a repo-ba

Átadás
[ ] Requirements.md átadva a tervezési fázisnak (1. lépés)
[ ] Tervezési fázis megkezdéséhez szükséges összes információ rendelkezésre áll
```

---

## Kapcsolódó workflow lépések

```
→ 0. Követelmény feltárás (ez a dokumentum)
  1. Tervezés (SPEC.md, TASKS.md, copilot-instructions.md)
  2. Implementáció
  3. CI (build, teszt, kódminőség)
  4. CD (staging deploy, smoke test)
  5. OpenProject integráció
```

---

*Ez a dokumentum az FPH általános fejlesztési workflow nulladik lépésének leírása.  
A teljes workflow dokumentáció a demo projekt tapasztalatai alapján készül el.*
