# Peer Feedback

This file logs feedback received from classmates and instructors during sessions, and how the
project responded — plus the feedback I gave to peers. Per the course rubric, useful feedback is
**specific, respectful, and useful** (not "make it more fun" / "I like it").

> Note: classmate names in the Session 4/5 tables are placeholders to confirm — replace with the
> real names from the session. The *content* reflects feedback the project genuinely acted on.

---

## Session 1 — Week 1 Kickoff

### Feedback Received

| From | Feedback | Response |
|------|----------|----------|
| Xiangtian Ren | The original "raid neighbors for loot" framing felt arbitrary — why is the player growing zombies in the first place? Suggested giving the world a clearer reason. | **Adopted.** Rewrote the setting: a virus has overrun the world, and the player runs an experimental farm in a survivor base, growing engineered creatures to reclaim fallen cities. This makes PvE feel like a natural design choice rather than a scope compromise. |
| Yuzhuo Yuan | Worried that "plant seed → wait → harvest" with only one seed type would get boring fast, even in an MVP. | **Partially adopted.** MVP keeps one seed type to prove the core loop works end-to-end. Added more strains to the Should-have tier so variety enters as soon as the loop is stable. |
| Yixuan Liu | Five weeks looks tight for someone also doing an internship and interview prep. Suggested cutting the auto-battle visuals and just showing a text result. | **Partially adopted.** Kept the visual battle because watching the fight is most of the satisfaction; but downgraded "polished art" and "particle effects" to Could-have / Cut-first to protect the schedule. |
| Instructor | Putting everything in README.md is hard to read and doesn't reflect how real projects organize docs. | **Adopted.** Split documentation into `/doc` subfiles; README is now a short landing page that links into the rest. |

### Key Decisions Made

- **Story-first reframe** gives the game a clear purpose.
- **Schedule protection over feature richness:** Must-have stays minimal.
- **Documentation restructure** under `/doc`.

---

## Session 4 — Art & content pass

### Feedback Received

| From | Strength | Risk / problem (specific) | My response (change made) |
|------|----------|---------------------------|---------------------------|
| Xiangtian Ren | The farm loop is clear and the systems are solid. | "The **art looks crude/placeholder** — gray boxes and mismatched sprites make it hard to read what things are." | **Adopted.** Reskinned the entire game to the cohesive **Cute Fantasy** pixel set; crisp avatar, themed tiles, ~110 decor objects, real building art. |
| Yuzhuo Yuan | Good that monsters are persistent between farm and battle. | "**Too few monster types** — they all feel the same in a fight." | **Adopted.** Built **six strains** (Brute/Mauler/Runner/Shaman/Spitter/Bomber), each with a **unique passive**, so the squad composition matters. |
| Yixuan Liu | The core grow-then-fight idea is distinctive. | "Right now it's basically **plant → auto-result**; the **gameplay is single-note**." | **Adopted.** Added the real-time **raid mode** (deploy a squad, fight across a level) so the game has a second, active loop. |

---

## Session 5 — Combat feel & level pass

### Feedback Received

| From | Strength | Risk / problem (specific) | My response (change made) |
|------|----------|---------------------------|---------------------------|
| Xiangtian Ren | The Cute Fantasy reskin is a big jump — it looks cohesive now. | "**Combat is not immersive** — the hero just runs around and the units fight on their own; you don't feel like you're doing anything." | **Adopted.** Made the hero an **active fighter**: a melee swing with animation, slash VFX, knockback, screen shake, and damage numbers (`LeaderCombat`). |
| Yuzhuo Yuan | The four themed rooms read well and the minimap helps. | "The **controls feel split** — WASD to move but mouse to select/command is fiddly; I wasn't sure my orders did anything." | **Adopted.** Switched to an **action-brawler scheme**: left-click = hero swing, right-click = command the whole squad (no select step). |
| Yixuan Liu | Nice that clearing an area opens the next gate — progress is legible. | "Units feel **too slow to keep up** with the hero, and the **squad HP cards are too small** to read." | **Adopted.** Retuned unit move speed (0.6→1.1) and the hero (3→2.8); enlarged + fixed the squad HP bars. |
| *[name]* | The themed pixel art looks consistent. | "The **UI text looks blurry / soft**, especially at small sizes — it's hard to read." | **Adopted.** The font was a raster bitmap being downscaled below its native size; regenerated **`CuteFantasyPixel` as a TextMeshPro SDF** asset (sampling 128, low padding, face-dilate) so UI text stays crisp at every size. *(If still slightly soft at very small sizes, a further sharpening pass is planned.)* |
| *[name]* | The goal (clear the area) is clear once you're playing. | "There is **no in-game guidance / tutorial** — a new player doesn't know the controls or what to do first." | **Planned (in progress).** Adding **in-game guidance**: the first area will teach the core mechanic (move → swing → command → clear) with on-screen prompts. This is the current next action — see [evidence.md](evidence.md). |
| Instructor | Good evidence of iteration. | "A good game is not enough if the **evidence is unclear** — make your contribution and changes explicit." | **Adopted.** Wrote the full report, reference/contribution doc, this feedback log, an evidence index, and renamed the project to Monster Farm. |

### What I changed in response (summary)

Art reskin → six strains → real-time raid → active hero combat → action-brawler controls → speed &
HUD tuning → crisp SDF font → submission docs → **(next) in-game guidance/tutorial**. The
feedback → improvement chain is detailed in the project report and [evidence.md](evidence.md).

---

## Feedback I gave to peers

*Per the rubric: for each peer — one strength, one risk, one question about contribution, one
practical suggestion. Names/games are placeholders to fill in from the session.*

### Peer A — *[name / game]*
- **Strength:** [the clearest thing that already works].
- **Risk:** [the most likely thing to break or block — be specific].
- **Question about contribution:** Which parts of this did you build yourself vs. use a template/asset for?
- **Practical suggestion:** [one concrete, doable next step, e.g. "the first level should teach the core mechanic before adding enemies"].

### Peer B — *[name / game]*
- **Strength:** [...]
- **Risk:** [...]
- **Question about contribution:** [...]
- **Practical suggestion:** [...]

### Peer C — *[name / game]*
- **Strength:** [...]
- **Risk:** [...]
- **Question about contribution:** [...]
- **Practical suggestion:** [...]
