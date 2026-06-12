# Backlog & Issue Conventions

> **🔧 Note.** This is the planning backlog. Some P1 stories were **cut/adjusted** in the shipped build
> — the **task system**, **plot expansion**, the **branching** map, and **box-select** combat (combat
> shipped as a four-room action-brawler). See [evidence.md](evidence.md) for what actually shipped.

How work is broken down for the GitHub Project (Kanban) board, and the current backlog. The
breakdown follows standard Agile sizing: a **Story** is a vertical slice (≈1–3 days) that
delivers observable value; sub-issues are the technical tasks inside a story.

> Issues, sub-issues, and the board are managed in the **GitHub web UI** (Issues + Projects).
> This file is the source list to populate it from.

## Hierarchy → GitHub mapping

| Layer | GitHub feature | What it is | Granularity |
|-------|----------------|------------|-------------|
| **Milestone** | Milestone | A time-box (one of the four weeks). | 1 week |
| **Epic** | Issue with type `Feature` | A whole system. Stays light; a tracking parent for its stories. | Multi-week |
| **Story** | Issue (child of the epic via **sub-issue** link) | A vertical slice the player can observe. Written from user value where natural. | **1–3 days** |
| **Task** | **Sub-issue** of the story | A technical step inside a story (UI / logic / data / wiring). Dev-defined. | Hours |

Two to four levels, no deeper. Keep epics as lightweight placeholders and only break a story
into sub-issues when it genuinely has several technical parts — otherwise the story is the
unit of work.

## Granularity rules (so issues are neither too small nor too big)

