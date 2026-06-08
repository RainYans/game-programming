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

- **Repo:** https://github.com/RainYans/game-programming (private).
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
- **`gh` CLI is installed and authenticated** — issues, sub-issues, milestones, and the board can
  be managed from the CLI (or the GitHub web UI).
- **Milestones:** the four weekly milestones in [roadmap.md](roadmap.md).

## Testing

- Manual in-editor verification is the baseline; the deterministic `BattleSimulator` can be
  unit-tested.
- Keep a short **testing log per milestone** under [`testing/`](testing/) (what was tested,
  what failed, what changed). `week-1.md` is the existing example.

## Day-to-day Kanban moves

Keep [Project #1](https://github.com/users/RainYans/projects/1) in sync as work happens.
The columns are `Backlog → Ready → In progress → In review → Done`. The helpers live in
[`tools/board/`](../tools/board/).

| When | Command |
|---|---|
| Starting a Story | `bash tools/board/board.sh move <issue#> in-progress` |
| Pausing / blocked | `bash tools/board/board.sh move <issue#> backlog` |
| PR opened, waiting on review | `bash tools/board/board.sh move <issue#> in-review` |
| Finished + merged | `gh issue close <issue#> -c "shipped in <commit>"` &nbsp;*(the "Item closed → Done" workflow moves the card)* |
| Spawn a Task under a Story | `gh issue create --title "Task — XXX" --label Task,system/X --body "..."` then `bash tools/board/board.sh link <story#> <task#>` |
| Brand-new Story | `gh issue create --title "Story — XXX" --label Story,P0,system/X --milestone "M4 — Progression & Polish"` &nbsp;*(auto-add workflow lands it in Backlog; sub-link it to its Epic if applicable)* |

Project ID and Status-field option IDs are hardcoded in
[`tools/board/board.sh`](../tools/board/board.sh) and
[`tools/board/set-status.sh`](../tools/board/set-status.sh) — refer there if a deeper
GraphQL call is needed.

## Definition of Done — Per-Chunk Tiers

Before writing code for any new chunk, acceptance criteria are declared in three tiers and
the user picks which to ship. This is the planning vocabulary; it sits *underneath* the
issue-closing DoD in [backlog.md](backlog.md).

- **M (minimum):** mechanic fires, no errors, the loop closes; placeholder numbers /
  visuals.
- **T (target):** feedback is in (visual / audio / UI), balance is reasonable, placeholder
  art is acceptable.
- **P (polish):** edge cases handled, transitions in place, demoable.

State the chunk's **M / T / P** explicitly **before** writing code, get an explicit pick
from the user, and ship at that tier. This avoids the recurring "I called it done, the user
feels it isn't" mismatch.

## Releases

- Tag a build at the end of each milestone (`v0.1`, `v0.2`, …) with artifacts attached, once
  a packaged build exists.
