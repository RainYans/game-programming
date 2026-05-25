# Zombie Farm (working title)

Grow engineered zombies on a survivor base's farm, raise them to fighting strength, and
lead a squad of them to retake cities overrun by wild zombies. A casual base-builder +
squad-tactics game built in Unity (2022.3 LTS, URP 2D).

> **Status:** In active development. The foundation loop (isometric farm, camera, crop
> growth, inventory, battle simulator, shop, save/load) is playable. Currently building out
> the full design: a controllable avatar, six zombie strains with a hunger system, and a
> dedicated stage-based combat mode. See [Roadmap](doc/roadmap.md) for the four-week plan.

## How to Run

Open the `ZombieFarm/` project in Unity 2022.3 LTS and press Play. (No packaged build is
published yet; builds will be attached to Releases once available.)

## Controls

| Input | Action |
|-------|--------|
| **WASD** | Move the avatar (farm and combat) |
| **E / interact key** | Plant, harvest, open buildings (when near) |
| **Left click** | Select zombies (combat) / confirm in UI |
| **Right click** | Command selected zombies to move or attack (combat) |
| **Number keys** | Use a carried combat item (placed at the cursor) |
| **Mouse drag** | Box-select zombies (combat) |
| **Scroll wheel** | Zoom camera |
| **ESC** | Pause |

> Controls are still being implemented and may change as combat lands.

## Documentation

The design and plan live under [`doc/`](doc/):

- **[Vision](doc/vision.md)** — premise, pillars, core loop, MVP gate
- **[Design Bible](doc/design/)** — one document per system:
  - [Farm & Avatar](doc/design/farm.md)
  - [Zombies](doc/design/zombies.md)
  - [Economy & Buildings](doc/design/economy.md)
  - [Combat](doc/design/combat.md)
  - [Progression](doc/design/progression.md)
  - [Presentation](doc/design/presentation.md)
- **[Roadmap](doc/roadmap.md)** — four-week milestone plan, scope tiers (P0/P1/P2)
- **[Backlog](doc/backlog.md)** — epic → story → sub-issue breakdown for the Kanban board
- **[Next Action](doc/next-action.md)** — what's being worked on right now
- **[Process](doc/process.md)** — Git workflow and Unity technical setup
- **[Asset Credits](doc/asset-credits.md)** — sources and licenses
- **[Peer Feedback](doc/peer-feedback.md)** — feedback received and responses
- **[Testing](doc/testing/)** — testing logs

## Biggest Risk

Scope. This is a multi-system game built solo in four weeks. Mitigation: a clearly defined
MVP gate (see [Vision](doc/vision.md)) that stays shippable even if later scope slips, and a
roadmap split into must-ship (P0), target (P1), and stretch (P2) tiers.

## Unity Version

Unity 2022.3 LTS, URP 2D Renderer, New Input System.
