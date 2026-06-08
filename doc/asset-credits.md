# Asset Credits

Sources and licenses for all third-party assets used in Monster Farm. The game code, design, scene
construction, and the way assets are sliced/combined into RuleTiles, animations, prefabs, and UI are
my own; the items below are the **external** assets.

## Art — Cute Fantasy (Kenmi)

All in-game pixel art (tiles, characters, monsters, buildings, props, and UI frames) comes from the
**Cute Fantasy RPG** asset set and its expansion packs by **Kenmi**.

| Asset | Source | License | Notes |
|-------|--------|---------|-------|
| Cute Fantasy RPG (base pack) | https://kenmi-art.itch.io/cute-fantasy-rpg | Used under the Cute Fantasy itch.io asset license (commercial use permitted under the purchased/free license; raw assets not redistributed) | Tiles, player character, animals, enemies, buildings, outdoor decoration, UI frames/buttons/ribbons, icons, fonts |
| Cute Fantasy expansion packs (Desert, ShroomLands, Volcano, Military Camp, Dungeons, Halloween, Christmas) | Kenmi (same itch.io creator) | Same Cute Fantasy asset license | Extra tiles/props (e.g. lookout tower, fences, rocks) used as battle decor |

Usage in this project: sprites were sliced and recombined into Unity **RuleTiles** (grass, hedge,
stone), **AnimatedTiles** (water), **animated monster strips** (`Resources/MonsterAnim/`), and
**decor prefabs** (`Assets/Art/BattleDecor/`). Each monster strain is mapped to a Cute Fantasy
species. No raw asset files are redistributed outside the built game.

## Fonts

| Asset | Source | License | Notes |
|-------|--------|---------|-------|
| CuteFantasyPixel (TMP SDF) | Generated from the Cute Fantasy UI pixel font (Kenmi) | Cute Fantasy asset license | Bitmap pixel font converted to a TTF and a TextMeshPro **SDF** font asset for crisp UI text *(confirm exact font source on the Cute Fantasy page)* |
| TextMesh Pro default | Unity built-in | Unity Companion License | Fallback |

## Audio

| Asset | Source | License | Notes |
|-------|--------|---------|-------|
| SFX hooks (`SfxManager`, `SfxKind`) | — | — | Sound is wired through `SfxManager`; any external clips used will be listed here with source + license. *(confirm/add clip sources before submission)* |

## Code / Packages

| Package | Source | License | Notes |
|---------|--------|---------|-------|
| Universal RP (URP) | Unity Package Manager | Unity Companion License | 2D Renderer + 2D Lights |
| 2D Tilemap Extras | Unity Package Manager | Unity Companion License | RuleTile / AnimatedTile |
| Input System | Unity Package Manager | Unity Companion License | New Input System |
| TextMesh Pro | Unity Package Manager | Unity Companion License | UI text |
| Cinemachine | Unity Package Manager | Unity Companion License | Farm camera follow + confiner |

> Action item before final submission: confirm the exact Cute Fantasy license tier on the itch.io
> page, and add any audio clip sources here.
