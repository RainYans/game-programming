# M4 Testing Notes — MVP Gate + Balancing/Cleanup

**Status:** in progress. Covers the M4 work that closed the MVP gate (hunger-in-combat,
buyable combat items, city-selection map) plus the balancing pass, the over-hunger trade-off,
and the dead-code cleanup. The dead-code cleanup is **verified** (below); the gameplay cases are
the in-editor test checklist — run each and mark PASS/FAIL before submission.

## Editor setup to run first (Farm scene open, then Ctrl+S)

- `Tools > Monster Farm > Setup Item Shop` — adds `ItemInventory` to Systems, registers Rotten
  Onion + Freeze Canister in `GameConfig.itemCatalog`, wires item refs.
- `Tools > Monster Farm > Setup City Map` — builds `CityMapPanel`, adds `CityProgress`, routes
  the WarCamp through the map.

## What to test

### Hunger affects combat (#40) + over-hunger trade-off (#42)
- [ ] Deploy a Full unit vs the same strain Hungry (set `GameConfig.hungerDelaySeconds` low,
      e.g. 15, to reach Hungry fast). Hungry unit's damage popups are visibly higher
      (×`hungryDamageMultiplier`, default 1.35).
- [ ] The Hungry unit also **takes** more damage (×`hungryDamageTakenMultiplier`, default 1.25)
      — it dies faster, so deploying a starving squad is a real risk, not a free buff.
- [ ] Enemies and Full units are unaffected (×1).

### Buyable combat items (#45) + freeze now buyable
- [ ] Shop shows **Rotten Onion** and **Freeze Canister** cards with price + "Owned: N".
- [ ] Buying decrements wallet, increments owned; reload preserves the count.
- [ ] In battle, the onion/freeze HUD counts equal what was carried in (0 if none bought —
      they are no longer free).
- [ ] Throwing consumes them; on return to the farm only the **thrown** count is deducted
      (unused ones stay). Quitting mid-raid keeps all (consistent with units surviving).

### City-selection map (#58)
- [ ] WarCamp + E opens the map: City 1 "Available" + hint; City 2/3 "Locked".
- [ ] Selecting City 1 opens the deploy panel for it; battle runs as before.
- [ ] After winning City 1: map shows "Cleared ✓"; City 2/3 become clickable but say
      "Coming soon". Restarting the game preserves cleared state.

### Balancing pass (#73)
- [ ] `GameConfig > Combat tuning` exposes aggro range, attack interval, reaches, and all six
      passive numbers; editing them changes battle behaviour (behaviour with defaults is
      identical to before the refactor).

### Dead-code cleanup
- [x] `DeployController` / `BattlePlayer` / `BattleSimulator` / `BattleUnitView` (scripts + the
      `BattleUnitView`/`BattileUnitView` prefabs + old `BasicZombie` / `BasicSeed` assets) are
      **removed from disk**, and no script references to them remain (verified by search).
- [ ] `Tools > Monster Farm > Clean Missing Scripts (open scene)` on the Farm scene — confirm no
      "missing script" warnings remain on the **Systems** object (needs an in-editor pass).

## What failed / changed

- **Bomber self-detonate crashed combat** — `InvalidOperationException: Collection was
  modified` in `BattleAgent.Die`. The SelfDetonate loop iterated the manager's live
  `Enemies`/`Players` list while a blast kill ran `OnAgentDied` → `Remove`, mutating the list
  mid-enumeration. Fixed by iterating a snapshot copy of the foe list. (Pre-existing latent bug,
  surfaced when a Bomber's death blast got a kill.)
