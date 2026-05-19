# Zombie Farm (working title)

## Current Status

Week 1 — Project initialization. Unity project setup in progress, no playable content yet.

## One-Sentence Game Idea

Grow zombies on an isometric farm, then send squads out to raid scripted enemy farms for loot.

## How to Run

**Current stage:** No playable build available yet.

**Future instructions (once development is further along):**
1. Download the latest ZIP from the Releases page
2. Extract and run `ZombieFarm.exe` (Windows only)
3. Or open the project source in Unity 6 LTS and press Play

## Controls

- **Left Mouse Button** — Select tile, plant, harvest, confirm menu
- **Right Mouse Button** — Cancel / back
- **Mouse Drag (middle button or empty area)** — Pan camera
- **Scroll Wheel** — Zoom in/out
- **ESC** — Open pause menu

*Controls may change during development; latest build is authoritative.*

## Unity Version

Unity 6 LTS, URP 2D Renderer.

## Smallest Playable Version (MVP)

One farm screen. Click tile → plant zombie seed → wait → harvest → send 3 zombies into one auto-battle → earn coins. One seed type, one enemy stage, gray boxes for art.

## What the Player Does Moment to Moment

1. Pans the camera with mouse drag, zooms with scroll wheel.
2. Hovers over a farm tile, sees a highlight.
3. Clicks an empty tile, picks a seed from a small popup, confirms planting.
4. Watches a growth timer tick down on the tile (real-time, seconds for MVP).
5. Clicks a ripe tile to harvest, sees the zombie added to inventory counter.
6. Repeats planting and harvesting until they have enough units.
7. Opens the battle menu, selects 3 zombies, hits "Send."
8. Watches a short auto-battle play out, sees win/lose result and coin reward.
9. Returns to the farm with more coins, buys more seeds, repeats.

## Realistic Vertical Slice — Five-Week Plan

- **Week 1 — Foundation:** Isometric tilemap, camera controller (pan + zoom), click-to-cell conversion, hover highlight.
- **Week 2 — Farming Loop:** Plant action, growth state machine driven by `DateTime.UtcNow`, harvest action, basic inventory.
- **Week 3 — Battle Loop:** `BattleSimulator` pure C# class, battle scene with placeholder units, `BattlePlayer` replays event log, win/lose result screen.
- **Week 4 — Glue:** JSON save/load, coin economy, simple shop UI, full loop playable end-to-end.
- **Week 5 — Polish & Demo:** Bug fixing, numerical tuning, UI cleanup, gameplay video, postmortem.

## Unity Technical Plan

- **Unity version:** Unity 6 LTS
- **Render pipeline:** URP, 2D Renderer
- **Tilemap:** Isometric Tilemap, Transparency Sort Mode = Custom Axis `(0, 1, 0)`
- **Input:** New Input System
- **Data:** ScriptableObjects (crops, units, stages) with a single `GameConfig` SO for tunable numbers
- **Save format:** `JsonUtility` to JSON, written to `Application.persistentDataPath`

Scripts in build order: `GridManager` → `CameraController` → `TileInteraction` → `CropData` + `CropInstance` → `Inventory` → `BattleSimulator` → `BattlePlayer` → `SaveManager`

## Scope Sorting

### Must-have (required for the game to function)
Isometric tilemap, camera controls, plant action, real-time growth, harvest action, inventory, `BattleSimulator`, one playable battle stage, win/lose + coin reward, JSON save/load.

### Should-have (important for quality, not required for first test)
Hover highlight, shop UI, battle replay animations, 2–3 zombie types, 2–3 enemy stages, numerical balancing, basic sound effects.

### Could-have (only if the main game works and is tested)
Rock-paper-scissors counter system, particle effects, background music, settings menu, tutorial, statistics screen.

### Cut first (remove if scope becomes too large)
PvP, crossbreeding system, terrain types, narrative mode, achievements, cloud save, polished art.

## GitHub Repository Setup

- **Repo:** https://github.com/RainYans/game-programming (private)
- **.gitignore:** Unity official template
- **Git LFS:** Enabled for `.png`, `.psd`, `.fbx`, `.wav`, `.mp3`
- **Branching:** `main` stays stable, feature work on `feature/<name>` branches with pull requests
- **Commits:** Conventional Commits style (`feat:`, `fix:`, `refactor:`, `docs:`, `chore:`)
- **Issues:** GitHub Issues used to track bugs, features, risks, and testing notes
- **Documentation:** `README.md` for overview, `/docs` folder for design notes and weekly progress logs

## Asset Credits

**Current stage:** No external assets used yet. All visuals are Unity built-in placeholder shapes (cubes, geometric primitives).

**Planned sources:**
- Kenney.nl — CC0 free assets
- itch.io — isometric asset packs (author and license to be listed when purchased)
- OpenGameArt.org — CC BY / CC BY-SA licensed assets

Each external asset added will be credited here with author, source link, and license.

## Testing Evidence

**Current stage:** No testable functionality yet.

Each week's testing will be logged here:
- What was tested
- What failed / which bugs surfaced
- What changed afterwards

Detailed weekly notes will live under `/docs/testing/`.

## Biggest Risk

Scope creep, not technical difficulty. Solo devs typically spend months on the farm, then months on combat, and ship nothing. Mitigation: PvP cut entirely, v1 is PvE only against scripted enemy farms; new ideas go to backlog until the vertical slice is playable. Secondary risk: isometric depth sorting bugs with overlapping multi-tile objects — budget 2–3 days.

## One Visible Task Before Next Session

Initialize the Unity project: create a Unity 6 URP 2D project, add `.gitignore` and Git LFS, push the empty project to the repo, and get a 10×10 isometric tilemap rendering with placeholder tiles. Outcome: open the project, see a diamond-shaped grid.