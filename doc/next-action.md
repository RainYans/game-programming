# Next Action

## Current Goal

Build the combat loop (M3): deploy harvested zombies against a fallen city and watch
an auto-battle resolve, then collect a resource reward.

## Concrete Task

- `ZombieData` / `UnitStats` and `MissionData` / `CityData` ScriptableObjects.
- `BattleSimulator` — pure C# class: two unit lists → result + ordered event log.
- Mission map: pick up to 3 zombies from inventory → Deploy.
- `BattlePlayer` — replay the event log as timed animations; mission result + reward.

## Done So Far

- **M0 — Setup:** Unity 2022.3 LTS + URP 2D, New Input System (Both), Transparency
  Sort Axis `(0, 1, 0)`.
- **M1 — Map & Camera:** isometric 10×10 grid, `GridManager`, `CameraController`
  (pan + zoom), `TileInteraction` (hover highlight + click).
- **M2 — Farming Loop:** `CropData` + `CropInstance` growth state machine,
  input-agnostic `FarmActions`, `Inventory` + on-screen counter. Full
  plant → grow → harvest → inventory loop playable.

## Current Branch

`feature/m3-combat-loop` (cut from `main`; M0–M2 already merged).

## Target Completion

End of Week 3.
