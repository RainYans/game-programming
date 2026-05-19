# game-programming

# Zombie Farm

**One-line idea:** Grow zombies on an isometric farm, then send them out to raid enemy farms for loot.

**Core loop:** Plant seed → wait → harvest zombie → send squad to battle → earn coins → buy more seeds.

**Type:** 2.5D PC single-player game, isometric view, PvE.

## Vertical Slice

One farm screen. Click tile → plant zombie seed → wait → harvest → send 3 zombies into one auto-battle → earn coins. One seed, one stage, gray boxes for art.

## Core Systems (build in this order)

- `GridManager` — isometric tilemap, coordinate conversion
- `CameraController` — mouse drag pan + scroll wheel zoom
- `TileInteraction` — click to plant/harvest, hover highlight
- `CropData` (ScriptableObject) + growth via `DateTime.UtcNow`
- `Inventory` — item ID → count
- `BattleSimulator` — pure C# class, returns result + event log
- `BattlePlayer` — replays log with animations
- `SaveManager` — JSON to `Application.persistentDataPath`

## Repo Setup

- Unity official `.gitignore`
- Git LFS enabled for art assets (`.png`, `.psd`, `.fbx`, `.wav`)

## Biggest Risk

Scope creep, not technical difficulty. Solo devs typically spend months on the farm, then months on combat, and ship nothing. Mitigation: PvP cut entirely, v1 is PvE only against scripted enemy farms; new ideas go to backlog until the vertical slice is playable. Secondary risk: isometric depth sorting bugs with overlapping multi-tile objects.