# Design — Current Direction (Top-Down Pixel · Monster Farm)

> **Source of truth for the v2 pivot (2026-06-06).** Where any older doc (`vision.md`,
> `combat.md`, `presentation.md`, `zombies.md`, `farm.md`, …) conflicts
> with this file, **this file wins**. Those docs carry a banner pointing here. Their
> *mechanics/logic* remain valid unless contradicted below.

---

## 1. Locked decisions

1. **Theme = "Monster Farm" (怪物农场).** The post-apocalyptic *zombie* fiction is dropped.
   You grow **monsters** on a farm and lead a squad to clear **monster-infested villages**.
   - The 6 strain ids (`brute / mauler / runner / spitter / shaman / bomber`), the wild
     variants, `SaveData` fields, and all ScriptableObject assets are **UNCHANGED**.
   - "Zombie" → "Monster" is a **display-layer rename only** (UI names, sprites, copy).
     **Do not** rename ids, `SaveData` keys, SO file names, or C# types yet (that's churn /
     save-breaking). A later cosmetic pass can rename types if desired.

2. **View = top-down (orthographic), 2D pixel-art.** Was isometric. Square grid + Y-sort.

3. **Art = ONE cohesive ecosystem: *Cute Fantasy* by Kenmi (16px tiles / ~32px characters,
   bright top-down pixel).** Chosen over Ninja Adventure for its brighter, higher-detail look.
   The full Kenmi ecosystem is owned (same artist → cohesion guaranteed): main **Cute_Fantasy**
   pack + **Characters** (Goblins/Knights/Orcs/Angels) + **Dungeons** + **MilitaryCamp** +
   **Desert/Volcano/ShroomLands** biomes + **UI** + fonts. Monster roster: 15 slimes (3 sizes ×
   5 colours), 4 skeleton types, **Bombschroom** (+ Toxic-Gas VFX), goblins/orcs/knights,
   shroomlings, snails, biome enemies — plenty for strains + wild + bosses.
   - Packs used: `Cute_Fantasy` (main) + `Cute_Fantasy_Characters`, `Cute_Fantasy_Dungeons`,
     `Cute_Fantasy_UI`, `Cute_Fantasy_MilitaryCamp`, and the biome packs.
   - **Audio is the ONE cross-borrow:** Cute Fantasy ships NO audio, so BGM/SFX come from the
     free **Ninja Adventure** pack (audio only) — audio is style-agnostic.
   - **Do NOT mix Ninja Adventure visuals in** (16px-muted vs CF 32px-bright = resolution +
     palette clash). Visuals stay 100% Cute Fantasy.
   - Licenses: record Cute Fantasy (Kenmi, purchased) + Ninja Adventure (audio) in
     [asset-credits.md](../asset-credits.md).

4. **Combat = "clear-the-village" levels.** Was a linear `City → Stage → Prep` room sequence.
   Now: a **larger village map** seeded with monsters; **clear all the monsters to win**.
   Many levels, produced cheaply by reusing the village tileset + monster prefabs and varying
   layout / enemy mix / a boss. The real-time **squad-tactics action layer is KEPT**
   (WASD-lead + loose-follow + auto-attack + select/command + field items + permadeath).

5. **New gameplay is deferred until the art is in.** Once the reskin + view swap are done,
   add (in priority order): **boss levels** (the pack ships 20 bosses), **enemy variety**,
   a **watering/feeding care layer** (ties to the existing hunger system), then optional
   gathering/fishing, pets/decor, evolution/merge. Do **not** start these before the art lands.

---

## 2. Why this refactor is low-risk (codebase audit, 2026-06-06)

**All gameplay LOGIC is view-agnostic** — combat math & passives, crop growth timers, hunger,
economy/inventory, save/load, and every ScriptableObject require **zero changes**. Isometric
coupling is tiny and localized:

| Where | Change |
|---|---|
| `Assets/Scripts/AvatarController.cs` (~L19, L60) | `isoYScale = 0.5` → `1.0` (stop squashing Y movement) |
| `Assets/Scripts/BattleAgent.cs` (~L75, L274) | remove `IsoYScale` const + `dir.y *= IsoYScale` |
| `Assets/Scripts/GridManager.cs` + the scene `Grid` | `CellLayout.Isometric → Rectangle`, `cellSize (1,0.5,1) → (1,1,1)` |
| `Assets/Editor/BattleSceneSetup.cs` (~L164-183, L212-221) | replace `CellWorld()` iso projection + `IsoX/YAngle` wall angles with orthographic; sort order |
| `Assets/Editor/FarmMapSetup.cs` | iso diamond layout → rectangular field |
| Farm/Battle **Camera** Transparency Sort Axis `(0,1,0)` | **KEEP** — top-down also sorts by Y |
| Project pixel pipeline | PPU consistent (16 or a multiple), **FilterMode = Point**, add **2D Pixel Perfect Camera**, disable anti-aliasing |

