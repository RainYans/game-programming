# Week 1 Testing Notes

**Status:** M0 (project setup) and M1 (map, camera, tile interaction) complete.

## What Was Tested

- Project opens in Unity 2022.3 LTS with URP 2D, no compile errors.
- Isometric 10×10 grid renders (placeholder diamond tile, PPU 256 to match the
  default isometric cell size `(1, 0.5, 1)`).
- Camera: middle-mouse drag pans, scroll wheel zooms, both clamped.
- Tile interaction: hovering a farm cell shows a highlight that follows the cursor;
  left-clicking a cell logs its coordinate to the Console.

## What Failed

- First pass of `TileInteraction` showed no highlight and clicks did nothing. Cause:
  the orthographic camera's `ScreenToWorldPoint` returns a world point at z = -10, so
  `WorldToCell` produced a cell with non-zero z and `HasTile` never matched tiles
  painted at z = 0. Both the highlight and the click gate (`onFarm`) silently failed.

## What Changed Afterwards

- Zero the world z (`world.z = 0`) before the cell lookup in `TileInteraction`, so
  cell resolution stays on the z = 0 tilemap plane.
