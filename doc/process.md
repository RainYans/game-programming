# Process & Technical Setup

How the project is built and tracked. (The schedule lives in [roadmap.md](roadmap.md); the
issue breakdown in [backlog.md](backlog.md).)

## Unity Technical Setup

- **Engine:** Unity 2022.3 LTS.
- **Render pipeline:** URP, 2D Renderer.
- **View:** Isometric Tilemap; Transparency Sort Mode = Custom Axis `(0, 1, 0)` so taller
  objects depth-sort correctly.
- **Input:** New Input System (used by both the farm avatar and combat controls).
- **Data:** ScriptableObjects for strains (`ZombieData`), crops (`CropData`), missions/
  cities, with a single **`GameConfig`** SO holding all tunable numbers.
- **Save:** `JsonUtility` to `Application.persistentDataPath` via `SaveManager`; autosaves on
  state change; fresh-save defaults seeded from `GameConfig`. Extend `SaveData` additively
  and tolerate missing fields when loading older saves.

## Project Layout

This is a monorepo of course projects; the game lives in **`ZombieFarm/`** (alongside the
older `2D_Game_Improvement` and `SolarSystem`). Scripts are in
`ZombieFarm/Assets/Scripts/`.

## Source Control

- **Repo:** https://github.com/RainYans/game-programming (private).
- **`.gitignore`:** Unity template (configured). Local Claude/MCP tooling and the local
  `memory/` directory are ignored.
- **Git LFS:** to be set up for binary assets (`.png .psd .fbx .wav .mp3`) before bulk art
  import — tracked as a `tech` task in [backlog.md](backlog.md), not yet configured.
- **Branching:** `main` stays stable. Work happens on `feature/<name>` branches and merges
  via Pull Request (self-review even when solo). Prefer **one branch per story/epic**.
- **Commits:** Conventional Commits — `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`.
  Code, identifiers, and commit messages are in **English**.

## Issue Tracking

- **Board:** GitHub Project (Kanban). Columns and conventions in [backlog.md](backlog.md).
- **`gh` CLI is not installed** here — issues, sub-issues, milestones, and the board are
  managed in the **GitHub web UI**.
- **Milestones:** the four weekly milestones in [roadmap.md](roadmap.md).

## Testing

- Manual in-editor verification is the baseline; the deterministic `BattleSimulator` can be
  unit-tested.
- Keep a short **testing log per milestone** under [`testing/`](testing/) (what was tested,
  what failed, what changed). `week-1.md` is the existing example.

## Releases

- Tag a build at the end of each milestone (`v0.1`, `v0.2`, …) with artifacts attached, once
  a packaged build exists.
