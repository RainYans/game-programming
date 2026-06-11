# Design — Zombies

> ⚠️ **Now "Monsters" — see [direction.md](direction.md).** Theme pivoted to **Monster Farm**;
> these are **monsters** at the display layer (sprites from the **Cute Fantasy** pack,
> top-down pixel). **All strain ids, stats, passives, and data below are UNCHANGED** — only
> names/sprites change.

Engineered zombies are both the crop and the army. There are **six strains**, each defined
by base stats and a single **passive** ability, plus a shared **hunger** system that shifts
combat strength over time. Zombies that die in battle are **permanently lost**.

## Stats

Each strain has: **HP**, **Attack**, **Move Speed**, plus an **attack range** flag
(melee/ranged). Stats live on a `ZombieData` ScriptableObject (extends the foundation's
`ZombieData` / `UnitStats`). Exact numbers are tuned in `GameConfig`; the table below gives
the **design intent / relative profile**, not final values.

## The Six Strains

Passives are **simple and auto-triggered** (no manual activation in the base design). The
set covers six tactical roles:

| # | Strain (working name) | Role | HP | Attack | Speed | Range | Passive |
|---|------------------------|------|----|--------|-------|-------|---------|
| 1 | **Brute** | Tank / front line | High | Low | Slow | Melee | **Thick Hide** — flat damage reduction taken |
| 2 | **Mauler** | Main damage | Med | High | Med | Melee | **Bloodlust** — consecutive hits on the same target ramp damage |
| 3 | **Runner** | Skirmisher | Low | Med | Fast | Melee | **Evasion / first strike** — high dodge or strikes first on engage |
| 4 | **Spitter** | Ranged attrition | Low | Med | Med | Ranged | **Corrosion** — hits have a chance to lower the target's defense |
| 5 | **Shaman** | Support | Low | Low | Med | — | **Aura** — slowly heals / buffs nearby allied zombies |
| 6 | **Bomber** | Burst / trade | Med | Med | Med | Melee | **Self-Detonate** — explodes for area damage on death |

### Starting vs. Unlockable

- **Unlocked at start:** Brute, Mauler, Runner — a complete front line / damage / mobility
  trio, enough to clear the first city.
- **Unlocked via tasks:** Spitter, Shaman, Bomber — add ranged attrition, sustain, and burst
  as tactical depth. (Task system in [progression.md](progression.md).)

## Hunger System

Every zombie has one of two states: **Full** or **Hungry**.

- **Hungry = stronger** in combat. **Full = weaker** (a stat penalty).
- A freshly harvested zombie starts **Full** (and therefore weak).
- Left alone (not fighting, not fed) for a while, a zombie becomes **Hungry** over time.
- **Eating** makes a zombie Full again — and in battle, zombies eat the wild zombies they
  kill, so a unit that fights a lot drifts toward Full (weaker) over a raid.

### The resulting decision loop

> Harvest → wait for the zombie to get **Hungry** (strong) → deploy it at peak → during the
> raid it eats kills and drifts back to **Full** (weaker) → manage this with rest and items.

- The **Hunger Tonic** item (see [economy.md](economy.md)) re-applies the Hungry state mid-
  battle as an emergency power spike.
- This is the freshest mechanic in the design and the main source of pre-raid planning.

### Open question — downside of being Hungry

As written, Hungry is strictly better in combat, so a player could keep every zombie Hungry
forever with no cost. To create a real trade-off, **over-hunger** should carry a penalty.
Candidates (pick one during balancing):

- **Starvation:** a zombie kept Hungry too long slowly loses HP and can eventually die.
- **Frenzy:** an over-hungry zombie briefly becomes uncontrollable (ignores commands).

This is the one hunger detail still to confirm; everything else above is decided.

## Permadeath

Zombies that die in combat are **gone for good**. This makes deployment a genuine risk:
bringing your strongest, hungriest squad maximizes power but risks losing irreplaceable
units. Casualties are resolved by the combat system (see [combat.md](combat.md)).

## Upgrades

Strain **base stats can be improved** by spending currency at the **Lab**. Upgrades apply to
the strain (all zombies of that type), not to individual units. (See [economy.md](economy.md).)

## Implementation Notes

- Six `ZombieData` assets; passives can start as **flags/enums** read by the combat
  resolver rather than bespoke code per strain, to keep the build cheap.
- Hunger state is a small state machine on the zombie (farm-side timer + combat-side eating
  events) and must be **persisted** in the save file.
