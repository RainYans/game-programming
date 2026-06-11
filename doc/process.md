# Process & Technical Setup

How the project is built and tracked. (The schedule lives in [roadmap.md](roadmap.md); the
issue breakdown in [backlog.md](backlog.md).)

## Unity Technical Setup

- **Engine:** Unity 2022.3 LTS.
- **Render pipeline:** URP, 2D Renderer.
- **View:** **Top-down (orthographic)**, rectangular Tilemap; Y-sort via Transparency Sort
  Custom Axis `(0, 1, 0)`. **Pixel-art**: FilterMode = Point, consistent PPU, 2D Pixel Perfect
  Camera. (Pivoted from isometric — see [design/direction.md](design/direction.md).)
- **Input:** New Input System (used by both the farm avatar and combat controls).
- **Data:** ScriptableObjects for strains (`ZombieData`), crops (`CropData`), missions/
  cities, with a single **`GameConfig`** SO holding all tunable numbers.
- **Save:** `JsonUtility` to `Application.persistentDataPath` via `SaveManager`; autosaves on
  state change; fresh-save defaults seeded from `GameConfig`. Extend `SaveData` additively
  and tolerate missing fields when loading older saves.

## Project Layout

This is a monorepo of course projects; the game lives in **`MonsterFarm/`** (alongside the
older `2D_Game_Improvement` and `SolarSystem`). Scripts are in
`MonsterFarm/Assets/Scripts/`.

## Source Control

- **Repo:** https://github.com/RainYans/game-programming
- **`.gitignore`:** Unity template (configured). Local dev tooling and the local
  `memory/` directory are ignored.
- **Git LFS:** intentionally skipped — 2D art is small; not worth the overhead. Revisit
  only if binary assets ever grow large.
- **Branching:** **`main`-direct workflow** — commit straight to `main` for routine work.
  Open a short-lived `feature/<name>` branch + PR only for risky/large changes that need a
  safety net. (Early milestones used per-chunk feature branches; that policy is retired.)
- **Commits:** Conventional Commits — `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`.
  Code, identifiers, and commit messages are in **English**.

## Issue Tracking

- **Board:** GitHub Project (Kanban). Columns and conventions in [backlog.md](backlog.md).
- **Issues, sub-issues, milestones, and the board** are managed through the GitHub web UI
  (Issues + Projects).
- **Milestones:** the four weekly milestones in [roadmap.md](roadmap.md).

## Testing

- Manual in-editor verification is the baseline; combat balance is tuned and checked through
  in-editor playtests (see [`testing/`](testing/)).
- Keep a short **testing log per milestone** under [`testing/`](testing/) (what was tested,
  what failed, what changed). `week-1.md` is the existing example.

## Day-to-day Kanban moves

Keep [Project #1](https://github.com/users/RainYans/projects/1) in sync as work happens.
The columns are `Backlog → Ready → In progress → In review → Done`.

| When | Move on the board |
|---|---|
| Starting a Story | Drag the card from **Ready** to **In progress** |
| Pausing / blocked | Drag the card back to **Backlog** |
| PR opened / self-review | Drag the card to **In review** |
| Finished + merged | Close the issue — the "Item closed → Done" automation moves the card to **Done** |
| Spawn a Task under a Story | Open a new **Task** issue (`Task` + `system/X`) and link it as a sub-issue of the Story |
| Brand-new Story | Open a new **Story** issue (`Story`, `P0/P1`, `system/X`, milestone); the auto-add workflow lands it in **Backlog** |

## Definition of Done — Per-Chunk Tiers

Before writing code for any new chunk, I declare acceptance criteria in three tiers and decide
which to ship. This is the planning vocabulary; it sits *underneath* the
issue-closing DoD in [backlog.md](backlog.md).

- **M (minimum):** mechanic fires, no errors, the loop closes; placeholder numbers /
  visuals.
- **T (target):** feedback is in (visual / audio / UI), balance is reasonable, placeholder
  art is acceptable.
- **P (polish):** edge cases handled, transitions in place, demoable.

I state the chunk's **M / T / P** explicitly **before** writing code and ship at that tier, so
"done" means a tier I picked up front rather than a moving target.

## Releases

- Tag a build at the end of each milestone (`v0.1`, `v0.2`, …) with artifacts attached, once
  a packaged build exists.
