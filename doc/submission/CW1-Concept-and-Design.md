# Coursework 1 — Game Concept and Design

**Student:** Yanshuo Liu  ·  **Student ID:** 2617486
**Module:** Games Programming  ·  **Engine:** Unity 2022.3 LTS (URP 2D, New Input System)
**Repository:** https://github.com/RainYans/game-programming

---

## 1. Game title

**Monster Farm** — a top-down 2D pixel game.

## 2. One-sentence game idea

Grow monsters on your farm like crops, raise them to fighting strength, then lead a squad of them
into monster-infested villages and clear them in real-time action combat.

## 3. Intended player experience

A relaxed-but-thoughtful loop with two connected moods that share one squad and one save:

- On the **farm** the player feels calm and in control — walking a little character around,
  planting and harvesting monsters, shopping, and deciding who to take to war.
- In a **raid** the mood flips to tense and active — the player is an on-screen fighter, not a
  spectator of an auto-battle, repositioning and spending items to keep a squad alive.

The emotional hook is **attachment with stakes**: the monsters you patiently raise are the exact
units you can lose forever (permadeath), so every deployment is a genuine decision. The target feel
is "cozy with stakes" — a friendly rancher's adventure, not horror.

## 4. Core mechanic

**Grow-then-fight, gated by a hunger risk.** A monster-seed sprouts through growth stages into a
fighter that roams the farm. Each monster has a **hunger state** that trades power for fragility,
and dies **permanently** if it falls in battle. The single tying mechanic is therefore not "farming"
or "fighting" in isolation, but **preparing a squad you are willing to risk** and then fighting
beside it.

## 5. What the player does moment to moment

- **On the farm:** move with **WASD**; press **E** at a tilled plot to plant a monster, and again
  when it is ripe to harvest it; walk up to buildings (Shop / Lab / War Camp / Home) and press **E**
  to open them; use the mouse in panels; press **Esc** for options.
- **In a raid:** lead the hero with **WASD** and **dash** with Shift; **left-click** for a melee
  swing toward the cursor; **right-click** to command the whole squad to focus an enemy or move; press
  **1 / 2** to throw field-control items. The squad auto-follows and auto-engages, so the player is
  never forced into twitch micro — control and positioning win fights, not raw clicking speed.

## 6. Target player

Casual-to-mid players who enjoy **cozy base-building and light tactics** — fans of farm/management
loops who also want an active payoff. The control scheme (WASD + mouse, auto-fighting squad) is
**easy to hold**; the depth sits in preparation (which strains to raise, when they are hungriest,
what to bring), so the game is **casual to pick up but rewards thinking**. Age-appropriate and
non-graphic.

## 7. Reference games or inspirations

- **Zombie Farm** (the casual "grow zombies on a farm" game) — my **one game reference**, used only
  for the core fantasy of *growing your own fighters on a farm like crops*.
- **Broadly familiar genre conventions** — farm-management loops, and action games with a hero plus
  helpers — used as general grammar rather than copying any specific title.
- **Cute Fantasy RPG** art set by Kenmi — the licensed visual style the world is built from.

## 8. What is original or creative about the idea

- In most farming games you grow crops to **sell**; here you grow monsters to **fight**, and you pick
  up a sword and fight **beside** them.
- The hook no other game on this art set has: the monsters you spend real time raising are the exact
  ones you can **lose forever**. Because you are risking units you are attached to, **caring about
  your monsters becomes a mechanic, not just flavour**.
- The **hunger system** makes squad strength shift over time, turning "when do I deploy?" into a real
  decision rather than a stat check.
- The game deliberately fuses two genres (cozy farm-sim + hands-on action raid) over **one shared
  squad and save**, so the calm half feeds the tense half.

## 9. Vertical slice plan

The vertical slice is the **whole core loop playable end-to-end from a fresh save**:

> Plant a monster → it grows → harvest it (it roams the farm) → buy seeds/items at the Shop → deploy
> a squad at the War Camp → raid one village and clear it through area-gated combat → win, earn
> resources → spend them (Shop / Lab) → repeat — with the entire state saved and loaded.

One **fully built village raid** (City 1) is enough to convey the finished experience; further
cities reuse the same systems and are cheaper content. This keeps the slice small but complete rather
than a set of disconnected experiments.

## 10. Must-have / should-have / could-have / cut-first (MoSCoW)

| Tier | Features |
|---|---|
| **Must-have** | WASD avatar; plant/harvest on a tile grid; monster roster; deploy → raid; real-time hero + squad combat; area-clear → gate → win/lose; single currency; **save/load**; clear controls + goal. |
| **Should-have** | Six monster strains with unique passives; the **hunger** system; a Shop (seeds + items); a themed multi-room level; HUD + minimap + result screen; cohesive art + animation; first-launch onboarding + a dedicated combat tutorial. |
| **Could-have** | A city-selection map with unlock gating; a **boss**; audio (per-scene music + SFX + volume sliders); a **Lab** for permanent strain upgrades; a **Bestiary** codex; **key rebinding**. |
| **Cut-first** (drop to protect the slice) | Branching city map → made **linear**; **task system** & **plot expansion**; some combat items (Barbed Wire) and consumables (Fertilizer, Hunger Tonic); RTS-style box-select micro; open-world farming / seasons. |

