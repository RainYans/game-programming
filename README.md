# Zombie Farm (working title)

## One-Sentence Game Idea

Grow zombies on an isometric farm, then send squads out to raid scripted enemy farms for loot.

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

- **Week 1 — Foundation:** Isometric tilemap setup, camera controller (pan + zoom), click-to-cell coordinate conversion, tile highlight on hover.
- **Week 2 — Farming Loop:** Plant action, growth state machine driven by `DateTime.UtcNow`, harvest action, basic inventory data.
- **Week 3 — Battle Loop:** `BattleSimulator` as a pure C# class, battle scene with placeholder units, `BattlePlayer` to replay the event log, win/lose result screen.
- **Week 4 — Glue:** Save/load to JSON, coin economy, simple shop UI to buy seeds, one full playable loop end-to-end.
- **Week 5 — Polish & Demo:** Bug fixing, numerical tuning, basic UI cleanup, record a short gameplay video, write postmortem.

Stretch goals (only if Week 4 finishes early): a second seed type, a second enemy stage, sound effects.

## Unity Technical Plan

- **Unity version:** Unity 6 LTS (or 2022.3 LTS if stability is preferred)
- **Render pipeline:** URP, 2D renderer
- **Tilemap:** Built-in Isometric Tilemap, with Transparency Sort Mode = Custom Axis `(0, 1, 0)` for Y-based depth sorting
- **Input:** New Input System package, mouse drag + scroll + click bindings
- **Data:** ScriptableObjects for crops, units, stages; one `GameConfig` SO for tunable numbers
- **Save format:** JSON via `JsonUtility`, written to `Application.persistentDataPath`

Scripts to build, in order:

- `GridManager` — isometric tilemap, world ↔ cell coordinate conversion
- `CameraController` — mouse drag pan + scroll wheel zoom, clamped to farm bounds
- `TileInteraction` — click to plant/harvest, hover highlight
- `CropData` (ScriptableObject) + `CropInstance` — growth state machine
- `Inventory` — item ID → count
- `BattleSimulator` — pure C# class, takes two unit lists, returns result + event log
- `BattlePlayer` — replays event log with animations
- `SaveManager` — JSON serialization, called on key events (harvest, battle end, purchase)

## GitHub Repository Setup Plan

- **Repo:** https://github.com/RainYans/game-programming (private)
- **.gitignore:** Unity official template from github.com/github/gitignore
- **Git LFS:** Enabled for `.png`, `.psd`, `.fbx`, `.wav`, `.mp3`
- **Branching:** `main` is always stable, feature work on `feature/<name>` branches, merged via pull request even when working solo (forces self-review)
- **Commits:** Conventional commit style — `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`
- **Documentation:** `README.md` for overview, `/docs` folder for design notes and weekly progress logs
- **Releases:** Tag the end of each week as `v0.1`, `v0.2`, etc., with build artifacts attached

## Biggest Risk

Scope creep, not technical difficulty. Solo devs typically spend months on the farm, then months on combat, and ship nothing. Mitigation: PvP cut entirely, v1 is PvE only against scripted enemy farms; new ideas go to backlog until the vertical slice is playable. Secondary risk: isometric depth sorting bugs with overlapping multi-tile objects — budget 2–3 days.

## One Visible Task Before Next Session

Initialize the Unity project: create a new Unity 6 URP 2D project, add `.gitignore` and Git LFS, push the empty project to the repo, and get an isometric tilemap rendering a 10×10 grid of placeholder tiles on screen. Outcome: open the project, see a diamond-shaped grid.


## Scope Sorting

### Must-have (required for the game to function — build first)
- Isometric tilemap with click-to-cell detection
- Camera controls (mouse pan + scroll zoom)
- Plant action on empty tile
- Real-time crop growth (one seed type)
- Harvest action → add to inventory
- `BattleSimulator` (pure C# auto-battle logic)
- One playable battle stage with placeholder enemies
- Win/lose result + coin reward
- Save/load to JSON

### Should-have (important for quality, not required for first test)
- Hover highlight on tiles
- Simple shop UI to buy seeds
- Battle replay animations (`BattlePlayer`)
- 2-3 zombie types with basic stat differences
- 2-3 enemy stages with varied difficulty
- Numerical balancing pass
- Basic sound effects

### Could-have (only if the main game works and is tested)
- Multiple seed types with rock-paper-scissors counters
- Particle effects (planting, harvest, combat hits)
- Background music
- Settings menu (volume, resolution)
- Tutorial / first-time player guidance
- Statistics screen (zombies grown, battles won)

### Cut first (remove if scope becomes too large)
- PvP / online multiplayer
- Crossbreeding system (combining seeds for new species)
- Terrain types affecting growth
- Narrative / story mode
- Achievement system
- Cloud save
- Polished art replacing placeholders