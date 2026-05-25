# Design — Presentation

The game is **casual** in feel; the presentation should match: approachable, readable,
pleasant — not grim survival-horror, and not AAA fidelity.

## Art Direction

- **Style:** light **cartoon**. Friendly, slightly stylized engineered zombies; readable
  silhouettes so the six strains are distinguishable at a glance; clear, non-grim ruined
  cities.
- **View:** 2D **isometric** throughout. Art must respect the Transparency Sort Axis
  `(0, 1, 0)` so overlapping objects depth-sort correctly.
- **Readability first:** strain identity and **hunger state** (Full vs. Hungry) must read
  instantly on the farm and in combat — via color, icon, or a small status indicator.

## Asset Pipeline

- **Source:** art is produced by the developer using **AI generation** plus sourced online
  assets, then handed to the implementer for integration. The dev provides finished assets;
  engineering wires them into prefabs, tilemaps, and animations.
- **Every scene needs art:** the farm base, building exteriors, the avatar, six zombie
  strains, wild-zombie enemies, combat items/effects, the city map, and combat
  stages/backgrounds. This is a large surface and a real schedule risk (see
  [roadmap.md](roadmap.md)).
- **Coverage strategy:** placeholder shapes remain acceptable until a system is functional;
  art is integrated per-system as each lands, prioritizing the farm, the avatar, and the six
  strains (the most-seen assets) first.
- Every imported asset is logged in [asset-credits.md](../asset-credits.md) with source and
  license.

## Animation

- **Minimum viable:** avatar walk; zombie idle/walk/attack/death; basic crop growth-stage
  swaps; item effects (freeze, repel, slow).
- Attack/death animations matter most in combat (they carry the satisfaction). Crop and farm
  animation can stay simple.

## Audio

- **Style:** **casual / light** — gentle ambient base music, soft UI clicks, satisfying but
  non-violent combat cues.
- **Minimum SFX set:** plant, harvest, purchase, deploy, item use, hit, zombie death,
  win/lose.
- Audio is a late pass (polish week); the game must be fully playable silent first.

## UI / UX

- **Bar:** clean and good-looking, **not** required to rival AAA — just polished and clear.
- **Core HUD:** currency, current squad / hunger states, active task hint.
- **Panels:** seed-pick popup, Shop, Lab, Deploy/city-map, combat prep, results.
- Consistent cartoon styling, legible typography (TextMesh Pro), clear affordances for the
  walk-up-and-interact model.

## Build Tiers (summary)

- **P0 (MVP gate):** placeholder-or-better art is fine; the game must be **playable and
  readable**, silent allowed.
- **P1 (target):** cartoon art integrated across the main scenes; the core SFX set; a clean
  HUD and panels.
- **P2 (stretch):** particle "juice," ambient layers, richer animations, settings/volume,
  menu polish. See [Icebox in roadmap.md](roadmap.md#icebox-p2).
