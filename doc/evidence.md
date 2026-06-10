# Evidence Index — What Changed & Where to Find It

*Maps each assessment question to concrete evidence. Pairs with the project report,
[reference-and-contribution.md](reference-and-contribution.md), and the GitHub commit history.*

## What changed since Session 1

| Stage | What it was | What I changed | Evidence |
|---|---|---|---|
| Concept / logic-first | "Zombie Farm" idea; planting loop + inventory + a basic battle, placeholder/mixed art, isometric | Built the **core systems first** (farm loop, inventory, save, basic combat) before art | early commits; `Inventory`, save system, `GameConfig` |
| Art & view pivot | Mixed/placeholder art, isometric | Re-themed to **Monster Farm** with the cohesive **Cute Fantasy** set; moved to **top-down** with Y-sort; reskinned farm + UI; real animated monster sprites | `Farm.unity`, `Assets/Art/…`, `Resources/MonsterAnim/`, commit history |
| Battle rebuild | One flat/placeholder arena, "no combat feel" | Rebuilt into a **four-room linear raid** (Farm Outskirts → River+Bridge → Hedge Garden → Village Square) with area-gated progression + minimap | `Battle.unity`, `BattleManager`/`BattleArea`/`BattleGate` |
| Combat feel | Passive hero, auto-attack, split RTS controls | **Action-brawler**: hero melee swing (animation + slash VFX + knockback), dash, **left-click attack / right-click command-all**, items; hero HP & death = defeat | `LeaderCombat`, `LeaderCombatant`, `BattleCommandController`, `BattleAgent` |
| UI / HUD | Plain, unclear HUD | Rebuilt: squad bars, item slots with icons, framed minimap, leader HP bar, parchment/pixel theme; reworked result screen | `Battle.unity` HUD, `BattleCommandController` |
| Tuning | Units too slow, drag-select misaligned, squad cards shrunk by a scale bug | Retuned move speeds; fixed the canvas drag-box; fixed the squad-card scale bug | `BattleAgent` (`BattleMoveScale` 0.6→1.1), `AvatarController` (hero 3→2.8) |
| Professionalism | Folder/name said "ZombieFarm" | Renamed project folder to **MonsterFarm/**, product name + menus to "Monster Farm" (kept internal class ids/saves intact); wrote full submission docs | this `doc/`, `README.md`, rename commit |

## Evidence index (assessment criterion → where)

| Criterion | Evidence |
|---|---|
| Game can be run | Open `MonsterFarm/`, Play `Farm.unity` (or `Battle.unity`) — see [README](../README.md) |
| Controls are clear | Controls table in [README](../README.md); `BattleCommandController`, `AvatarController` |
| Goal is clear | "How to Play" in [README](../README.md); win = clear City1's four areas; lose = hero/squad down |
| README explains how to play | [README.md](../README.md) |
| GitHub shows progress | commit history on `origin/main` |
| Report explains decisions | the project report |
| Credits included | [asset-credits.md](asset-credits.md) |
| Contribution is clear | the project report, [reference-and-contribution.md](reference-and-contribution.md) |
| Unity scene | `Farm.unity`, `Battle.unity` (four hand-built rooms) |
| Playable feature | action-brawler raid (`LeaderCombat` + `BattleManager` area mode) |
| Script / system | scripts table in the project report |
| Level-design change | `Battle.unity` four-room rebuild |
| Feedback improvement | [peer-feedback.md](peer-feedback.md) + the project report |
| Reference transformation | [reference-and-contribution.md](reference-and-contribution.md) |
| Playtest note | [testing/playtest-s5.md](testing/playtest-s5.md), [testing/](testing/) |
| Next action | below |

## Scope sorting (MoSCoW)

- **Must-have (built first, done):** WASD avatar; plant/harvest; monster inventory; deploy → raid;
  squad + hero real-time combat; area clear → gate → win/lose; save/load; clear controls + goal.
- **Should-have (done):** six strains with passives; hunger system; shop (seeds + items); themed
  four-room level; HUD + minimap + result screen; cohesive art + animation; first-launch onboarding
  + a dedicated combat tutorial scene.
- **Could-have (only if the core holds):** more cities (City2+), a boss, audio polish, a standalone
  build.
- **Cut first (dropped/deferred to protect the slice):** isometric view (cut), RTS micro (cut),
  open-world farm/seasons (cut), boss + packaged build (deferred this pass).

## One concrete next action

**Produce a packaged Windows build** (add the scenes to Build Settings in order, build, and
smoke-test the full loop from a fresh save). The in-game tutorial earlier peer feedback asked for is
now **done** (`Tutorial.unity` + `FarmTutorialController` / `CombatTutorialController`). Runner-up
next actions: a `GameConfig` balancing pass; add City2 from the City-1 template.

## Build readiness checklist (build deferred this pass)

- [x] Correct scenes exist (`Farm.unity`, `Battle.unity`)
- [x] Player spawns correctly (farm avatar; battle squad + hero)
- [x] Controls work (verified in play)
- [x] UI appears correctly (HUD verified)
- [x] No serious console errors in play (only benign font/underline warnings)
- [ ] Scenes added to Build Settings in final order — to confirm before building
- [ ] Build target chosen (Windows) and build produced — scheduled, not yet done
