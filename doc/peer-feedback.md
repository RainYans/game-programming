# Peer Feedback

This file logs feedback received from classmates and instructors during sessions, and how the
project responded. Per the course rubric, useful feedback is **specific, respectful, and useful**
(not "make it more fun" / "I like it").

---

## Session 1 — Week 1 Kickoff

### Feedback Received

| From | Feedback | Response |
|------|----------|----------|
| Xiangtian Ren | The original "raid neighbors for loot" framing felt arbitrary — why is the player growing zombies in the first place? Suggested giving the world a clearer reason. | **Adopted.** Rewrote the setting: a virus has overrun the world, and the player runs an experimental farm in a survivor base, growing engineered creatures to reclaim fallen cities. This makes PvE feel like a natural design choice rather than a scope compromise. |
| Yuzhuo Yuan | Worried that "plant seed → wait → harvest" with only one seed type would get boring fast, even in an MVP. | **Partially adopted.** MVP keeps one seed type to prove the core loop works end-to-end. Added more strains to the Should-have tier so variety enters as soon as the loop is stable. |

### Key Decisions Made

- **Story-first reframe** gives the game a clear purpose.
- **Schedule protection over feature richness:** Must-have stays minimal.
- **Documentation restructure** under `/doc`.

---

## Session 4 — Art & content pass

### Feedback Received

| From | Strength | Risk / problem (specific) | My response (change made) |
|------|----------|---------------------------|---------------------------|
| Xiangtian Ren | The farm loop is clear and the systems are solid. | "The **art looks crude/placeholder** — gray boxes and mismatched sprites make it hard to read what things are." | **Adopted.** Reskinned the entire game to the cohesive **Cute Fantasy** pixel set; crisp avatar, themed tiles, real building art. |
| Junfan Zhou | Liked the core idea — raising monsters and then fighting with them — "the concept is interesting." | "The farm still uses **gray-box placeholder art** and has **too few decorations**, so the space feels empty / unfinished." | **Adopted.** On top of the Cute Fantasy reskin, scattered **~110 decoration objects** (trees, fences, props) across the farm so it reads as a real place, not an empty grid. |
| Yuzhuo Yuan | Good that monsters are persistent between farm and battle. | "**Too few monster types** — they all feel the same in a fight." | **Adopted.** Built **six strains** (Brute / Mauler / Runner / Shaman / Spitter / Bomber), each with a **unique passive**, so squad composition matters. |
| Yanbin Xu | Found the grow-then-fight concept genuinely interesting. | "The **gameplay is too single-note** — there isn't enough variety in what the player actually does." | **Adopted.** Added depth on two fronts: **six strains + a hunger risk system** (preparation depth) and a **real-time raid mode** (a second, active loop), so the game isn't just plant-and-wait. |
| Yixuan Liu | The core grow-then-fight idea is distinctive. | "Right now it's basically **plant → auto-result**; the moment-to-moment gameplay is passive." | **Adopted.** Added the real-time **raid mode** (deploy a squad, fight across a level) so there's an active loop, not just an auto-result. |

---

## Session 5 — Combat feel & level pass

### Feedback Received

| From | Strength | Risk / problem (specific) | My response (change made) |
|------|----------|---------------------------|---------------------------|
| Xiangtian Ren | The Cute Fantasy reskin is a big jump — it looks cohesive now. | "**Combat is not immersive** — the hero just runs around and the units fight on their own; you don't feel like you're doing anything." | **Adopted.** Made the hero an **active fighter**: a melee swing with animation, slash VFX, knockback, screen shake, and damage numbers (`LeaderCombat`). |
| Yuzhuo Yuan | The four themed rooms read well and the minimap helps. | "The **controls feel split** — WASD to move but mouse to select/command is fiddly; I wasn't sure my orders did anything." | **Adopted.** Switched to an **action-brawler scheme**: left-click = hero swing, right-click = command the whole squad (no select step). |
| Jingyu Pan | Nice that clearing an area opens the next gate — progress is legible. | "Units feel **too slow to keep up** with the hero, and the **squad HP cards are too small** to read." | **Adopted.** Retuned unit move speed (0.6→1.1) and the hero (3→2.8); enlarged + fixed the squad HP bars. |
| Yajing Xu | The "clear the area" goal is clear once you're playing. | "There is **no in-game guidance / tutorial** — a new player doesn't know the controls or what to do first." | **Adopted (built).** Added first-launch **farm onboarding** (`FarmTutorialController`) and a dedicated **combat tutorial scene** (`Tutorial.unity` / `CombatTutorialController`) that teaches move → dash → attack → command → throw-item before the first real raid. |
| Instructor | Good evidence of iteration. | "A good game is not enough if the **evidence is unclear** — make your contribution and changes explicit." | **Adopted.** Wrote the reference/contribution doc, this feedback log, and an evidence index, and renamed the project to Monster Farm. |

### What I changed in response (summary)

Art reskin + ~110 decorations → six strains → real-time raid → active hero combat → action-brawler
controls → speed & HUD tuning → in-game onboarding + dedicated combat tutorial → readable fonts
(Pixel Operator for UI, Alagard for titles) → submission docs. The feedback → improvement chain is
detailed in [evidence.md](evidence.md).
