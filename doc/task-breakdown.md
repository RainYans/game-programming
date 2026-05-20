# Task Breakdown

Granular, ordered backlog for Zombie Farm. Tasks are listed in build order — later tasks
depend on earlier ones. Tags: **[M]** must-have, **[S]** should-have, **[C]** could-have.

Vertical-slice target: plant → grow → harvest → deploy → battle → reward → save, all playable
end to end. Everything tagged **[M]** is part of that slice; do not start **[S]**/**[C]** work
until every **[M]** task above it is done and tested.

**Interaction model:** click-driven for v1 (no avatar). Farm actions (plant, harvest, open
shop, deploy) are written as input-agnostic operations; click input dispatches them. This
keeps the door open to add a controllable doctor-farmer character later without rewriting the
action logic.

---

## Milestone 0 — Project Setup

- [x] Create Unity 2022.3 LTS project, 2D URP template **[M]**
- [ ] Add Unity official `.gitignore`; init Git LFS for `.png .psd .fbx .wav .mp3` **[M]**
- [x] Set Transparency Sort Mode = Custom Axis `(0, 1, 0)` (Project Settings → Graphics) **[M]**
- [x] Enable New Input System **[M]**
- [ ] Commit empty configured project and push to `origin` **[M]**

## Milestone 1 — Map & Camera (Week 1)

- [x] Create Isometric Tilemap, paint a 10×10 diamond grid with placeholder tile **[M]**
- [x] `GridManager`: world ↔ cell coordinate conversion + cell occupancy lookup **[M]**
- [x] `CameraController`: mouse-drag pan, clamped to farm bounds **[M]**
- [x] `CameraController`: scroll-wheel zoom with min/max clamp **[M]**
- [x] Click → highlight the hovered cell **[S]**
- [ ] Verify isometric depth sorting: overlapping multi-tile objects draw correctly (named risk) **[M]**

## Milestone 2 — Farming Loop (Week 2)

- [ ] `CropData` ScriptableObject: id, display name, grow seconds, yield count, sprite **[M]**
- [ ] `CropInstance`: per-tile growth state machine driven by `DateTime.UtcNow` **[M]**
- [ ] `FarmActions`: input-agnostic action layer (plant / harvest / openShop / deploy) **[M]**
- [ ] `TileInteraction`: click empty tile → confirm plant → dispatch plant action **[M]**
- [ ] Growth visual: tile changes appearance as it ripens (seed → growing → ripe) **[M]**
- [ ] `TileInteraction`: click ripe tile → dispatch harvest action → free the cell **[M]**
- [ ] `Inventory`: item id → count, with add/remove + change event **[M]**
- [ ] On-screen inventory counter showing harvested zombies **[M]**
- [ ] Seed-pick popup / seed bank UI when planting **[S]**
- [ ] Add 2–3 engineered zombie strains as separate `CropData` assets **[S]**

## Milestone 3 — Combat Loop (Week 3)

- [ ] `ZombieData` / `UnitStats` ScriptableObject: hp, attack, speed (for both engineered & wild) **[M]**
- [ ] `MissionData` / `CityData` ScriptableObject: enemy roster, reward, one playable city **[M]**
- [ ] `BattleSimulator`: pure C# class, two unit lists → result + ordered event log **[M]**
- [ ] Unit tests for `BattleSimulator` (deterministic given a seed) **[S]**
- [ ] Mission map screen: pick up to 3 zombies from inventory → "Deploy" button **[M]**
- [ ] Battle scene: spawn placeholder engineered vs. wild zombies **[M]**
- [ ] `BattlePlayer`: replay the event log as timed animations **[M]**
- [ ] Mission result screen: win/lose + resource reward **[M]**
- [ ] Add 2–3 more city missions of rising difficulty **[S]**
- [ ] Strain-counter system: strain types strong/weak vs. wild types **[C]**

## Milestone 4 — Economy & Save (Week 4)

- [ ] Resource economy: battle reward → currency, spent at shop / on unlocks **[M]**
- [ ] Seed Shop panel: spend currency to buy seeds (dispatched via `FarmActions`) **[M]**
- [ ] `GameConfig` SO: all tunable numbers (grow times, costs, rewards, stats) in one place **[M]**
- [ ] `SaveManager`: `JsonUtility` to `Application.persistentDataPath` **[M]**
- [ ] Save on key events (harvest, mission end, purchase); load on launch **[M]**
- [ ] Full loop playable end to end from a fresh save **[M]**
- [ ] Lab panel: spend currency to unlock / upgrade engineered zombie strains **[S]**
- [ ] Plot expansion: spend currency to unlock additional farm cells **[S]**

## Milestone 5 — Polish & Demo (Week 5)

- [ ] Numerical balancing pass on `GameConfig` **[S]**
- [ ] Basic SFX (plant, harvest, deploy, win/lose) **[S]**
- [ ] Pause menu (ESC) **[C]**
- [ ] Settings menu (volume, etc.) **[C]**
- [ ] Tutorial / mission briefing **[C]**
- [ ] Mission statistics screen **[C]**
- [ ] Particle effects, ambient base sounds **[C]**
- [ ] Record gameplay video + write postmortem **[M]**

---

## Cut (do not build)

PvP · strain crossbreeding · per-city terrain effects · cinematic story scenes ·
achievements · cloud save · polished art.
