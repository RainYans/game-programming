# Design — Progression

> **🔧 Build note (final submission).** This is the **planned** progression; the build **adjusted** it
> for scope. **Cut/changed:** the **task system** (no in-game tasks/achievements), **strain-unlock via
> tasks**, and the **branching** city map. What shipped: a **linear** city map (City 1 → Thornwood
> Hollow → Ashen Reach) with clear-to-unlock gating. See [evidence.md](../evidence.md).

Progression gives the player reasons to keep playing: a **city map** to conquer, a **task
system** that unlocks strains and rewards play, and the economic growth covered in
[economy.md](economy.md).

## City Map

- The world is shown as a **map of cities** with **branching, optional routes** — the player
  chooses which city to attack next rather than following a single line.
- The map also surfaces **light info about each city** (e.g., the kinds of wild monsters
  there, rough difficulty) so the player can choose informed.
- **v1 scope: three cities.** Build the **first city fully** as the template; cities 2–3
  reuse its scene structure with new layouts, enemies, and difficulty (much cheaper).
- A city node shows locked / available / cleared state. Clearing a city may unlock adjacent
  nodes on the branch.

Each city's internal structure (stages + prep) is defined in [combat.md](combat.md).

## Task System

An **achievement-style** task system: the game issues goals; completing them during normal
play grants rewards.

- **Task examples:** "Plant your first monster," "Harvest 5 monsters," "Win a combat stage,"
  "Upgrade a strain in the Lab," "Clear City 1."
- **Rewards:** currency and — importantly — **unlocking the 3 locked strains** (Spitter,
  Shaman, Bomber; see [zombies.md](zombies.md)).
- Tasks double as a **soft tutorial**, steering a new player through the loop (plant →
  harvest → shop → deploy → win) without an explicit tutorial mode.
- Implementation: a data-driven list of tasks (id, description, completion condition,
  reward), a tracker that listens to game events (planted, harvested, purchased, stage
  cleared, upgrade bought), and a small UI to view active/completed tasks. Progress is
  **persisted**.

## Unlock Flow

```
Play the loop → complete tasks → earn currency + unlock strains
   → buy/plant new strains → field deeper squads → take harder cities
   → clear cities → bigger rewards → fund more upgrades and expansion
```

## Difficulty Curve

- City 1: gentle — clearable with the 3 starting strains, teaches positioning and items.
- Cities 2–3: rising enemy count, tougher/varied wild monsters, rewarding the unlocked
  strains, upgrades, and item use.
- Numbers tuned in `GameConfig` during the balancing pass.

## Build Tiers (summary)

- **P0 (MVP gate):** one city playable end-to-end; a minimal set of tasks is optional for
  the gate.
- **P1 (target):** the task system with strain unlocks; the three-city branching map.
- **P2 (stretch):** more cities, deeper branching, per-city modifiers, a codex of enemy
  info. See [Icebox in roadmap.md](roadmap.md#icebox-p2).
