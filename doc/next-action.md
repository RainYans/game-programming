# Next Action

## Current Goal

**Milestone 1 — Avatar Farm & Zombie Foundation.** Re-tool the farm around a controllable
avatar and stand up the zombie data + hunger model. (See [roadmap.md](roadmap.md#milestone-1--avatar-farm--zombie-foundation-week-of-may-25).)

## Immediate P0 Tasks

1. **WASD avatar movement** (isometric) + camera follow + zoom clamp.
2. **Walk-up interaction:** plant near a plot (seed-pick popup) and harvest.
3. **Six `ZombieData` strains** defined (stats + passive flags); 3 starting strains plantable.
4. **Hunger state machine** (Full ↔ Hungry) on zombies with the farm-side idle timer.
5. **Extend save/load** to cover planted crops and zombie state.
6. **Set up Git LFS** before importing binary art.

## Then (P1, if M1 P0 is done & tested)

- Harvested zombies spawn as **roaming farm units**.
- On-farm visual indicators for strain and hunger state.

## Open Decisions to Resolve

- **Over-hunger downside:** starvation (lose HP) or frenzy (uncontrollable)? Needed before
  hunger combat balancing in M3. (See [zombies.md](design/zombies.md#open-question--downside-of-being-hungry).)
- Squad-size cap for deployment (~3–4, finalized against City-1 stage design).

## Status of the Foundation

The pre-existing loop (isometric farm, camera, crop growth, inventory, battle simulator,
shop, wallet, `GameConfig`, save/load, building placeholders) is implemented and merged.
The four-week plan builds the full design on top of it — see [roadmap.md](roadmap.md).

## Branch

Cut a fresh `feature/<story>` branch from `main` per story. (The repo is mid-transition from
the old milestone branches; the design/plan rewrite lives on its own branch.)
