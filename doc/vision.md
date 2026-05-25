# Vision

## Logline

Grow engineered zombies on a survivor base's farm, raise them to fighting strength,
and lead a squad of them to retake cities overrun by wild zombies.

## Setting

A zombie virus has swept the world. Most cities have fallen and humanity has retreated
into fortified bases. Scientists in one such base engineered a breakthrough:
**plantable zombies** — a domesticated, controllable strain bred from infected DNA. The
player runs the base's experimental farm. Each crop is a soldier; each reclaimed city is
one step toward taking the world back.

## Genre & Pivot

A **casual base-builder + squad-tactics** game with a light cartoon art style. This is a
deliberate step up in scope from the project's original "minimal vertical slice." The core
loops (grid, camera, crop growth, inventory, a battle simulator, shop, save) are already
implemented; this design layers real systems on top of that foundation:

- a **controllable avatar** that walks the farm (the original click-only model is retired),
- **six zombie strains** with stats, passive abilities, and a hunger mechanic,
- a **dedicated combat mode** with stage-based, squad-tactics gameplay (not the placeholder
  overlay used during prototyping).

## Design Pillars

1. **The farm is a place, not a menu.** You walk a character through the base to plant,
   harvest, shop, and deploy. Grown zombies roam the farm and become your army.
2. **Outsmart the horde, don't out-damage it.** Combat in an isometric view is won by
   control — repelling, freezing, slowing, and rerouting wild zombies — not by raw numbers.
3. **Every zombie is a risk.** Zombies are permanently lost if they die in battle, and a
   hunger system means a squad's strength shifts over time. Deployment is a real decision.
4. **Casual to hold, deep to master.** Easy moment-to-moment input (WASD + mouse); the
   depth lives in preparation: which strains to raise, when they're hungriest, what to bring.

## Core Loop

```
Plant seed → grow → harvest → zombie roams the farm
   → let it get hungry (stronger) → deploy a squad to a city
   → clear stage-by-stage (control the horde, mind casualties)
   → earn currency → buy seeds / items, upgrade strains, expand plots
   → unlock new strains via tasks → take the next city
```

## Target Experience

A relaxed but thoughtful session: tend the farm, plan a raid, watch a tense stage play out
where good positioning saves your squad, come home richer, and invest in the next push.

## Definition of Done (MVP Gate)

If time runs short, the game is still shippable when this end-to-end slice works:

- Walk the avatar; plant and harvest at least one strain.
- At least **3 starting strains**, each visibly different in battle.
- Hunger affects combat strength.
- A shop that sells seeds and at least one combat item; a single currency.
- **One city** with a short sequence of combat stages and a between-stage prep phase.
- Squad deployment, control-item usage, permadeath, a win/lose result, and a reward.
- Save and load the whole state.

Everything beyond this gate (lab upgrades, plot expansion, all three items, the full task
system, cities 2–3, the branching map, art and audio polish) is **target** or **stretch**
scope — see [roadmap.md](roadmap.md).

## Audience & Purpose

Solo developer; a course deliverable built over four weeks alongside an internship. Quality
bar: a polished, readable, **casual** game — cartoon art, light audio, clean UI. Not AAA
fidelity, but cohesive and pleasant to play.
