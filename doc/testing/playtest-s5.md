# Playtest Note — Session 5 (Battle vertical slice)

*Focus: the action-brawler raid (City1) after the combat rework. Build: editor play, 1920×1080.*

## What I tested

- **Flow:** Farm → deploy at War Camp → enter Battle → fight through the four areas → result → return.
- **Hero combat:** left-click melee swing (arc damage, knockback, slash VFX, swing animation), dash.
- **Squad:** auto-follow + auto-engage; right-click command-all (focus enemy / move).
- **Items:** Rotten Onion (repel), Freeze Canister (freeze).
- **Progression:** area trigger on hero entry → spawn → clear → gate opens → next area; final area = win.
- **Fail state:** hero takes damage from enemies; hero HP 0 → defeat; squad wipe → defeat.
- **HUD:** leader HP bar, squad bars, item slots, minimap.

## Observations / issues found

| Observation | Action taken |
|---|---|
| Hero had no attack and could only run/die → felt like "no presence" | Added `LeaderCombat`: melee swing + slash VFX + knockback; later added swing **animation** frames |
| Attack happened invisibly (auto) → felt cheap | Switched to **manual left-click** swing (auto-attack off by default) |
| Squad + enemies felt **too slow** to keep up with the hero | Unit speed scale 0.6 → 1.1; hero 3 → 2.8 |
| Squad cards rendered tiny regardless of size | Found a leftover **scale 0.114** on the SquadHud container; reset to 1 |
| Drag/selection box didn't line up with the cursor | Canvas had switched to scale-with-screen-size; fixed by converting screen→canvas space |
| Item buttons were unclear | Added onion/ice icons, hotkey + count badges, names; dropped the heavy plaque |
| Bridge looked wrong | The bridge sheet was a tileset placed whole; cropped a clean stone deck instead |
| No end screen | Added a win/lose result card (dim + parchment card + message + Return-to-Farm) |

## Result

The slice now has a clear start, clear goal, working controls, meaningful interaction + decisions
(positioning, when to commit the hero, when to spend items), feedback (HP bars, damage numbers,
shake, slash), and a win/lose condition. Combat reads and feels active rather than passive.

## Still open (next playtests)

- First-pass balance only (hero/strain numbers tuned by feel).
- Squad units can clip walls (no collider by design).
- No tutorial yet — the first area should teach the mechanic (planned next action).
