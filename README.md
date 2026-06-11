# Monster Farm

A casual **top-down 2D pixel** game built in Unity (2022.3 LTS, URP 2D): grow and raise a stable
of monsters on your farm, then **lead a squad of them into monster-infested villages and clear
them in real-time action combat.** Two connected modes — a calm farm-management loop and an
action-brawler raid — share one squad and one save.

> **Status:** Playable vertical slice. The farm loop (move, plant, harvest, shop, inventory,
> save/load) and the first raid (City1 — four linear combat rooms with a hero you control
> directly) are implemented and play end-to-end. See [Evidence](doc/evidence.md) for what
> changed and where to find it.

## How to Run

1. Open the **`MonsterFarm/`** project in **Unity 2022.3 LTS** (URP 2D, New Input System).
2. Open `Assets/Scenes/Farm.unity` and press **Play**.
3. Walk to the **War Camp** building and interact to deploy a squad into the raid (City1), or open
   `Assets/Scenes/Battle.unity` directly to jump into combat with a test squad.

Tuned for **1920×1080**. (No standalone build is attached yet — see the build-readiness notes in
[Evidence](doc/evidence.md).)

> **Art assets are not in this repo (licensing).** The **Cute Fantasy** pixel art (by Kenmi) is
> licensed, and its terms forbid redistributing the files, so the art is **kept local and not
> committed** here. To build from source, download the packs from
> [kenmi-art.itch.io/cute-fantasy-rpg](https://kenmi-art.itch.io/cute-fantasy-rpg) and import them
> into `MonsterFarm/Assets/Art/CuteFantasy/`. Fonts (Pixel Operator, Alagard) and SFX (Ninja
> Adventure) are CC0/free and **are** included. See [Asset Credits](doc/asset-credits.md).

## How to Play

**Goal.** On the farm, grow monsters and gear up your squad. In a raid, **lead your squad through
four areas and clear every enemy to reclaim the village.** You lose if your hero falls or the
whole squad is wiped.

### Farm (Farm.unity)
| Input | Action |
|-------|--------|
| **WASD** | Move your avatar |
| **E** | Plant / harvest / interact with a building when standing next to it |
| **Mouse** | Use the shop, seed picker, deploy screen, and other panels |

Buildings: **Shop** (buy monster seeds + combat items), **War Camp** (deploy a squad → raid),
**Home** (save). *(A Lab for strain upgrades is planned — not in this build.)*

### Raid / Battle (action-brawler)
| Input | Action |
|-------|--------|
| **WASD** | Move your hero (leader) |
| **Left Shift** | Dash |
| **Left click** | Hero melee swing toward the cursor (arc damage + knockback) |
| **Right click** | Command the **whole squad** — focus the enemy under the cursor, or move there |
| **1** | Rotten Onion — repel blast (click to aim/throw) |
| **2** | Freeze Canister — freeze enemies (click to aim/throw) |
| **Esc** | Pause |

Your monster squad auto-follows the hero and auto-engages nearby enemies; you reposition with the
hero, swing to fight alongside them, command the squad with right-click, and spend items when a
fight gets tough. Clearing an area opens the gate to the next; clear the final Village Square to win.

## Features

- **Farm loop** — WASD avatar, plant/harvest on a tile grid with cell highlight, crop growth,
  a monster **inventory**, a **shop** (seeds + items), and **versioned save/load**.
- **Six monster strains** — Brute, Mauler, Runner, Shaman, Spitter, Bomber — each with distinct
  stats and a unique passive (thick hide, bloodlust, evasion, corrosion, healing aura, self-detonate)
  plus a **hunger system** that trades attack power for vulnerability.
- **Action-brawler raid** — a directly-controlled hero with a melee swing + dash, an auto-fighting
  squad, throwable items, a four-room **linear level** (City1) with area-gated progression, a
  **minimap**, and a win/lose result screen.
- **Cohesive pixel art** — Cute Fantasy tileset with Y-sorted decor, animated monsters and water,
  and a parchment/pixel UI.

## Documentation

Design, process, and submission evidence live under [`doc/`](doc/):

- **[Reference & Creative Contribution](doc/reference-and-contribution.md)** — what inspired the
  game, the reference→transformation table, and what is my own
- **[Evidence index](doc/evidence.md)** — what changed since Session 1, mapped to scenes/scripts/commits
- **[Peer Feedback](doc/peer-feedback.md)** — feedback received and what I changed in response
- **[Asset Credits](doc/asset-credits.md)** — art/audio sources and licenses
- **[Vision](doc/vision.md)** · **[Design Bible](doc/design/)** · **[Roadmap](doc/roadmap.md)** ·
  **[Testing](doc/testing/)**

## My Contribution (solo)

This is a solo project. The **design and direction** are mine (concept, the farm + raid loop, the
six strains and their passives, hunger/permadeath, combat feel, level layout). The **gameplay
scripts** were written **with an AI coding assistant under my direction** — I decided what each
system needed, then reviewed, modified, and integrated the code, and can explain any of it (see
[Use of AI](doc/reference-and-contribution.md#use-of-ai)). I built the **scenes** (farm + the
four-room battle level) using my own `Assets/Editor/*Setup.cs` scaffolding tools plus manual layout,
implemented **player interaction** (planting, the action-brawler controls, squad command) and the
**UI/feedback** (HUD, panels, result screen), and did the **testing and post-feedback improvements**
(see [Evidence](doc/evidence.md) and [Peer Feedback](doc/peer-feedback.md)). External assets (art,
fonts, audio) are licensed and credited in [Asset Credits](doc/asset-credits.md).

## Unity Version

Unity **2022.3 LTS**, URP 2D Renderer, New Input System.
