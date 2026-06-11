# M1 Testing — Avatar Farm & Zombie Foundation

**Status:** M1 features implemented and verified in-editor. Two chores (Git LFS, save
versioning) and one spike (depth sorting) tracked as close-out items.

> This supersedes the M1 section of [week-1.md](week-1.md), which describes the pre-pivot
> setup milestone (map / camera / tile interaction), not this avatar-farm rebuild.

## What Was Tested (verified working)

- Avatar WASD movement with Cinemachine follow camera + scroll-zoom.
- Walk up to empty soil + **E** opens the seed-pick popup; choosing a strain plants it and
  consumes one seed of that strain.
- Walk up to a ripe plot + **E** harvests it.
- Six `ZombieData` strains exist — Brute / Mauler / Runner plantable; Spitter / Shaman /
  Bomber defined but task-locked. Generated via Tools > Monster Farm > Setup Zombie Strains.
- Hunger drifts **Full → Hungry** over idle time (`GameConfig.hungerDelaySeconds`); the
  roamer label flips color/`!` accordingly.
- Save/load persists owned zombies (strain id + hunger timestamp); reload restores them and
  their hunger state.

## What Failed / Changed

- No blocking issues surfaced in M1 play; the readability/sorting items still to confirm are
  tracked under **Open / to confirm** below.

## Open / to confirm

- **Isometric depth sorting** with the avatar overlapping crops / buildings / roamers — confirm
  no wrong-layer pop (M1 P0 spike).
- **Roamer hunger label** is a placeholder TMP; readability (size/position) may need tuning.

## Known follow-ups (not M1 blockers)

- **Git LFS** intentionally skipped — the project's art is 2D and small, so it's not worth the
  overhead; revisit only if binary assets ever grow large.
- **Save versioning** added (`SaveData.version`, current = 1); legacy saves load as version 0.
- **Resolved (M4):** the old prototype roster (`DeployController.zombieRoster` → `BasicZombie`)
  and the unused `BasicSeed` / `BasicZombie` assets — flagged here as the M1→M3 migration gap —
  were all removed in the M4 dead-code cleanup; the real-time battle uses the six strains directly.