- A **Story** should: deliver a demoable change end-to-end (a **vertical slice**, not "the
  UI" or "the database" alone), fit in **1–3 days**, and be testable. (INVEST.)
- If a candidate issue would take **> ~3 days** or has several independent parts → it's an
  **Epic** or needs splitting into stories.
- If a candidate issue is "create a script / add a field / make a prefab" with no standalone
  player value → it's a **sub-issue (task)**, not a story.
- Slicing by architectural layer (separate issues for UI / logic / data) is an **anti-
  pattern** at the story level — do that with sub-issues inside one story instead.

## Labels & issue types

- **Type** (GitHub issue type): `Feature` (epic), `Story`, `Task`, `Bug`.
- **Priority:** `P0`, `P1`, `P2` (matches [roadmap.md](roadmap.md#scope-tiers)).
- **System:** `farm`, `zombies`, `economy`, `combat`, `progression`, `presentation`, `tech`.
- **Status** is the board column, not a label.

## Kanban columns

`Backlog` → `Ready` (refined, P0/P1 for the current milestone) → `In Progress` →
`In Review` (PR open / self-review) → `Done`.

- **Definition of Ready:** scoped to a vertical slice, acceptance criteria written, fits the
  milestone, dependencies known.
- **Definition of Done:** implemented, manually verified in-editor, tested where practical,
  merged to `main`, and (for player-facing work) the relevant exit criterion in the
  roadmap is satisfied. The chunk's pre-agreed **M / T / P tier** (see
  [process.md](process.md#definition-of-done--per-chunk-tiers)) must also be met.

---

## The Backlog

Stories are tagged `[tier · milestone]`. Sub-issues (tasks) are nested where a story needs
breaking down. Stories without listed sub-issues are small enough to take as-is.

### EPIC: Farm & Avatar (`farm`)

- **Walk the avatar around the farm** `[P0 · M1]`
  - Movement input action map (New Input System)
  - Isometric movement + camera follow with zoom clamp
  - Collision against plots and buildings
- **Plant a chosen seed by walking up to a plot** `[P0 · M1]`
  - Proximity detection + interact prompt
  - Seed-pick popup listing owned seeds
  - Dispatch plant via `FarmActions`; growth starts
- **Harvest a ripe plot by interacting** `[P0 · M1]`
- **Harvested zombies spawn as roaming farm units** `[P1 · M1]`
  - Roamer prefab + idle wander behaviour
  - Spawn on harvest; reconcile with `Inventory`
- **Verify isometric depth sorting with avatar + overlapping objects** `[P0 · M1]` (spike)

### EPIC: Zombies & Hunger (`zombies`)

- **Define six strains as `ZombieData` (stats + passive flags)** `[P0 · M1]`
- **Zombies drift Full → Hungry over idle time** `[P0 · M1]`
- **Persist strain + hunger state in the save** `[P0 · M1]`
- **Hunger changes combat stats (Hungry stronger / Full weaker)** `[P1 · M3]`
- **Implement the six passive abilities** `[P1 · M3]`
  - one sub-issue per passive (Thick Hide, Bloodlust, Evasion, Corrosion, Aura, Self-Detonate)
- **Over-hunger penalty (starvation or frenzy)** `[P1 · M3]` (resolve open question first)

### EPIC: Economy & Buildings (`economy`)

- **Earn currency from a completed stage/reward** `[P0 · M2]`
- **Buy unlocked seeds in the Shop** `[P0 · M2]` (extend existing Shop)
- **Buy combat items in the Shop** `[P0 · M2]`
- **Consolidate all economy numbers in `GameConfig`** `[P0 · M2]` (chore)
- **Upgrade a strain's stats at the Lab** `[P1 · M2]`
  - Lab panel UI; upgrade tiers + costs in `GameConfig`; apply per-strain; persist
- **Expand plots with currency** `[P1 · M2]`

### EPIC: Combat (`combat`)

- **Deploy screen: pick a squad (capped)** `[P0 · M3]`
  - Squad-pick UI from roaming zombies; cap enforcement; hand selection to the battle scene
- **Battle scene with City → Stages → Prep structure** `[P0 · M3]`
  - Scene + stage manager; prep-phase UI (enemy preview, squad/formation, hunger choices);
    stage transitions
- **Lead the squad with WASD; zombies auto-attack** `[P0 · M3]`
  - Leader movement; squad follow; target acquisition + auto-attack; passive triggers
- **Select and command zombies with the mouse** `[P0 · M3]`
  - Click / box select; right-click move-or-attack; selection visuals
- **Place a control item at a target area** `[P0 · M3]`
  - Targeting/placement; Rotten Onion repel; (P1) Freeze Canister, Barbed Wire
- **Permadeath: dead zombies removed from the roster** `[P0 · M3]`
- **Win/lose result + reward returned to the farm** `[P0 · M3]`
- **Hunger-in-combat: eating kills → Full; Hunger Tonic spike** `[P1 · M3]`
- **Wild-zombie enemy variety + full first city** `[P1 · M3]`

### EPIC: Progression (`progression`)

- **City-selection map with City 1 selectable** `[P0 · M4]`
- **Task system (data-driven tasks, event tracker, UI)** `[P1 · M4]`
  - Task data model; event-driven tracker; task list UI; reward grant
- **Unlock strains via tasks** `[P1 · M4]`
- **Three-city branching map + cities 2–3** `[P1 · M4]`
- **Show city info on the map** `[P1 · M4]`

### EPIC: Presentation (`presentation`)

- **Integrate avatar art + walk animation** `[P0 · M4]`
- **Integrate six strain sprites (idle / walk / attack / death)** `[P0 · M4]`
- **Integrate farm base + building art** `[P1 · M4]`
- **Integrate combat backgrounds + item effects** `[P1 · M4]`
- **Core SFX set (plant, harvest, buy, deploy, item, hit, death, win/lose)** `[P1 · M4]`
- **HUD + panel visual polish** `[P1 · M4]`

### EPIC: Tech & Release (`tech`)

- **Set up Git LFS for binary art/audio assets** `[P0 · M1]` — **skipped** (2D art is small;
  not worth the overhead — revisit only if binaries grow large)
- **Extend `SaveData` additively + version it as state grows** `[P0 · ongoing]` (chore)
- **Per-milestone testing log under `doc/testing/`** `[P0 · ongoing]` (chore)
- **Balancing pass on `GameConfig`** `[P0 · M4]`
- **Record gameplay video + write postmortem** `[P0 · M4]`
