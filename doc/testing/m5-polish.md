# Testing — Final Polish Pass (audio, Lab, collision, tuning, UX)

**Status:** in-editor verification of the final polish sprint. Format: *tested → found → changed*.
Build: editor play, 1920×1080.

## Combat tuning (faster, tighter spreads)

- **Tested:** deployed mixed squads and read the new strain numbers in `GameConfig` / the six
  `Zombie_*.asset` files.
- **Found:** the old spread made the Brute (speed 1.6) lag badly behind the hero while the Runner
  (4.2) shot ahead, and damage ran 1–6 so the Shaman felt useless and the Mauler one-shot things —
  fights dragged and read unevenly in a demo.
- **Changed:** compressed **speed to ~2.2–3.2** (Brute 1.6→2.2, Runner 4.2→3.2, others 2.4→2.7) and
  **attack to ~3–5** (Shaman 1→3, Brute 2→3, Mauler 6→5). Roles preserved, but the squad now keeps up
  and a raid clears quickly enough to demo. Passives unchanged.

## Unit collision (no overlap / no wall-clipping)

- **Tested:** entered the Battle scene in play mode with a test squad; observed spawn + movement.
- **Found (before):** squad units stacked on the same pixel and walked straight through walls (they
  had no colliders by design).
- **Changed:** each `BattleAgent` now gets a small dynamic `Rigidbody2D` + `CircleCollider2D`
  (radius 0.28, high drag) and moves via `rb.MovePosition`. **Verified in play:** units spawn
  **visibly separated**, no exceptions, gameplay runs normally. Walls/hero were already physics
  colliders, so units now stop at / slide along walls.
- **Still to confirm in a full playthrough:** squad funnelling through a doorway (no pathfinding, so
  units slide rather than route) — tune `ColliderRadius`/`BodyDrag` in `BattleAgent` if it bunches.

## Hunger visibility

- **Tested:** deployed a Hungry unit and read the squad HUD.
- **Found:** hunger affected combat but was invisible in battle (only the deploy card colour hinted it).
- **Changed:** the squad HUD row now appends **"(Hungry)"** and tints the name orange for any unit
  deployed Hungry (`BattleAgent.IsHungry`). The mechanic now reads at a glance.

## Lab (strain upgrades)

- **Tested:** the data path — `LabManager.TryUpgrade` → `Wallet.TrySpend` → save → reload → deploy.
- **Expected/built behaviour to confirm in play:** open the **Lab** building (E) → panel lists the six
  strains with level + cost; upgrading spends resources and raises the level (×1.12 HP+ATK per level,
  max 3); the level **persists** across reload (new `SaveData.strainUpgrades`, version 2); a deployed
  upgraded unit has visibly higher HP/ATK. Old (version-1) saves load with no upgrades and no error.

## Audio (BGM)

- **Tested:** placed `MusicManager` (with the three CC0 clips assigned) in MainMenu/Farm/Battle.
- **Expected/built behaviour:** menu/farm/battle each loop their theme and **crossfade** on scene
  change; missing-file is silent (no error); the Esc options sliders change music/SFX volume live and
  persist (`PlayerPrefs`).

## Options / Esc menu

- **Tested:** wired `FarmPauseMenu` (Esc) with Resume / Main Menu / Quit + Music & SFX sliders.
- **Note:** Esc opens the options panel only when no other panel is up (it checks avatar input is
  active), so Esc still closes the shop/deploy/lab panels normally.

## Open items

- Run each Lab/audio case above PASS/FAIL in a full editor playthrough before submission.
- Confirm squad doorway navigation with the new colliders across all four rooms.
