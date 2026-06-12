# Evidence Index — What Changed & Where to Find It

*Maps each assessment question to concrete evidence. Pairs with
[reference-and-contribution.md](reference-and-contribution.md) and the GitHub commit history.*

## What changed since Session 1

| Stage | What it was | What I changed | Evidence |
|---|---|---|---|
| Concept / logic-first | "Zombie Farm" idea; planting loop + inventory + a basic battle, placeholder/mixed art, isometric | Built the **core systems first** (farm loop, inventory, save, basic combat) before art | early commits; `Inventory`, save system, `GameConfig` |
| Art & view pivot | Mixed/placeholder art, isometric | Re-themed to **Monster Farm** with the cohesive **Cute Fantasy** set; moved to **top-down** with Y-sort; reskinned farm + UI; real animated monster sprites | `Farm.unity`, `Assets/Art/…`, `Resources/MonsterAnim/`, commit history |
| Battle rebuild | One flat/placeholder arena, "no combat feel" | Rebuilt into a **four-room linear raid** (Farm Outskirts → River+Bridge → Hedge Garden → Village Square) with area-gated progression + minimap | `Battle.unity`, `BattleManager`/`BattleArea`/`BattleGate` |
| Combat feel | Passive hero, auto-attack, split RTS controls | **Action-brawler**: hero melee swing (animation + slash VFX + knockback), dash, **left-click attack / right-click command-all**, items; hero HP & death = defeat | `LeaderCombat`, `LeaderCombatant`, `BattleCommandController`, `BattleAgent` |
| UI / HUD | Plain, unclear HUD | Rebuilt: squad bars, item slots with icons, framed minimap, leader HP bar, parchment/pixel theme; reworked result screen | `Battle.unity` HUD, `BattleCommandController` |
| Tuning | Units too slow; squad cards shrunk by a scale bug; UI clicks misaligned after a canvas-scaling change | Retuned move speeds; fixed the squad-card scale bug; fixed the screen→canvas coordinate mapping | `BattleAgent` (`BattleMoveScale` 0.6→1.1), `AvatarController` (hero 3→2.8) |
| Professionalism | Folder/name said "ZombieFarm" | Renamed project folder to **MonsterFarm/**, product name + menus to "Monster Farm" (kept internal class ids/saves intact); wrote full submission docs | this `doc/`, `README.md`, rename commit |
| Final polish pass | No music; hunger/combat only; fixed WASD; one flat Scripts folder | Added per-scene **BGM + SFX + volume sliders**, a **Lab** (resources → permanent strain upgrades, persisted), a **Bestiary** codex, **key rebinding**, **unit collision** (no wall-clip/overlap), **cities 2–3 + a boss**, and **reorganised the scripts into system folders** | `MusicManager`/`SfxManager`/`MasterAudio`, `LabManager`/`LabPanel`, `BestiaryPanel`, `KeyBindings`/`KeyRebindUI`, `BattleAgent` (RB2D+collider), `Assets/Scripts/` (Core/Farm/Combat/UI/…) |

## Evidence index (assessment criterion → where)

| Criterion | Evidence |
|---|---|
| Game can be run | Open `MonsterFarm/`, Play `Farm.unity` (or `Battle.unity`) — see [README](../README.md) |
| Controls are clear | Controls table in [README](../README.md); `BattleCommandController`, `AvatarController` |
| Goal is clear | "How to Play" in [README](../README.md); win = clear City1's four areas; lose = hero/squad down |
| README explains how to play | [README.md](../README.md) |
| GitHub shows progress | commit history on `origin/main` |
| Design decisions explained | [reference-and-contribution.md](reference-and-contribution.md) + the [design bible](design/) |
| Credits included | [asset-credits.md](asset-credits.md) |
| Contribution is clear | [reference-and-contribution.md](reference-and-contribution.md) |
| Unity scene | `Farm.unity`, `Battle.unity` (four distinct rooms I designed and hand-tuned) |
| Playable feature | action-brawler raid (`LeaderCombat` + `BattleManager` area mode) |
| Script / system | system-organised scripts under `MonsterFarm/Assets/Scripts/` (Core / Farm / Combat / UI / …) |
| Level-design change | `Battle.unity` four-room rebuild |
| Feedback improvement | [peer-feedback.md](peer-feedback.md) |
| Reference transformation | [reference-and-contribution.md](reference-and-contribution.md) |
| Playtest note | [testing/playtest-s5.md](testing/playtest-s5.md), [testing/](testing/) |
| Accessibility / audio | [accessibility.md](accessibility.md); Esc options menu (master / music / SFX volume + key rebinding); per-scene music via `MusicManager` |
| Next action | below |

## Scope sorting (MoSCoW)

- **Must-have (built first, done):** WASD avatar; plant/harvest; monster inventory; deploy → raid;
  squad + hero real-time combat; area clear → gate → win/lose; save/load; clear controls + goal.
- **Should-have (done):** six strains with passives; hunger system; shop (seeds + items); themed
  four-room level; HUD + minimap + result screen; cohesive art + animation; first-launch onboarding
  + a dedicated combat tutorial scene.
- **Could-have (built this final stretch):** cities 2–3 (Thornwood Hollow, Ashen Reach), a **boss**
  (Wildbloom Brute — HP 200, ~2× scale), **audio** (per-scene BGM + SFX + master/music/sfx volume
  sliders), **Lab** strain upgrades, a **Bestiary** codex, and **key rebinding**.
- **Cut first (dropped/deferred to protect the slice):** isometric view (cut), RTS micro (cut),
  open-world farm/seasons (cut), the task system / plot expansion / branching map (cut for scope).
  The one outstanding deferred item is a **packaged Windows build**.

## One concrete next action

**Produce a packaged Windows build** (add the scenes to Build Settings in order, build, and
smoke-test the full loop from a fresh save). The in-game tutorial earlier peer feedback asked for is
now **done** (`Tutorial.unity` + `FarmTutorialController` / `CombatTutorialController`). Runner-up
next action: record the gameplay demo video once the build is produced.

## Build readiness checklist (build deferred this pass)

- [x] Correct scenes exist (`Farm.unity`, `Battle.unity`)
- [x] Player spawns correctly (farm avatar; battle squad + hero)
- [x] Controls work (verified in play)
- [x] UI appears correctly (HUD verified)
- [x] No serious console errors in play (only benign font/underline warnings)
- [ ] Scenes added to Build Settings in final order — to confirm before building
- [ ] Build target chosen (Windows) and build produced — scheduled, not yet done
