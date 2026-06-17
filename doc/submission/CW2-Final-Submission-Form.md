# Coursework 2 — Final Submission Form

> Source for `2617486_CW2_FinalSubmissionForm.pdf`.

| Field | Value |
|---|---|
| **Student name** | Yanshuo Liu |
| **Student ID** | 2617486 |
| **Game title** | Monster Farm |
| **Unity version** | 2022.3.62f3 (2022.3 LTS) |
| **Build platform** | Windows, Standalone (x86_64) |
| **GitHub repository link** | https://github.com/RainYans/game-programming |
| **Final commit hash** | `e569e4ae825d7e2534cefdac4bfe2bc1aa216269` |
| **Playable build link** | https://github.com/RainYans/game-programming/releases/tag/v1.1 |

---

### How to run the game

**Quickest — play the build.** Download `MonsterFarm_build.zip` from the
[v1.1 Release](https://github.com/RainYans/game-programming/releases/tag/v1.1), unzip it, and run
**`Monster Farm.exe`** (Windows). The build bundles all art, so it plays out of the box. Tuned for
**1920×1080**.

**From source (for code review).** Open the **`MonsterFarm/`** project in **Unity 2022.3 LTS** and
play `Assets/Scenes/MainMenu.unity`. Note: the licensed Cute Fantasy art is git-ignored, so a bare
clone will not render correctly until the packs are re-imported — to simply play, use the build.

### Controls

**Farm:** **WASD** move · **E** plant / harvest / open a building when next to it · **Mouse** for
panels · **Esc** options (Master/Music/SFX volume, key rebinding, How-to-Play, return to menu, quit).

**Raid / Battle:** **WASD** lead the hero · **Left Shift** dash · **Left-click** melee swing toward
the cursor · **Right-click** command the whole squad (focus enemy / move) · **1** Rotten Onion ·
**2** Freeze Canister · **Esc** pause.

### Main game objective

Grow monsters on the farm and build a squad, then lead them into a monster-infested village and
**clear every enemy across its four areas** to reclaim it; win raids to earn resources, then spend
them on seeds, items, and Lab upgrades to field a stronger army.

### Win / lose / completion condition

- **Win:** clear all four areas of the raid (City 1: Farm Outskirts → River + Bridge → Hedge Garden →
  Village Square) — the final Village Square completes the raid.
- **Lose:** the hero's HP reaches 0, or the whole squad is wiped.
- Monsters that die in a raid are **permanently lost** from the roster.

### Main systems / scripts I created

- **Farm:** `AvatarController`, `AvatarInteraction`, `FarmActions`, `GridManager`, `TileInteraction`,
  `CropInstance`, `FarmRoamer` / `FarmRoamerSpawner`, `Building`.
- **Monsters / combat:** `BattleManager`, `BattleAgent` (stats + six passives + hunger),
  `LeaderCombat` / `LeaderCombatant` / `LeaderDash`, `BattleCommandController`, `DeployPanel`,
  `BattleArea` / `BattleGate`, `BattleProjectile`, `BattleResultApplier`, `DamagePopup`,
  `BattleMinimap`.
- **Economy:** `Wallet`, `Inventory` / `SeedInventory` / `ItemInventory`, `ShopController` /
  `ShopPanelUI` / `ItemStore`, `LabManager` / `LabPanel`.
- **Core / data:** `GameConfig`, `ZombieData`, `CropData`, `MissionData`, `CityProgress`,
  `SaveManager`, `BattleHandoff`.
- **UI / presentation / audio:** `UIManager`, `ManualBookController`, `BestiaryPanel`, `CityMapPanel`,
  `MusicManager` / `SfxManager` / `MasterAudio`, `KeyBindings` / `KeyRebindUI`.
- **Onboarding / tutorial:** `FarmTutorialController`, `CombatTutorialController`, `TutorialState`,
  `GroundGuideTrail`, `TutorialBanner`.
- **Menu / intro:** `MainMenuController`, `StoryBookController`.

*(Scripts are organised by system under `MonsterFarm/Assets/Scripts/`. Written with an AI coding
assistant under my direction — see below.)*

### External assets / templates / tutorials / AI used

- **Art:** Cute Fantasy RPG + expansion packs by **Kenmi** (licensed; git-ignored, not redistributed;
  bundled in the build). https://kenmi-art.itch.io/cute-fantasy-rpg
- **Fonts:** Pixel Operator (CC0), Alagard (CC-BY).
- **Audio:** SFX from Ninja Adventure (CC0, audio only); music = three CC0 OpenGameArt tracks.
- **Unity packages:** URP, 2D Tilemap Extras, Input System, TextMesh Pro, Cinemachine.
- **Templates / tutorials:** none — built from scratch; no project starter template or tutorial code was used.
- **Game reference:** Zombie Farm (the "grow fighters on a farm" fantasy only).
- **AI:** an AI coding assistant helped write the C# and stand up basic scene content, under my
  direction; the design, decisions, Unity construction/tuning, debugging, and the finished game are
  mine. Full credits: `doc/asset-credits.md`; AI disclosure: `doc/reference-and-contribution.md`.

### Known issues

- **One fully built raid:** City 1 is complete; cities 2–3 (Thornwood Hollow, Ashen Reach) exist as
  map nodes only (deliberate vertical-slice scope).
- **First-pass balance:** strain/hero numbers tuned by feel, not a formal study.
- **Display:** tuned for 1920×1080; other aspect ratios untested.
- **Accessibility:** no colour-blind palette or text-scale toggle yet (mitigated by text-plus-colour
  state and a large legible font).
- Minor benign console warnings (font/underline); no serious runtime errors in play.
- Save data is local plain JSON (single-player; editable by the player by design).

### Supporting evidence uploaded here

- Final report: `2617486_CW2_FinalReport.pdf`
- Playable build: link above (also `2617486_CW2_FinalGameBuild.zip` if uploaded directly)
- Demo video: `2617486_CW2_DemoVideo.txt` (link)
