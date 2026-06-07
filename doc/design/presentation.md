# Design — Presentation

> ⚠️ **Direction updated (2026-06-06) — see [direction.md](direction.md).** Now **top-down
> pixel-art**, theme **"Monster Farm"**, art = the single **Ninja Adventure** pack.
> "isometric / cartoon / AI-generated" wording below is superseded; **"zombie" = "monster"
> (display only)**. Logic stays valid unless it conflicts with `direction.md`.

The game is **casual** in feel; the presentation should match: approachable, readable,
pleasant — not grim survival-horror, and not AAA fidelity.

## Art Direction

- **Style:** **16-bit pixel-art** from one cohesive pack (**Ninja Adventure**). Friendly,
  readable monster silhouettes so the six strains are distinguishable at a glance.
- **View:** 2D **top-down (orthographic)** throughout; Y-sort via Transparency Sort Axis
  `(0, 1, 0)`. Pixel-art settings: **FilterMode = Point**, consistent PPU, **2D Pixel Perfect
  Camera**, no anti-aliasing.
- **Readability first:** strain identity and **hunger state** (Full vs. Hungry) must read
  instantly on the farm and in combat — via color, icon, or a small status indicator.

## Asset Pipeline

> The concrete production plan — art-direction decision, full asset manifest, animation specs,
> AI workflow, prompt library, and naming/integration conventions — lives in
> [art-pipeline.md](art-pipeline.md). This section is the high-level direction only.

- **Source:** a single cohesive pack — **Ninja Adventure** (16px top-down pixel: tiles,
  characters, monsters, 20 bosses, FX, items, UI, audio). Engineering wires the pack's assets
  into prefabs, tilemaps, and animations. (The earlier AI-generation + Kenney plan is dropped.)
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
