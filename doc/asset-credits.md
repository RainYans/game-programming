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
cobble, water, river, field), **animated monster strips** (`Resources/MonsterAnim/`,
`Resources/Monsters/`), **strain + UI icons** (`Art/MonsterIcons/`, `Art/UIIcons/`), **decor
prefabs** (`Assets/Art/BattleDecor/`), and the **menu / intro backgrounds** (`Art/Menu/`, composed
from in-game screenshots and Cute Fantasy slices). Each monster strain is mapped to a Cute Fantasy
species (e.g. bomber → Bombschroom, spitter → Skeleton, brute → Slime/Knight, mauler → Orc,
runner → Goblin, shaman → Angel — all present in the packs above).

**Licensing note (important).** Kenmi's Cute Fantasy license states the assets *"can't be resold or
redistributed even if modified,"* and Kenmi has confirmed this includes hosting the files anywhere
others can download them — e.g. a public repository (verified on the itch.io license page, 2026-06).
The Cute Fantasy art — the raw packs **and** everything sliced/derived from them (`Art/CuteFantasy/`,
`Resources/MonsterAnim`, `Resources/Monsters`, `Art/MonsterIcons`, `Art/UIIcons`, `Art/Menu`, the
`Tiles/*Src.png`) — is therefore **kept local and git-ignored, not committed to this public repo**.
The base packs are available at <https://kenmi-art.itch.io/cute-fantasy-rpg>; the playable game may
still be distributed as a **build**. The fonts and audio below are separately licensed (CC0 / OFL /
CC-BY) and **are** included.

## Fonts

| Asset | Source | License | Notes |
|-------|--------|---------|-------|
| **Pixel Operator** | Jayvee Enaguas (HarvettFox96) — https://www.dafont.com/pixel-operator.font | **CC0 1.0** (public domain) | Main UI font (HUD, shop, tutorial, menus, dialogue). Source TTF `Fonts/PixelOperator.ttf`, baked into the TextMesh Pro SDF asset `Fonts/CuteFantasyPixel.asset` |
| **Alagard** | Pix3M / Hewett Tsoi — https://www.dafont.com/alagard.font (also on OpenGameArt) | Free for personal & commercial use, **credit the author** (distributed as CC-BY on OpenGameArt) | Fantasy display font for titles + the storybook intro narration (`Alagard.asset`) |
| TextMesh Pro default (LiberationSans) | Unity built-in | Unity Companion License | TMP fallback glyphs |
| TextMesh Pro example fonts (Roboto, Anton, Bangers, Oswald, Electronic Highway Sign) | Bundled with Unity TextMesh Pro (`TextMesh Pro/Examples & Extras`) | OFL / Apache 2.0 (license files ship beside each font) | Unity sample fonts. **Roboto-Bold** is referenced by one label in `Farm.unity`; the rest only by TMP's own demo scenes |

## Audio

| Asset | Source | License | Notes |
|-------|--------|---------|-------|
| SFX clips (`Resources/SFX/*.ogg`, played via `SfxManager`) | **Ninja Adventure** asset pack — Pixel-Boy & AAA — https://pixel-boy.itch.io/ninja-adventure-asset-pack | CC0 1.0 (audio only; no Ninja Adventure visuals are used) — *confirm on the pack page* | 11 SFX: button click, buy, plant, harvest, dash, hit, death, gate open, item throw, win, lose. **No background music in this build** (SFX only) |

## Code / Packages

| Package | Source | License | Notes |
|---------|--------|---------|-------|
| Universal RP (URP) | Unity Package Manager | Unity Companion License | 2D Renderer + 2D Lights |
| 2D Tilemap Extras | Unity Package Manager | Unity Companion License | RuleTile / AnimatedTile |
| Input System | Unity Package Manager | Unity Companion License | New Input System |
| TextMesh Pro | Unity Package Manager | Unity Companion License | UI text |
| Cinemachine | Unity Package Manager | Unity Companion License | Farm camera follow + confiner |

> Action item before final submission: confirm the exact Cute Fantasy and Ninja Adventure license
> terms on their itch.io pages.
