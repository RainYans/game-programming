# Next Action

## Current Focus

**MVP gate is hit; the project is now in the v2 ART/VIEW PIVOT — see
[design/direction.md](design/direction.md) (source of truth).** Switching to **top-down
pixel-art**, theme **"Monster Farm"**, art from the single **Ninja Adventure** pack, and
**clear-the-village** combat. A codebase audit (2026-06-06) confirms gameplay logic is
view-agnostic, so this is a **presentation swap + ~6 small code edits + scene rebuild +
reskin**, not a rewrite. Old saves still load.

Order (per `direction.md` phases): **0** pixel pipeline + import Ninja Adventure + a
`StyleTest` spike → **1** view flip (the ~6 edits + farm grid → rectangular + battle layout
→ orthographic + camera/PPU) → **2** full reskin → **3** clear-the-village level(s) +
selected new gameplay (boss / enemy variety / watering) → **4** polish. **New gameplay waits
until the art is in.** Earlier M3-P1 / M2 economy-depth items stay deferred.

### v2 pixel pivot — live progress (handoff 2026-06-06)

**Art LOCKED = Cute Fantasy (Kenmi) ecosystem**, owned at `F:\unity_repo\Yanshuo\像素明亮`
(`Cute_Fantasy` main + `_Characters`/`_Dungeons`/`_UI`/`_MilitaryCamp`/biome packs). Bright 16px
tiles / ~32px chars. Ninja Adventure (`F:\unity_repo\Yanshuo\像素风`) kept ONLY as an audio source
(CF ships no audio). Do NOT mix Ninja visuals (resolution/palette clash). See `design/direction.md`.

**Phase 0 (pixel pipeline + spike): DONE.** Spike subset imported to `Assets/Art/CuteFantasy/`
(Tiles, Chars/Player, Enemies: slimes/skeleton/goblin, UI_Frames), pixel-imported (Point, PPU 16,
uncompressed) and sliced per-frame (Player/Skeleton 32px, Slime_Big 64px, Goblin 48px, FarmLand 16px).
`StyleTest.unity` validated the bright top-down look.

**Phase 1 (view flip): code DONE; Farm.unity DONE.**
- Code: removed iso Y-squash — `AvatarController` (straight XY), `BattleAgent.IsoYScale = 1f`. Compiles
  clean. `GridManager` needed no change (delegates to the Grid).
- `Farm.unity`: `FarmGrid` Isometric→Rectangle (cellSize 1,1,1); `Assets/Tiles/GroundTile.asset` sprite
  → CF `Grass_1_Middle`, `FieldTile.asset` → CF `Path_Middle` (dirt) — IsFarmCell logic intact;
  GroundTilemap cleared + repainted 24×16 grass with a central 6×4 plot; Home/Shop/Lab/WarCamp + Avatar
  repositioned via grid cells; Avatar sprite → CF `Player_0`; Main Camera orthographic + PixelPerfectCamera
  (PPU 16) + transparency sort axis (0,1,0); Cinemachine brain re-enabled. Gameplay wiring preserved.
- Deleted unused `Assets/Art/Ninja`.

**NEXT (Phase 2) — the farm is intentionally bare right now (view-flip only); richness comes here:**
1. **Buildings still Kenney** (clash) → import `Cute_Fantasy/Buildings` (Houses/Tent/Unique), swap the 4.
2. Delete leftover Kenney art `Assets/Art/{Buildings,Props,Tiles}` after references are repointed.
3. Roamer/crop placeholder sprites → CF (slimes/monsters; **slime Small→Med→Big = the 3 growth stages**).
4. **Dress the farm** (CF trees/flowers/paths/fences) to the example-map quality the user wants.
5. **Battle scene flip**: `BattleSceneSetup` iso projection (`CellWorld`, `IsoX/YAngle`, Grid Isometric)
   → orthographic; reskin with CF + `Cute_Fantasy_Dungeons`/`_MilitaryCamp`; build "clear-the-village" levels.
6. **Playtest** Farm (walk / plant / enter buildings) — confirm logic survived the grid/position change.

**Gotchas (save the next session time):**
- MCP is live & authorized: `unity-mcp-cli run-tool <tool> --input-file -` (CLI). Use `script-execute`
  for bulk edit-time C# (author persisted objects — the USER asked: NO runtime scene generation).
  `screenshot-camera` gives a fresh render; `screenshot-game-view` can return a STALE cached frame in edit mode.
