# Next Action

## Current Goal

Build the farming loop (M2): plant a seed on a cell, watch it grow in real time,
harvest it into inventory.

## Concrete Task

Add `CropData` (ScriptableObject), `CropInstance` (growth state machine driven by
`DateTime.UtcNow`), and an input-agnostic `FarmActions` layer that `TileInteraction`
dispatches plant/harvest calls into.

## Done So Far

- **M0 — Setup:** Unity 2022.3 LTS + URP 2D, New Input System (Both), Transparency
  Sort Axis `(0, 1, 0)`.
- **M1 — Map & Camera:** isometric 10×10 grid, `GridManager` (world↔cell + occupancy),
  `CameraController` (pan + zoom), `TileInteraction` (hover highlight + click).

## Target Completion

End of Week 2.
