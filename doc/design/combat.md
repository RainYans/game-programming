# Design — Combat

> ⚠️ **Structure updated (2026-06-06) — see [direction.md](direction.md).** Combat is now
> **"clear-the-village"**: a larger **top-down** village map seeded with monsters — clear them
> all to win; **many levels**. The **real-time squad-tactics action layer described below is
> KEPT** (WASD-lead, loose-follow, auto-attack, select/command, field items, permadeath). The
> old `City → Stage → Prep` structure can still back a level. "zombie" = "monster" (display).

Combat is a **dedicated mode** (a separate scene), not the placeholder overlay used during
prototyping. It's a casual **squad-tactics / stage-clear** experience: the player leads a
small squad of their farm zombies through a fallen city, stage by stage, to reclaim it.
The design goal is **high agency through positioning and control**, not micro-management.

Reference points: **Pikmin** (lead a squad with a controllable leader, send detachments),
**Bad North** (minimal isometric squad tactics with permadeath), and the **pre-engagement
prep** beat of tactical games like Ready or Not — borrowing the "set up before you breach"
layer, **not** their micro depth.

## Structure: City → Stages → Prep

- A **City** is a full level — one large "scene" / raid.
- A city is a **sequence of combat stages** (small encounters / sub-areas).
- Between stages there is a **Prep phase** (regroup) before entering the next stage.
- Clear all stages → the city is reclaimed → large reward.

```
City (level)
 ├─ Prep ─ Stage 1 ─ Prep ─ Stage 2 ─ Prep ─ Stage 3 ... ─ City reclaimed
```

For v1, the **first city is built fully** as the template; later cities reuse its scene
structure and assets with new layouts/enemies (cheaper to produce). Target stage count for
city 1: **3–4 stages** (linear path).

## Prep Phase (the strategy layer)

Quiet, non-real-time. Before each stage the player can:

- See a **preview of the upcoming enemies** (which wild zombies, roughly how many).
- **Choose the squad / formation** for this stage (within a squad-size cap — TBD, ~3–4 to
  start, finalized against level design).
- Make **hunger trade-offs**: feed a zombie to refill (safer but weaker) or keep it Hungry
  (stronger but risks over-hunger); decide whether to use a Hunger Tonic.
- This is where the game's "thinking" happens — every meaningful decision is here.

## Action Phase (the real-time layer)

The player controls the **avatar (leader)** and a squad of zombies.

### Controls

- **WASD** — move the leader; the camera follows. **Zombies loosely follow the leader by
  default** and **auto-attack** enemies in range (passives trigger automatically). Doing
  nothing still results in the squad tagging along and fighting — no forced micro.
- **Left click / drag-box** — select one or a group of zombies.
- **Right click** — command the selection: click an enemy = attack it; click ground = move
  there. (This avoids overloading left-click with multiple meanings.)
- **Number key → left click** — use a carried **combat item**, placed at the cursor area
  (items do **not** require selecting zombies).

So the leader's positioning is itself a tactic (lead the squad into or out of a fight),
while the mouse is for precise detachment and focus-fire.

### Why this is fun

Strategy is front-loaded into Prep (who to bring, hunger state, permadeath risk). The action
layer only needs to feel responsive and readable — repel/freeze/reroute the horde, focus a
dangerous target, pull back a wounded unit. It is intentionally **not** a hardcore RTS.

## Field-Control Items (in combat)

The three v1 items (full details in [economy.md](economy.md)) are the heart of the action
layer — they **change the situation** rather than just dealing damage:

- **Rotten Onion** — repels/scatters wild zombies away from an area.
- **Freeze Canister** — freezes wild zombies in an area.
- **Barbed Wire** — slows and reroutes the horde.

The leader carries item(s) into a stage; using one is a timed, positional decision.

## Hunger in Combat

- Zombies **eat the wild zombies they kill**, drifting from Hungry (strong) toward Full
  (weaker) over a stage — so a long fight naturally wears down your edge.
- **Hunger Tonic** snaps the squad back to Hungry (strong) as an emergency spike.
- See [zombies.md](zombies.md) for the full hunger model and the over-hunger open question.

## Casualties (Permadeath)

Zombies that die in a stage are **permanently lost** — removed from the farm roster. This is
the core risk: a hungry, upgraded squad clears stages fast but a wipe is a real setback.
No mid-battle revives.

## Result & Reward

- **Win** (clear all stages): reclaim the city, large currency reward, may satisfy tasks
  and unlock the next city node.
- **Lose** (squad wiped or retreat): keep any partial progress per design (TBD — likely
  no stage checkpoints in v1; a lost raid is a lost raid), surviving zombies return home.

## Relationship to the Foundation

The prototype's `BattleSimulator` (deterministic, two unit lists → event log) and
`BattlePlayer` (replays the log) were a **simulation/replay** model. The new design is
**interactive real-time**. The deterministic simulator may be retained for AI/auto-resolve
or unit tests, but the playable combat is the real-time squad-tactics scene described here.
This is the largest single piece of new work — see the schedule in [roadmap.md](roadmap.md).

## Build Tiers (summary)

- **P0 (MVP gate):** one city, 3 stages, prep phase, WASD-lead + select/command +
  auto-attack, at least one control item, permadeath, result + reward.
- **P1 (target):** all three items, full hunger-in-combat, 3-strain+ squads, polished
  enemy variety, the full first city.
- **P2 (stretch):** cities 2–3 with branching, manual "highlight" abilities per strain,
  richer enemy AI, extra items.