- `FarmGrid` transform is offset — position objects/camera via `grid.GetCellCenterWorld(cell)`, not literal coords.
- `PixelPerfectCamera` type lives in assembly **`Unity.2D.PixelPerfect`** (not the URP runtime) — use that in reflection.
- Slice sheets by per-asset frame size; CF chars ~32px, tiles 16px.
- Strain casting is flexible = **distinct species** (slime/goblin/orc/skeleton/bombschroom…), NOT slime recolours;
  strain **ids/data/SaveData unchanged** ("zombie"→"monster" is display-only).

## Workflow (must follow)

- **`main`-direct** — commit straight to `main`. Only branch out for risky/large changes.
- **DoD-first** — for any new chunk, propose acceptance criteria in three levels and wait
  for explicit "go" before writing code:
  - **M (minimum):** mechanic fires, no errors, loop closes; placeholder numbers/visuals.
  - **T (target):** feedback in, balance reasonable, placeholder art acceptable.
  - **P (polish):** edge cases handled, transitions, demoable.
- Chinese chat / English code & commits.
- **MCP-driving the Unity Editor is authorized** (since 2026-06-06): the agent may create
  scenes, import/configure assets, place GameObjects, and screenshot via `unity-mcp-cli`
  (Unity 2022.3.62f3, MCP at `localhost:20858`). The user still does bulk file ops (e.g.
  unzipping asset packs) by hand when given a precise list. Editor one-click menu scripts are
  still fine where cleaner.
- `gh` **is installed and authed** — GitHub actions can use `gh` / `tools/board` directly.

## Done — Milestone 1 (merged)

Foundation:
- WASD avatar with Rigidbody2D + Cinemachine follow + scroll-zoom.
- Walk-up + **E** context interaction (building / plant / harvest).
- Isometric tilemap (Kenney + 256 sheet) and 4 buildings (Home / Shop / Lab / WarCamp).

Zombie data + roster:
- 6 `ZombieData` strains (HP / Attack / MoveSpeed / Melee-vs-Ranged + Passive enum),
  generated by **Setup Zombie Strains** into `Assets/ScriptableObject/Strains/`.
- Per-unit `Inventory` of `ZombieUnit`s (uid + strainId + "became full" timestamp);
  count API preserved.
- Hunger Full ↔ Hungry, time-based via `GameConfig.hungerDelaySeconds`, persisted.

UI / Save:
- Hand-built seed-pick popup (rows clone a template per catalog entry).
- Per-unit `FarmRoamer` keyed by uid, with floating strain-name + hunger color label.
- `SaveData.version = 1` with legacy-count migration; autosave on wallet/inventory/seed change.

## Done — Milestone 3 (merged)

Battle scene:
- Isometric arena (`BattleGrid` + `GroundTilemap`, same config as the farm) + Global Light 2D
  + smoothed camera follow with shake hook. WASD leader reuses `AvatarController`.
- `BattleAgent` real-time AI with priority: Frozen → Flee → Commanded target → Commanded
  move → Default AI (aggro nearest / squad follow leader). Player units render HP bar +
  selection ring.
- **All six passives** implemented in `BattleAgent`: ThickHide, Bloodlust, Evasion,
  Corrosion, Aura, SelfDetonate.
- `BattleManager` orchestrates one city raid; squad survives across stages.

Loop (farm ↔ battle):
- `BattleHandoff` static carrier; `GameConfig.allStrains` + `FindStrain(id)` + `squadCap = 4`.
- Hand-built `DeployPanel` opened at the WarCamp (`UIManager` routes WarCamp → DeployPanel).
- `BattleResultApplier` on Systems applies permadeath + reward on return (deferred one
  frame so it runs after `SaveManager.Load`).
- Farm + Battle in Build Settings.

City structure:
- `MissionData` carries `Stage` (list of `EnemySpawn`) + flat `enemies` as fallback.
- Stages run in order with a **Prep** phase (`Time.timeScale = 0` + panel + Continue).
- `Setup City 1 Stages` populates City1 with 3 mixed stages of wild zombies.
- `WildZombiesSetup` editor menu generates `WildNormal` / `WildRunner` / `WildBrute`
  ZombieData variants.