→ The work is a **presentation swap + ~6 small code edits + scene rebuild + art reskin**, not
a gameplay rewrite. Old saves still load (crop/unit positions are grid-agnostic).

---

## 3. Art mapping (Cute Fantasy → our systems)

| System | Source |
|---|---|
| Farm ground / field / paths / water | `Cute_Fantasy/Tiles` (Grass / FarmLand / Cobble_Road / Water / Cliff / Cave) |
| Avatar (farmer / squad leader) | `Cute_Fantasy/Player` (modular base + `Tools` for farming + sword-attack frames) |
| 6 monster strains (display of brute/mauler/…) | **Assorted distinct species** (no longer limited to slime recolours — distinct silhouettes read better). Flexible casting, e.g. brute = Knight / Big-Slime, mauler = Orc, runner = Goblin Thief, spitter = Skeleton Mage, shaman = Angel, bomber = **Bombschroom** (gas VFX). Mix freely from Slimes / Goblins / Orcs / Knights / Skeletons / Shroomlings / Snails. Casting finalised during reskin. |
| Growth / evolution stages | **Slime Small → Medium → Big** = the 3 growth stages (or evolution tiers) — built in! |
| Wild enemies (clear-the-village) | `Cute_Fantasy_Characters` (Goblins / Orcs / Knights / Angels) + `Enemies/Skeleton` (normal/bow/mage/sword) |
| Bosses | Big slimes / `Volcano` + `Desert` enemies, scaled up |
| Buildings (Home / Shop / Lab / WarCamp) | `Cute_Fantasy/Buildings` (Houses / Unique_Buildings / Tent) |
| Combat scenes | `Cute_Fantasy_Dungeons` (Dungeon 1/2/3) + `Cute_Fantasy_MilitaryCamp` + biome tiles |
| VFX (gas / weather / impacts) | `Cute_Fantasy/Enemies/Bombschroom/Toxic_Gas_Cloud_VFX`, `Cute_Fantasy/Weather effects` |
| Full UI (HUD / panels / bars / icons / ribbons) | `Cute_Fantasy_UI/UI` (UI_ALL / Bars / Buttons / Frames / Icons / Pop_Up / Ribbons / Sliders) + fonts |
| Animals / crops / trees / decor | `Cute_Fantasy/Animals`, `Crops`, `Trees`, `Outdoor decoration` |
| SFX / music (ONLY cross-borrow) | **Ninja Adventure** `Audio/` (Cute Fantasy ships none) |

---

## 4. Refactor phases (fits ~2 weeks; new gameplay last)

| Phase | Work | Est. |
|---|---|---|
| **0 — Pipeline + spike** | Pixel pipeline (PPU/Point/Pixel-Perfect Camera) · import Ninja Adventure · a throwaway `StyleTest` scene with one monster + a tile patch + one UI panel, verified in top-down | 0.5 d |
| **1 — View flip** | the ~6 code edits · farm `Grid` → rectangular · battle layout → orthographic · camera/PPU · both scenes run with placeholder art | 2–3 d |
| **2 — Full reskin** | avatar · 6 strains · wild enemies · crop stages · buildings · ground · UI · FX · SFX, all from the pack | 3–4 d |
| **3 — Combat v2 + picks of new gameplay** | clear-the-village level(s) · a boss · enemy variety · watering layer · balance | 2 d |
| **4 — Polish / buffer** | bug-fix, transitions, record gameplay video | 2 d |

≈ 10 working days + ~4 days buffer.

---

## 5. Combat v2 — "clear-the-village" (replaces City→Stage→Prep)

- A **LEVEL = one village map** (a real scene/area), seeded with monsters.
- **Win = clear all monsters** (optionally: all monsters in the required zones).
- **Kept from the built combat:** WASD-lead the squad, loose-follow + auto-attack, left-click /
  box-select, right-click command (focus-fire / move), number-key field items, permadeath,
  a short **prep** beat before entering a level.
- **Many levels, cheap to make:** reuse the village tileset + monster prefabs; vary the map
  layout, the enemy mix, and cap each with a **boss** from `Actor/Boss`.
- The old `MissionData`/`Stage`/`Prep` structure still works and can back the new levels (a
  "village" can be one big stage); we re-skin and re-layout rather than rewrite the engine.

---

## 6. What is frozen (touch only via the display layer)

`SaveData` & `SaveManager`, all ScriptableObjects (`ZombieData`/`CropData`/`MissionData`/
`GameConfig`), combat math & passives in `BattleAgent`, crop growth, hunger model, wallet /
inventory / shop / lab logic. Reskin and re-layout around these; do not rewrite them.
