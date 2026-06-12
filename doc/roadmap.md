# Roadmap

> **🔧 Read this first — the plan vs. the final build (so it isn't misread).** This page is the
> **original four-week plan**. It deliberately uses hard **P0 / P1 / P2 tiers**: the contract was
> that the **P0 core ships solid even if later work has to be cut**. That is exactly what happened,
> and the gaps below are **intentional scope decisions, not abandoned or unfinished work**:
>
> - **P0 shipped and is solid** — the whole loop (farm → grow → deploy → raid → clear → reward →
>   spend) plays end-to-end. On top of it, several *could-have* items also landed: per-scene audio,
>   the Lab, a boss, and a **city-selection map** (City 1 is the one fully-built raid; cities 2–3 are
>   scaffolded as map nodes — for this demo one polished level is enough, with the rest planned next).
> - **A few P1 items were deliberately cut** to keep quality high rather than ship them half-built:
>   the **task system**, **plot expansion**, and the **branching** city map (the map is **linear**
>   instead — clear one city to unlock the next).
> - **Combat was redesigned, not dropped.** The planned **City → Stage → Prep / RTS-select**
>   structure was replaced — directly in response to playtest feedback — with a tighter **four-room
>   action-brawler** raid. That is an improvement driven by testing, logged in
>   [peer-feedback.md](peer-feedback.md).
>
> The game **as actually delivered** is documented in [evidence.md](evidence.md). Where this plan and
> the build differ, the three points above are the reason — by design.

A four-week plan to build the full game described in [vision.md](vision.md) and the
[design bible](design/) on top of the existing foundation. Time is the dominant constraint
(solo, on a tight timeline), so scope is split into tiers and the plan stays shippable
even if later work slips.

## Scope Tiers

| Tier | Meaning |
|------|---------|
| **P0** | **Must ship.** The MVP gate (see [vision.md](vision.md#definition-of-done-mvp-gate)). If everything else is cut, P0 alone is a complete, demoable game. |
| **P1** | **Target.** The full intended design. Built once the week's P0 work is done and tested. |
| **P2** | **Stretch / Icebox.** Nice-to-have depth. Only if time remains. See [Icebox](#icebox-p2). |

**Rule:** do not start P1 work in a milestone until that milestone's P0 work is done and
tested. Do not start P2 at all until the P0 MVP gate is met end-to-end.

## Already Built (Foundation)

These are implemented and merged, and the four-week plan builds on them:

> **Pivot note:** this lists the foundation as it stood at the *start* of the four-week plan. Two
> pieces were later replaced — the **isometric** view → **top-down** (see [vision.md](vision.md)),
> and the prototype **`BattleSimulator` / `BattlePlayer`** → the real-time
> **`BattleManager` / `BattleAgent`** combat. The rest still stand.

- Isometric tilemap + `GridManager`; `CameraController` (pan/zoom); tile interaction.
- `CropData` / `CropInstance` real-time growth; `FarmActions` (input-agnostic); `Inventory`.
- `BattleSimulator` (deterministic) + `BattlePlayer` (replay) — prototype combat.
- `Wallet`, Shop UI (`ShopController`/`ShopPanelUI`/`ItemStore`), `GameConfig`.
- `SaveManager` (JSON to `persistentDataPath`, autosave on change, fresh-save defaults).
- Building click-to-open placeholders; decorative ground/path tilemap.

## Milestones

Four one-week milestones (≈ Mon–Sun). Each maps to GitHub **Milestones**; the epic/story
breakdown is in [backlog.md](backlog.md).

---

### Milestone 1 — Avatar Farm & Zombie Foundation (Week of May 25)

Re-tool the farm around a controllable avatar and stand up the zombie data + hunger model.

- **P0**
  - WASD avatar movement (isometric) with camera follow.
  - Walk-up interaction: plant (seed-pick popup) and harvest near a plot.
  - Six `ZombieData` strains defined (stats + passive flags); 3 starting strains plantable.
  - Hunger state machine (Full ↔ Hungry) on zombies, with the farm-side idle timer.
  - Save/load extended to cover planted crops and zombie state.
- **P1**
  - Harvested zombies spawn as **roaming units** on the farm (not just inventory counts).
  - On-farm visual indicators for strain and hunger state.
- **Exit criteria:** walk the avatar, plant a chosen strain, harvest it, see it as a
  Full/Hungry unit; reload preserves the state.

---

### Milestone 2 — Economy, Buildings & Lab (Week of Jun 1)

Currency sinks and sources around the base.

- **P0**
  - Single currency fully wired (earn via a stub reward, spend in Shop).
  - Shop sells unlocked seeds + at least one combat item.
  - All economy numbers consolidated in `GameConfig`; save extended to new state.
- **P1**
  - Lab: spend currency to upgrade strain base stats.
  - Plot expansion: spend currency to unlock more plots.
  - Fertilizer + Hunger Tonic items in the Shop.
- **Exit criteria:** earn currency, buy a seed and an item, (P1) upgrade a strain / expand a
  plot — all persisted.

---

### Milestone 3 — Combat (Week of Jun 8) — heaviest milestone

The dedicated combat scene. This is the largest single piece of new work; expect P1 items
here to spill into Week 4 or down-tier.

- **P0**
  - Separate Battle scene; Deploy screen (pick a squad up to the cap).
  - City → 3 stages → prep-between-stages structure (first city).
  - Action controls: WASD-lead, left-select / right-command, auto-attack with passives.
  - At least one control item (Rotten Onion) placeable in the field.
  - Permadeath casualties; win/lose result; reward returned to the farm.
- **P1**
  - All three control items (Onion / Freeze / Barbed Wire).
  - Hunger-in-combat (eating kills → Full; Hunger Tonic spike).
  - Enemy variety; the full first city.
- **Exit criteria:** deploy a squad, clear 3 stages of City 1 with prep between, lose units
  permanently, win and bring a reward home.

---

### Milestone 4 — Progression, Art & Polish + Demo (Week of Jun 15)

Close the loop and make it presentable. Hit the MVP gate early in the week, then polish.

- **P0**
  - City map with City 1 selectable; minimal on-screen task/objective hints.
  - Integrate core art for the farm, avatar, and six strains.
  - Balancing pass on `GameConfig`.
  - Record a gameplay video + write the postmortem.
- **P1**
  - Task system with strain unlocks (Spitter / Shaman / Bomber).
  - Three-city branching map; cities 2–3 built from the City-1 template.
  - Core SFX set; HUD and panel polish.
- **Exit criteria (MVP gate):** the full loop is playable from a fresh save — plant → grow →
  harvest → deploy → clear City 1 → reward → spend → repeat — saved/loaded, and recorded.

---

## Risk Register

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Combat (M3) overruns | High | It's scoped as the heaviest week; P0 is deliberately thin (1 city, 3 stages, 1 item). P1 combat can slip without breaking the MVP. |
| Art volume exceeds time | High | Placeholders are acceptable through P0; integrate art per-system, most-seen assets first. |
| Scope creep | High | Hard P0/P1/P2 tiers; the MVP gate is the contract; P2 stays in the Icebox. |
| Isometric depth-sorting bugs with avatar + overlaps | Medium | Transparency Sort Axis already set; verify early in M1 with overlapping objects. |
| Save migration as state grows | Medium | Versioned save; extend `SaveData` additively; tolerate missing fields on load. |

## Icebox (P2)

Captured depth for after the MVP gate, time permitting — **not committed work**.

- **Items:** UV lamp, noise maker, pheromone spray, auto-turret, electric net, blood-bait
  (risk/reward), mystery serum (random effect), infection suppressant, EMP, decoy, drone,
  fuel-drum combos.
- **Combat:** manual per-strain "highlight" abilities, formations, richer enemy AI, stage
  checkpoints, per-city modifiers.
- **Progression:** more cities, deeper branching, an enemy/strain codex.
- **Farm:** walkable building interiors, NPC survivors, day/night cycle, crop tending
  (watering/pests), more strains beyond six.
- **Presentation:** particle "juice," ambient audio layers, settings/volume menu, save-slot
  management.