User-added Slice 4 systems (shipped on main; read the scripts for current behaviour):
- `BattleCommandController` — left-click / box select on player agents, right-click to
  command (focus-fire enemy / move to ground).
- `BattlePauseMenu`, `DamagePopup` (floating numbers), `LeaderDash`, `SfxManager`,
  `BattleGate` (gate / lockdown affordance).

## Next — Milestone 4 (the MVP gate)

The hard target is the 7-line **Definition of Done** in `vision.md`. Cross-check what's
already shipped vs. what's still needed:

- ✅ Walk the avatar; plant and harvest at least one strain. *(M1 — done)*
- ✅ At least **3 starting strains**, each visibly different in battle. *(M1 + M3 — done)*
- ⚠️ Hunger affects combat strength. *(Hunger model is in; combat-side effect is M3 P1 and
  not yet wired — minimal fix: have Hungry units take/deal scaled damage in `BattleAgent`.)*
- ✅ Shop sells seeds and at least one combat item; single currency. *(Shop sells seeds; an
  in-shop **combat-item entry for Rotten Onion** + a "carried items" hook is the remaining
  piece — Onion mechanic exists in battle; needs to be buyable and brought in.)*
- ✅ **One city** with a short sequence of combat stages and a between-stage prep phase.
  *(City1 with 3 stages + Prep — done)*
- ✅ Squad deployment, control-item usage, permadeath, win/lose result, reward. *(All in.)*
- ✅ Save and load the whole state. *(Versioned, with migration — done.)*

**So the MVP gate is one-and-a-half items away:** *(a)* hunger must visibly affect combat;
*(b)* the Rotten Onion must be a real shop item that the player buys + brings in.

In addition, M4 P0 asks for the demo deliverables and a balancing pass:
- **City map** with City 1 selectable (minimal — one node on a map for now).
- **Tiny task / objective hint** UI (steer a new player through plant → harvest → deploy →
  win — even a single ribbon text is enough at M-tier).
- **Core art** for the farm, avatar, and the 6 strains (placeholder is acceptable through
  the MVP gate, but integrating real art is M4 P0 per the roadmap).
- **Balancing pass** on `GameConfig` (battle constants currently live in `BattleAgent`).
- **Record gameplay video + write postmortem** (the actual deliverable for the course).

## Open punch list (carried, not blockers)

- `DeployController` / `BattlePlayer` / the in-farm battle page are **dead code** since the
  flow moved to `DeployPanel` + scene transition. Decide: delete, or leave dormant.
- Isometric depth-sort spike — still deferred until real art lands (placeholder shapes
  aren't a useful look-test).
- Over-hunger downside (starvation vs frenzy) — still undecided.

## Notes / tunables

- **Battle tunables** are constants at the top of `BattleAgent` (aggro range, attack
  interval, passive numbers). Move to `GameConfig` during the balancing pass.
- **Hunger timing:** `GameConfig.hungerDelaySeconds` (set low, e.g. 15s, while testing).
- **Fresh game / money:** `GameConfig.startingResources`; delete the save via
  **Tools > Zombie Farm > Save File** to re-bootstrap.
- **Git LFS:** intentionally skipped — 2D art is small.

## For the next session (Step 0)

Before writing any code, read this file plus `doc/vision.md` (DoD) and `doc/roadmap.md`
(M4), then **read the user-added Slice 4 scripts** to know the real M3 state:

- `Assets/Scripts/BattleCommandController.cs`
- `Assets/Scripts/BattlePauseMenu.cs`
- `Assets/Scripts/DamagePopup.cs`
- `Assets/Scripts/LeaderDash.cs`
- `Assets/Scripts/SfxManager.cs`
- `Assets/Scripts/BattleGate.cs`
- `Assets/Editor/WildZombiesSetup.cs`
- The current `Assets/Scripts/BattleAgent.cs` (heavily expanded since the early version).

Then report back: *(1)* M3 completion per chunk on the M / T / P scale, *(2)* MVP-gate gap
analysis against `vision.md`, *(3)* a DoD draft (M / T / P) for the **first** M4 chunk you'd
recommend, and wait for the user's pick before writing code.
