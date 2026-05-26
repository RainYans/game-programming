# Next Action

## Current Focus

**Milestone 1 — Avatar Farm & Zombie Foundation.** The farm *scene* is built and playable
with placeholder/early art. We're mid **art pass** (dropping in real isometric tiles); after
that, build the zombie data + hunger model and move toward combat.

## Done so far (farm scene)

- **Avatar:** WASD movement via Rigidbody2D (collides with solid objects); `AvatarController`
  + `AvatarInteraction`.
- **Camera:** Cinemachine 2.9.7 — `FollowCamera` follows the avatar (Framing Transposer) with
  `CinemachineConfiner2D` bounded by a `CameraBounds` PolygonCollider2D; `CameraController`
  drives scroll-zoom via the vcam lens. (Do NOT scale the FarmGrid transform — it desyncs
  `WorldToCell`; change cell look via zoom or the Grid Cell Size + tile PPU instead.)
- **Interaction:** walk up + **E** is context-sensitive — open the nearest building, else
  plant/harvest the field cell underfoot. Mouse interaction removed (`TileInteraction` inert).
- **Map:** isometric `GroundTilemap` rebuilt via script (≈20×20 ground, centered ~6×6
  plantable field; rest is open ground for buildings + landscaping). Can also be hand-painted
  with the Tile Palette.
- **Roaming zombies:** harvested zombies spawn as wandering `FarmRoamer` units, synced to the
  `Inventory` by `FarmRoamerSpawner`.
- **Buildings (placeholders, walk-up + E):** **Home** (manual save), **Shop** (real shop
  panel), **Lab** (placeholder toast), **WarCamp** (opens deploy/battle page; future city-map
  staging point). `Building` marker + `BuildingType` enum.
- **Scenery (placeholders, with collision):** wall ring, mountains; invisible `MapBoundary`
  contains the avatar. River is a painted `RiverTile`.
- **Art (in progress):** importing the Kenney *Isometric Landscape* tiles and an *Isometric
  Assets* 256px sheet; `GroundTile`/`FieldTile`/`RiverTile` point at real iso tiles via the
  tile setup scripts. Remaining tile nit: a possible color shift from texture compression
  (set the sheet's Compression = None for true colors).

## Editor tools (Tools > Zombie Farm)

Scene setup is repeatable via menus: **Setup Avatar**, **Setup Cinemachine Camera**, **Rebuild
Farm Map**, **Setup Buildings**, **Setup Scenery (Placeholder)**, **Setup Farm Boundary**,
**Apply Ground & Field Tiles** (Kenney), **Apply Farm Sheet Tiles** (256 sheet). Scripts live
in `ZombieFarm/Assets/Editor/`.

## Next

1. Finish the tile art pass (lock the ground/field/river look; fix compression color if needed).
2. Bring in avatar / building / zombie art (transparent PNG, bottom-center pivot).
3. **Then gameplay:** six `ZombieData` strains (stats + passive flags), the hunger state
   machine (Full ↔ Hungry, persisted), seed-pick popup on planting.

## Open decisions

- Over-hunger downside: starvation (lose HP) or frenzy? (See design/zombies.md.)
- Squad-size cap for deployment (~3–4).
- Whether the river should block the avatar (needs a separate collider tilemap).

## Notes / tunables

- Map size: `FarmMapSetup.cs` (`GroundSize`/`FieldSize`).
- Building positions: `BuildingsSetup.cs` (cell consts); open range: `AvatarInteraction.Building Reach`.
- Tile fit: PPU in the tile setup scripts (gaps = lower PPU); vertical offset = GroundTilemap Tile Anchor.
- Git LFS still not set up — do it before bulk binary art grows the repo.
