# Changelog

All notable changes to **Monster Farm**. Only **v1.0** and **v1.1** are tagged builds on the
[GitHub Releases](https://github.com/RainYans/game-programming/releases) page; the entries under
*Earlier development milestones* are recorded for the project history, not separately tagged.

The project uses Conventional Commits and keeps every balance number in one `GameConfig` asset. The
workflow started on short-lived `feature/*` branches with pull requests (PRs #1, #18–#24) and moved
to a **main-direct** workflow once the foundation was stable (2026-06-04), as noted in
[`doc/process.md`](doc/process.md).

---

## v1.1 — 2026-06-17 · `e569e4a` — new-player onboarding + combat feel

- **How-to-Play manual** — a turning picture-book that teaches the core loop and the controls;
  auto-opens on the first farm visit and is re-openable from the pause menu.
- **Ground-trail farm onboarding** — a path of arrows is painted on the ground toward each
  objective; the shop step now requires an actual purchase, and the War Camp is gated until the
  basics are done.
- **Clearer combat** — combatants pass through each other and use boids-style separation (no more
  stacking); the melee attack is a visual-only jab (no body shove); ranged units fire a projectile.
- **Combat-tutorial robustness** — every step has a soft-lock fallback so it can't hang, plus an
  end-of-training panel.
- Framerate-correct farm-roamer movement; deploy panel shows an empty-state hint; starting resources
  50 → 200 for a gentler early game.

## v1.0 — 2026-06-12 · `8381ce3` — first packaged playable build (Windows)

The full vertical slice, released as a Windows build.

- **Front-end flow** — main menu → storybook intro → farm; first-launch farm onboarding and a
  dedicated combat-tutorial scene.
- **Combat rebuilt into a four-room action-brawler raid** (Farm Outskirts → River + Bridge → Hedge
  Garden → Village Square): a controllable hero with a melee swing + dash, command-the-whole-squad,
  throwable items, a minimap, area-gated progression, and a win/lose result screen.
- **Audio** — per-scene background music with crossfades, a full SFX set, and Master/Music/SFX
  volume sliders.
- **Depth & polish** — a Lab (permanent strain upgrades), a Bestiary codex, key rebinding, unit
  collision (Rigidbody2D + colliders so units respect walls), a boss, and a city-selection map.
- Framerate-independent battle-unit movement.
- **Project rename** Zombie Farm → Monster Farm; art reskinned to the cohesive **Cute Fantasy** set;
  licensed art untracked from the public repo; scripts reorganised into system folders; dead assets
  removed.
- Submission documentation added (README, evidence, asset credits, peer feedback, reference &
  contribution).

## Earlier development milestones (pre-1.0, not tagged)

- **2026-06-08** — Battle rebuilt into the four-room action-brawler; project renamed to MonsterFarm;
  art assets + UI rework; first submission docs.
- **2026-06-04** — Combat core: deploy, stages/prep, permadeath, the six strain passives, items, and
  mouse control; hunger-in-combat, a buyable Rotten Onion, and the city-selection map; combat tuning
  consolidated in `GameConfig`; **switched to a main-direct workflow**.
- **2026-05-27** — Six monster strains, the hunger system, per-unit farm roamers, and the seed-pick
  popup; save versioning + an M1 testing log; the real-time battle core (slice 1).
- **2026-05-26** — Farm scene rebuilt around a walking avatar with a Cinemachine follow camera,
  buildings, and scenery.
- **2026-05-25** — Design docs restructured and the plan re-scoped for the expanded two-mode game.
- **2026-05-22** — Shop & buy UI, building interaction, the save system, and item/seed inventories
  with on-screen counters.
- **2026-05-21** — Prototype combat loop: a deterministic battle simulator, deploy, replay, and a
  reward.
- **2026-05-20** — Farming loop (plant → grow → harvest → inventory) on the isometric farm scaffold
  (grid, camera, tile interaction); the Week-1 foundation.
- **2026-05-19** — Initial concept, README, scope sorting (MoSCoW), and the first round of peer
  feedback captured.
