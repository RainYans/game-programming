# M4 Testing Notes — MVP Gate + Balancing/Cleanup

**Status:** all M4 gameplay cases verified **PASS** in-editor (hunger-in-combat, buyable items,
city-map select + deploy + the cities 2–3 unlock/clear chain), the dead-code + missing-script sweeps are
clean, and the combat-tuning pass is done (see the balancing + movement-fix notes in
[m5-polish.md](m5-polish.md)).

## Scene prerequisites (already set up in the Farm scene)

- The **item shop**: `ItemInventory` is on Systems, with Rotten Onion + Freeze Canister registered
  in `GameConfig.itemCatalog` and the item refs wired.
- The **city map**: `CityMapPanel` + `CityProgress` are built, and the WarCamp routes through the map.

## What to test

### Hunger affects combat (#40) + over-hunger trade-off (#42)
- [x] Deploy a Full unit vs the same strain Hungry (set `GameConfig.hungerDelaySeconds` low,
      e.g. 15, to reach Hungry fast). Hungry unit's damage popups are visibly higher
      (×`hungryDamageMultiplier`, default 1.35).
- [x] The Hungry unit also **takes** more damage (×`hungryDamageTakenMultiplier`, default 1.25)
      — it dies faster, so deploying a starving squad is a real risk, not a free buff.
- [x] Enemies and Full units are unaffected (×1).

### Buyable combat items (#45) + freeze now buyable
- [x] Shop shows **Rotten Onion** and **Freeze Canister** cards with price + "Owned: N".
- [x] Buying decrements wallet, increments owned; reload preserves the count.
- [x] In battle, the onion/freeze HUD counts equal what was carried in (0 if none bought —
      they are no longer free).
- [x] Throwing consumes them; on return to the farm only the **thrown** count is deducted
      (unused ones stay). Quitting mid-raid keeps all (consistent with units surviving).

### City-selection map (#58)
- [x] WarCamp + E opens the map: City 1 "Available" + hint; City 2/3 "Locked".
- [x] Selecting City 1 opens the deploy panel for it; battle runs as before.
- [x] After winning City 1: map shows "Cleared ✓"; **Thornwood Hollow** (City 2) unlocks, then
      **Ashen Reach** (City 3) after it is cleared. Restarting the game preserves cleared state.
      *(Note: cities 2–3 reuse City 1's authored rooms — this checks the unlock/clear progression, not new level content.)*

### Balancing pass (#73)
- [x] `GameConfig > Combat tuning` exposes aggro range, attack interval, reaches, and all six
      passive numbers; editing them changes battle behaviour. Final pass: re-tuned the six strains'
      `moveSpeed` after fixing a framerate-dependent movement bug (see [m5-polish.md](m5-polish.md)).

### Dead-code cleanup
- [x] `DeployController` / `BattlePlayer` / `BattleSimulator` / `BattleUnitView` (scripts + the
      `BattleUnitView`/`BattileUnitView` prefabs + old `BasicZombie` / `BasicSeed` assets) are
      **removed from disk**, and no script references to them remain (verified by search).
- [x] `Tools > Monster Farm > Clean Missing Scripts (open scene)` on the Farm scene — confirmed no
      "missing script" warnings remain on the **Systems** object.

## What failed / changed

- **Bomber self-detonate crashed combat** — `InvalidOperationException: Collection was
  modified` in `BattleAgent.Die`. The SelfDetonate loop iterated the manager's live
  `Enemies`/`Players` list while a blast kill ran `OnAgentDied` → `Remove`, mutating the list
  mid-enumeration. Fixed by iterating a snapshot copy of the foe list. (Pre-existing latent bug,
  surfaced when a Bomber's death blast got a kill.)