The contract is explicit: **the Must-have core ships solid even if later tiers are cut.** That makes
the project shippable at any point and avoids a large idea that can't be delivered.

## 11. Unity development plan

- **Engine / pipeline:** Unity 2022.3 LTS, **URP 2D Renderer** (2D lights), **New Input System** for
  both farm and combat.
- **View:** top-down orthographic, square Tilemap with **Y-sort** (Transparency Sort custom axis
  `(0,1,0)`); crisp pixel pipeline (Point filtering, consistent PPU, 2D Pixel-Perfect Camera).
- **Data-driven design:** ScriptableObjects for strains, crops, and missions/cities, with **one
  `GameConfig`** asset holding every tunable number so balancing happens in one place.
- **Persistence:** `JsonUtility` save to `persistentDataPath`, autosaving on change, **versioned and
  additive** so older saves keep loading as state grows.
- **Scenes:** MainMenu → Intro → Farm → Battle → Tutorial, all wired into Build Settings in order.
- **Process:** GitHub `main`-direct workflow with Conventional Commits; a GitHub Project (Kanban)
  board with weekly milestones; a short testing log per milestone; tagged release builds.

## 12. Main systems / scripts expected to build

- **Farm:** avatar movement + interaction, a grid/tile interaction layer, crop growth, harvested
  monsters that **roam** the farm, and proximity-opened buildings.
- **Monsters:** six strains as data assets, each with a passive, plus a hunger state machine and
  permadeath.
- **Economy:** a wallet/currency, a Shop (seeds + items), and a Lab (permanent strain upgrades).
- **Combat:** a deploy screen, a battle manager spawning a squad + enemies, a directly-controlled
  hero (melee swing, dash), squad follow + auto-attack + commands, throwable field items, area-gated
  progression, and a win/lose result.
- **Core / tech:** a single `GameConfig`, a save manager, and cross-scene hand-off of the squad.
- **Presentation / UX:** HUD, minimap, panels, audio managers (music + SFX + volume), and new-player
  guidance (onboarding + a dedicated combat tutorial).

## 13. Asset / resource plan

- **Art:** a single cohesive ecosystem — **Cute Fantasy RPG** by Kenmi (+ its expansion packs) —
  chosen because one artist guarantees a consistent look. It is **licensed**; the licence forbids
  redistributing the files, so the art is **git-ignored / kept out of the public source tree**, and
  the playable game is distributed as a **build** instead.
- **Fonts:** Pixel Operator (CC0) for UI, Alagard (CC-BY) for titles.
- **Audio:** Cute Fantasy ships no audio, so SFX come from the **Ninja Adventure** pack (CC0, audio
  only), and music from three **CC0 OpenGameArt** tracks.
- **Tooling:** stock Unity packages (URP, 2D Tilemap Extras, Input System, TextMesh Pro, Cinemachine).
- All third-party assets are credited in `doc/asset-credits.md`.

## 14. Legal, ethical, social, accessibility & security considerations

- **Legal / ethical:** every external asset is licensed and credited; the licensed art is never
  re-hosted publicly (kept local per its licence). Use of an **AI coding assistant is disclosed**.
- **Social:** deliberately **cozy and non-graphic** — no gore, no human-on-human violence, no sexual
  content, no targeting of any real group; no microtransactions, loot boxes, gambling, or dark
  patterns.
- **Accessibility:** simple conventional input (no chords / no twitch timing); **full key rebinding**;
  independent **Master / Music / SFX** volume sliders; state shown with **text + colour together**
  (e.g. "(Hungry)"), not colour alone; clear objective banners + onboarding + a tutorial. *Known gaps:*
  no colour-blind palette / text-scale toggle, tuned for 1920×1080.
- **Security:** offline, single-player; **no network, accounts, telemetry, or personal data**; local
  JSON save with **defensive loading** (corrupt save falls back to defaults); no dynamic code loading.

## 15. Development schedule / milestone plan

A four-week plan, one milestone per week, each with P0 (must-ship) before P1 (target):

| Week | Milestone | Focus |
|---|---|---|
| **M1** | Avatar Farm & Monster Foundation | WASD avatar + camera; walk-up plant/harvest; six strains as data; hunger state machine; save/load extended. |
| **M2** | Economy, Buildings & Lab | Single currency; Shop (seeds + items); all numbers in `GameConfig`; (P1) Lab upgrades. |
| **M3** | Combat (heaviest week) | Battle scene + deploy; WASD-lead hero + auto-attacking squad + commands; a field item; permadeath; win/lose + reward. |
| **M4** | Progression, Art & Polish + Demo | City-selection map; integrate art for farm/avatar/strains; balancing pass; audio; onboarding + tutorial; record demo; release build. |

**Risk management:** combat (M3) is scoped as the heaviest week with a deliberately thin P0; hard
P0/P1/P2 tiers and a fixed MVP gate guard against scope creep; saves are versioned to tolerate
growth. The detailed backlog and the design bible live under `doc/` in the repository.
