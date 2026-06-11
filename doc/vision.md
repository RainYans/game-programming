# Vision

> ⚠️ **Direction updated (2026-06-06) — see [design/direction.md](design/direction.md).**
> Pivoted to **top-down pixel-art**, theme **"Monster Farm"**, art from the single
> **Cute Fantasy** pack, and **clear-the-village** combat. The older "isometric / zombie"
> wording below is superseded; **"zombie" now means "monster" at the display layer (ids
> unchanged)**. Mechanics/logic stay valid unless they conflict with `direction.md`.

## Logline

Grow **monsters** on your farm, raise them to fighting strength, and lead a squad of them to
**clear monster-infested villages**.

## Story & World

Long ago, monsters and people shared the **Verdant Vale** in balance — farmers even grew
**seed-monsters**: gentle creatures sprouted from spores and raised like livestock to guard the
fields. Then came the **Wildbloom**, a creeping spore-corruption drifting out of the deep
forest. It turned free monsters feral and swallowed the outer villages one by one, until the
people fell back to the last safe town.

You are the Vale's last **Monster Rancher**, heir to the half-forgotten seed-monster craft.
From your farm at the town's edge you revive the old art — **plant monster-seeds, raise them to
strength, and lead a small squad to purge the Wildbloom village by village**, taming the land
back. At the heart of the worst-hit regions wait **Wildbloom-corrupted great beasts** (dragons,
tengu, giant slimes…) — the boss of each region. Reclaim every village, reach the **source of
the Wildbloom**, and restore the Vale's balance.

**The fiction carries the mechanics:**
- **Plant & grow** — a monster-seed sprouts through three stages into one of your fighters.
- **Hunger** — a freshly-fed monster is docile and *weaker*; let it grow **hungry and it turns
  fierce** (stronger in battle), so a long raid slowly wears down your edge.
- **Permadeath** — these are creatures you raised by hand; one lost in the field is a real loss.
- **Clear-the-village** — each village is choked with feral monsters; clear them all to reclaim
  it, earn resources, and unlock new seeds. Regions cap with a **boss**.

Tone: warm, hopeful, **cozy-with-stakes** — a friendly rancher's adventure, *not* horror.
(Implementation note: the 6 strain ids, stats, passives, hunger, and save data are unchanged
from the original "engineered zombie" build — this is a theme/skin layer only.)

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
2. **Outsmart the horde, don't out-damage it.** Combat in a **top-down view** is won by
   control — repelling, freezing, slowing, and rerouting wild monsters — not by raw numbers.
3. **Every zombie is a risk.** Zombies are permanently lost if they die in battle, and a
   hunger system means a squad's strength shifts over time. Deployment is a real decision.
4. **Casual to hold, deep to master.** Easy moment-to-moment input (WASD + mouse); the
   depth lives in preparation: which strains to raise, when they're hungriest, what to bring.

## Core Loop

```
Plant seed → grow → harvest → monster roams the farm
   → raise/feed it for battle → deploy a squad to a village
   → clear the village of all monsters (control the horde, mind casualties)
   → earn currency → buy seeds / items, upgrade strains, expand plots
   → unlock new monsters via tasks → take the next village
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
- **One city** played as a short sequence of **area-gated combat rooms** (clear an area to open the gate to the next).
- Squad deployment, control-item usage, permadeath, a win/lose result, and a reward.
- Save and load the whole state.

Everything beyond this gate (lab upgrades, plot expansion, all three items, the full task
system, cities 2–3, the branching map, art and audio polish) is **target** or **stretch**
scope — see [roadmap.md](roadmap.md).

## Audience & Purpose

Solo developer; a course deliverable built over four weeks alongside an internship. Quality
bar: a polished, readable, **casual** game — cartoon art, light audio, clean UI. Not AAA
fidelity, but cohesive and pleasant to play.
