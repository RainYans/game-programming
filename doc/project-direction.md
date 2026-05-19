# Project Direction

## Setting & Story

A zombie virus has swept the world. Most cities have fallen, and humanity has retreated into fortified bases. Scientists in one such base have engineered a breakthrough: **plantable zombies** — a domesticated, controllable strain bred from infected DNA. These engineered zombies are humanity's new weapon. Sent into the ruins, they fight wild zombies on equal footing — flesh against flesh.

The player runs the base's experimental farm. Each crop is a soldier. Each harvest is one step closer to retaking a fallen city.

## Smallest Playable Version (MVP)

One farm screen inside the survivor base. Click tile → plant engineered zombie seed → wait → harvest → send 3 zombies to reclaim a fallen city → earn resources. One seed type, one city stage, gray boxes for art.

## What the Player Does Moment to Moment

1. Pans the camera with mouse drag, zooms with scroll wheel.
2. Hovers over a farm tile in the base, sees a highlight.
3. Clicks an empty tile, picks an engineered zombie seed from a small popup, confirms planting.
4. Watches a growth timer tick down on the tile (real-time, seconds for MVP).
5. Clicks a ripe tile to harvest, sees the zombie added to inventory counter.
6. Repeats planting and harvesting until they have enough units for a mission.
7. Opens the mission map, selects 3 zombies, hits "Deploy" against a fallen city.
8. Watches a short auto-battle play out between engineered zombies and wild zombies, sees mission result and resource reward.
9. Returns to the base with more resources, plants more seeds, advances to the next city.

## Scope Sorting

### Must-have (required for the game to function)
Isometric tilemap, camera controls, plant action, real-time growth, harvest action, inventory, `BattleSimulator`, one playable city mission, mission result + resource reward, JSON save/load.

### Should-have (important for quality, not required for first test)
Hover highlight, seed bank UI, battle replay animations, 2–3 engineered zombie strains, 2–3 city missions, numerical balancing, basic sound effects.

### Could-have (only if the main game works and is tested)
Strain counter system (different engineered strains effective against different wild zombie types), particle effects, ambient base sounds, settings menu, tutorial briefing, mission statistics screen.

### Cut first (remove if scope becomes too large)
PvP, strain crossbreeding, terrain effects across city types, cinematic story scenes, achievements, cloud save, polished art.

## Biggest Risk

Scope creep, not technical difficulty. Solo devs typically spend months on the farm, then months on combat, and ship nothing. Mitigation: PvP cut entirely, v1 is PvE only — players reclaim cities from scripted wild zombie encounters; new ideas go to backlog until the vertical slice is playable. Secondary risk: isometric depth sorting bugs with overlapping multi-tile objects — budget 2–3 days.