# Process Plan

## Five-Week Plan

- **Week 1 — Foundation:** Isometric tilemap, camera controller (pan + zoom), click-to-cell conversion, hover highlight.
- **Week 2 — Farming Loop:** Plant action, growth state machine driven by `DateTime.UtcNow`, harvest action, basic inventory.
- **Week 3 — Combat Loop:** `BattleSimulator` pure C# class, mission scene with placeholder engineered zombies vs. wild zombies, `BattlePlayer` replays event log, mission result screen.
- **Week 4 — Glue:** JSON save/load, resource economy, simple seed bank UI, full loop playable end-to-end.
- **Week 5 — Polish & Demo:** Bug fixing, numerical tuning, UI cleanup, gameplay video, postmortem.

## Unity Technical Plan

- **Unity version:** Unity 2022.3 LTS
- **Render pipeline:** URP, 2D Renderer
- **Tilemap:** Isometric Tilemap, Transparency Sort Mode = Custom Axis `(0, 1, 0)`
- **Input:** New Input System
- **Data:** ScriptableObjects (zombie strains, missions, cities) with a single `GameConfig` SO for tunable numbers
- **Save format:** `JsonUtility` to JSON, written to `Application.persistentDataPath`

### Scripts in Build Order

1. `GridManager` — isometric tilemap, world ↔ cell coordinate conversion
2. `CameraController` — mouse drag pan + scroll wheel zoom, clamped to farm bounds
3. `TileInteraction` — click to plant/harvest, hover highlight
4. `CropData` (ScriptableObject) + `CropInstance` — growth state machine
5. `Inventory` — item ID → count
6. `BattleSimulator` — pure C# class, takes two unit lists, returns result + event log
7. `BattlePlayer` — replays event log with animations
8. `SaveManager` — JSON serialization, called on key events (harvest, mission end, purchase)

## GitHub Workflow

- **Repo:** https://github.com/RainYans/game-programming (private)
- **.gitignore:** Unity official template from github.com/github/gitignore
- **Git LFS:** Enabled for `.png`, `.psd`, `.fbx`, `.wav`, `.mp3`
- **Branching:** `main` stays stable. Feature work on `feature/<name>` branches, merged via pull request (forces self-review even when working solo).
- **Commits:** Conventional Commits style:
  - `feat:` new feature
  - `fix:` bug fix
  - `refactor:` code restructuring, no behavior change
  - `docs:` documentation only
  - `chore:` build, tooling, dependencies
- **Issues:** GitHub Issues track bugs, features, risks, testing notes, and next tasks. Labels: `bug`, `feature`, `risk`, `testing`, `week-1` through `week-5`.
- **Releases:** End of each week tagged as `v0.1`, `v0.2`, etc., with build artifacts attached.
- **Documentation:** `README.md` for overview, `/docs` folder for design notes and weekly logs.