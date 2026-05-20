# Peer Feedback

This file logs feedback received from classmates and instructors during sessions, and how the project responded.

---

## Session 1 — Week 1 Kickoff

### Feedback Received

| From | Feedback | Response |
|------|----------|----------|
| Xiangtian Ren | The original "raid neighbors for loot" framing felt arbitrary — why is the player growing zombies in the first place? Suggested giving the world a clearer reason. | **Adopted.** Rewrote the setting: a zombie virus has overrun the world, and the player runs an experimental farm in a survivor base, growing engineered zombies to reclaim fallen cities. This also makes PvE feel like a natural design choice rather than a scope compromise. |
| Yuzhuo Yuan | Worried that "plant seed → wait → harvest" with only one seed type would get boring fast, even in an MVP. | **Partially adopted.** MVP keeps one seed type to prove the core loop works end-to-end. Added 2–3 engineered zombie strains to the Should-have tier so variety enters as soon as the loop is stable. A strain counter system is in Could-have for later depth. |
| Yixuan Liu | Five weeks looks tight for someone also doing an internship and interview prep. Suggested cutting the auto-battle visuals and just showing a text result. | **Partially adopted.** Kept `BattlePlayer` (animation replay) in Must-have because watching the battle is most of the satisfaction in this kind of game. But explicitly downgraded "polished art" and "particle effects" to Could-have / Cut-first to protect the schedule. |
| Instructor | Pointed out that putting everything in README.md is hard to read and doesn't reflect how real projects organize documentation. | **Adopted.** Split documentation into `/docs` subfiles: `project-direction.md`, `process-plan.md`, `next-action.md`, `asset-credits.md`, `peer-feedback.md`, and a `testing/` folder. README.md is now a short landing page that links into the rest. |

### Key Decisions Made

- **Story-first reframe:** the engineered-zombies-vs-wild-zombies premise replaces the original "raid neighbors" framing, giving the game a clearer purpose.
- **Schedule protection over feature richness:** Must-have stays minimal; anything risking the five-week timeline gets pushed to Could-have or cut.
- **Documentation restructure:** README.md is now a short landing page; full design and process docs live under `/docs`.

---