# Coursework 2 — Final Report

**Game:** Monster Farm  ·  **Student:** Yanshuo Liu  ·  **Student ID:** 2617486
**Engine:** Unity 2022.3.62f3 (URP 2D, New Input System)  ·  **Platform:** Windows (Standalone)
**Repository:** https://github.com/RainYans/game-programming
**Playable build:** https://github.com/RainYans/game-programming/releases/tag/v1.1
**Final commit:** `e569e4ae825d7e2534cefdac4bfe2bc1aa216269`

> Monster Farm is a top-down 2D pixel game that fuses a cozy farm-management loop with a hands-on
> real-time raid: grow monsters on your farm, raise them, then lead a squad of them into a village
> and clear it. The two modes share one squad and one save. This report explains the design and
> technical decisions, the problems and limitations, how testing changed the game, how it developed
> from concept to final, my personal contribution, and the use of external assets and AI.

---

## 1. Design choices

**Two connected modes over one squad.** The central design bet is that a calm farm loop and a tense
action raid reinforce each other when they share the *same units and save*: the farm exists to
prepare a squad, and the raid gives the farm stakes. The whole experience is built to make that link
felt rather than stated.

**A single strong mechanic: grow-then-fight, gated by hunger and permadeath.** Rather than a wide
shallow feature set, the game centres one mechanic — *preparing monsters you are willing to risk*.
Three design systems serve it:

- **Six strains with one passive each** (Brute–Thick Hide, Mauler–Bloodlust, Runner–Evasion,
  Spitter–Corrosion, Shaman–healing Aura, Bomber–Self-Detonate) so squad composition matters but the
  rule per unit stays readable.
- **Hunger** — a Hungry unit hits harder but is more fragile — so "when do I deploy?" is a decision.
- **Permadeath** — a unit lost in a raid is gone from the roster — so deployment carries real weight.

**Combat designed for agency through positioning, not micro.** The squad auto-follows and
auto-engages; the player's job is to lead, swing in alongside, command focus-fire, and spend items
at the right moment. This keeps the game casual to hold while leaving room to play well.

**Readable, forgiving structure.** A raid is a short **four-room linear level** (Farm Outskirts →
River + Bridge → Hedge Garden → Village Square) with **area-gated** progression and a minimap, so
progress is always legible. The farm has no timers or death; only the units the player deliberately
risks can be lost.

## 2. Technical decisions

**Engine & rendering.** Unity 2022.3 LTS with the **URP 2D Renderer**. The view is **top-down
orthographic** with **Y-sorting** via a Transparency Sort custom axis `(0,1,0)` so taller objects
overlap correctly; a pixel pipeline (Point filtering, consistent PPU, 2D Pixel-Perfect Camera) keeps
the art crisp.

**Data-driven, single source of truth for balance.** Strains, crops, and missions are
ScriptableObjects, and **every tunable number lives in one `GameConfig` asset**, so balancing happens
in one place instead of being scattered through code.

**Persistence.** `SaveManager` serialises state to JSON in `persistentDataPath`, autosaves on change,
and is **versioned + additive** — loading is defensive, so a corrupt or older save falls back to
fresh-save defaults instead of crashing.

**Architecture chosen for low-risk change.** Farm actions were kept **input-agnostic**, which let the
project pivot from the original isometric/click model to a top-down WASD avatar with only a handful of
localised edits rather than a rewrite. The combat layer was later rebuilt from a deterministic
simulate-and-replay prototype into a real-time `BattleManager` / `BattleAgent` scene.

**Game systems exercised (mapped to the brief):**

- **Input** — New Input System drives both the farm avatar and combat (WASD, E, mouse, Shift,
  left/right click, number keys); keys are **rebindable** and persisted.
- **Game logic** — strain stats + six passives, hunger, crop growth timers, economy, area-gated
  level flow, win/lose resolution.
- **Interaction** — proximity-based walk-up planting/harvesting and building panels.
- **Audio** — per-scene background music with crossfades (`MusicManager`), a full SFX set
  (`SfxManager`), and Master/Music/SFX volume sliders (`MasterAudio`).
- **Animation** — animated monster strips (idle/walk/attack/death), avatar walk frames, animated
  water and decor.
- **Physics & collision** — squad units carry a Rigidbody2D + collider and respect walls; in the
  final pass combatants **pass through each other** and use **boids-style separation** so they spread
  out instead of stacking; melee is a visual jab that no longer shoves bodies.
- **UI** — a parchment/pixel HUD: squad HP cards, item slots with icons + hotkeys, leader HP bar,
  framed minimap, result screen, shop/lab/deploy/bestiary panels, and a turning **How-to-Play manual**.
- **AI** — enemies and the squad acquire targets and auto-engage; the squad loosely follows the
  leader and responds to focus-fire/move commands.

**Framerate correctness (final pass).** Monster movement was moved from a per-frame position update to
velocity / `FixedUpdate` physics so units move at a **consistent speed regardless of framerate** (they
previously crawled at high FPS).

## 3. Problems and limitations

This was a solo project on a tight timeline, so scope was actively managed and several planned
features were **deliberately cut to keep quality high** rather than shipped half-built. These are
intentional scope decisions, not abandoned work:

- **Cut:** the task/achievement system, plot expansion, the branching city map (made **linear**), the
  RTS box-select control, one combat item (Barbed Wire), and two consumables (Fertilizer, Hunger
  Tonic). The eating-drift hunger model was simplified to **hunger set at deploy time**.
- **Cities 2–3** (Thornwood Hollow, Ashen Reach) exist as **map nodes** but only **City 1 is a fully
  built raid** — for a vertical slice, one polished level conveys the experience; the rest are planned
  to follow.
- **Balance** is first-pass — strain and hero numbers were tuned by feel through playtests, not a
  formal balancing study.
- **Accessibility gaps:** no colour-blind palette or text-scale toggle (mitigated by a text-plus-colour
  rule and a large legible font); the UI is **tuned for 1920×1080** and other aspect ratios are
  untested.
- Minor benign console warnings (font/underline) remain; no serious runtime errors in play.

The "plan vs. built" differences are tracked openly in `doc/evidence.md` and the design bible carries
build-notes where the shipped game diverges from the original design.

**Known limitations (and why they're acceptable for a vertical slice)**

- *Scope.* Only City 1 is a fully built raid; cities 2–3 exist as map nodes. The task system, plot
  expansion, the branching map, and a third combat item were planned but **cut as tier-P1 features**
  to keep the shipped systems polished.
- *Balance.* Strain and hero numbers were tuned by playtest feel rather than a formal study; every
  value lives in `GameConfig`, so a later balance pass is data-only.
- *Platform & display.* A Windows standalone build laid out for 1920×1080; other aspect ratios are
  untested and there is no gamepad support.
- *Accessibility.* No colour-blind palette or text-scaling option yet — mitigated by text-plus-colour
  state (e.g. "(Hungry)") and a large legible font.
- *Persistence.* A single local JSON save the player can edit — acceptable for an offline
  single-player game, and loading falls back to defaults if the file is corrupt.

## 4. Testing and what changed because of testing

Testing was **manual in-editor verification plus structured peer playtests** each session, logged
under `doc/testing/` and `doc/peer-feedback.md`. Feedback drove the largest improvements in the
project — the design changed *because* of it:

| Feedback (specific) | Change made |
|---|---|
| "The art looks crude/placeholder; hard to read what things are." | Reskinned the entire game to the cohesive **Cute Fantasy** set; added ~110 farm decorations so the space reads as a real place. |
| "Too few monster types — they all feel the same." | Built **six strains, each with a unique passive**, so composition matters. |
| "It's basically plant → auto-result; moment-to-moment is passive." | Added the **real-time raid mode** — an active second loop. |
| "Combat isn't immersive — the hero just runs around." | Made the hero an **active fighter**: melee swing, slash VFX, knockback, screen shake, damage numbers. |
| "Controls feel split — WASD move but mouse select/command is fiddly." | Switched to an **action-brawler** scheme: left-click = swing, right-click = command the whole squad (no select step). |
| "Units too slow to keep up; squad HP cards too small to read." | Retuned move speeds (unit 0.6→1.1, hero 3→2.8); fixed a leftover 0.114 scale bug shrinking the HP cards; fixed screen→canvas coordinate mapping. |
| "No in-game guidance — a new player doesn't know what to do." | Added first-launch **farm onboarding**, a dedicated **combat tutorial scene**, and a turning **How-to-Play manual**. |
| Units could clip/overlap walls and each other. | Gave each unit a Rigidbody2D + collider; later made combatants pass through allies and separate (boids) so they stop stacking. |

A concrete bug-fix example from the Session-5 playtest log: the squad HP cards rendered tiny
regardless of their size setting — the cause was a stray `scale 0.114` on the HUD container, found and
reset to 1. This is the kind of "test → diagnose → fix" loop the testing notes capture.

## 5. Reflection — concept to final version

The project started as a **"zombie farm" planting loop** with placeholder/mixed art and an isometric
view. Three turning points shaped the final game, each driven by a decision or by testing:

1. **Art & theme pivot** — adopting the cohesive Cute Fantasy set and re-theming "Zombie Farm" →
   "Monster Farm", and moving from isometric to top-down. Because gameplay logic was view-agnostic,
   this was a presentation swap plus a few localised edits, not a rewrite.
2. **Combat rebuild** — the biggest change. Playtesters found the early auto-resolve/RTS-micro combat
   passive and fiddly, so it was redesigned into a **hands-on action-brawler** with a fighting hero
   and command-the-whole-squad controls. This is the clearest case of the game improving *because of*
   feedback, not despite it.
3. **Onboarding & polish** — adding audio, a Lab, a Bestiary, key rebinding, unit collision, a boss,
   a city map, framerate-correct movement, and a full How-to-Play manual + tutorials so a new player
   can actually learn the game.

What I would do next, given more time: build cities 2–3 as full raids, run a proper balancing study,
and add a colour-blind/text-scale accessibility pass. The core loop, though, is complete and stable —
the project hit its MVP gate and was then polished, which was the plan from the start.

## 6. Personal contribution

This is a **solo project**. The **design and direction are entirely mine** — the concept, the
two-mode loop, the six strains and their passives, the hunger and permadeath systems, the combat feel,
and the level layout. I **designed every scene** (the farm and the four-room raid) — what each
contains and how the rooms escalate — built a basic version, then **reworked and hand-tuned them
myself** (layout, spacing, sizing, and many rounds of fine-tuning are manual work). I ran the whole
**testing-and-iteration loop** off playtests and peer feedback, and managed the project on GitHub
(commit history, milestones, Kanban board, testing logs, and the design bible under `doc/`).

For the C#, I worked with an **AI coding assistant under my direction**: I decided what each system
needed and how it should be structured, then reviewed, modified, debugged, and integrated the code and
iterated it from playtests (see §7).

## 7. Use of templates, assets, tutorials and AI

**External creative assets** (all licensed and credited in `doc/asset-credits.md`):

- **Art** — Cute Fantasy RPG and its expansion packs by **Kenmi** (purchased/licensed). The licence
  forbids redistributing the files, so the art is **git-ignored and excluded from the public source
  tree**; the playable game is distributed as a **build** in Releases. Sprites were sliced/recombined
  by me into RuleTiles, animated monster strips, decor prefabs, and a parchment HUD.
- **Fonts** — Pixel Operator (CC0, UI) and Alagard (CC-BY, titles).
- **Audio** — SFX from the Ninja Adventure pack (CC0, audio only) and three CC0 OpenGameArt music
  tracks; no Ninja Adventure visuals are used.
- **Unity packages** — URP, 2D Tilemap Extras, Input System, TextMesh Pro, Cinemachine (stock).

**Templates / tutorials:** none — the game is built from scratch. No project starter template or
tutorial code was copied in; only the stock Unity packages above are used.

**Game reference:** one — **Zombie Farm**, for the "grow your fighters on a farm" fantasy only;
everything else was designed by iterating on what felt fun, drawing on broadly familiar genre
conventions rather than any single title.

**Use of AI (disclosed):** an AI coding assistant helped turn my design decisions into C# and to stand
up basic scene content faster. The substantive work is in **directing** it — deciding what to build,
judging whether the result is right, fixing it when it isn't, integrating the pieces, and iterating
from playtests. AI accelerated *writing the code*; the design, the architecture decisions, the Unity
construction and hand-tuning, the debugging, and the finished game are mine. A fuller account is in
`doc/reference-and-contribution.md`.

**Legal, ethical, social, accessibility & security.** All third-party assets are licensed and credited
(`doc/asset-credits.md`); the licensed Cute Fantasy art is kept out of the public repository per its
licence, and the use of AI is disclosed above — the project's *legal/ethical* footing. *Socially*, the
tone is deliberately cozy and non-graphic: no gore, no human-on-human violence, no sexual content, and
no real-world group is targeted, with no microtransactions, loot boxes, gambling, or dark patterns. For
*accessibility*, input is simple and conventional with **full key rebinding** and independent
**Master/Music/SFX** volume sliders, state is shown as **text + colour** (e.g. "(Hungry)") rather than
colour alone, and onboarding plus a tutorial teach the controls — the known gaps are no colour-blind
palette or text-scaling toggle and a 1920×1080-tuned UI. On *security*, the game is offline and
single-player with **no network, accounts, telemetry, or personal data**; the only state is a local
JSON save loaded defensively (a corrupt file falls back to defaults). The full account is in
`doc/accessibility.md`.

## 8. Professionalism — organisation, time management, and process

**Organisation.** The whole project lives in one GitHub repository with a readable `doc/` structure:
a design bible (`doc/design/`), per-milestone testing logs (`doc/testing/`), an evidence index
(`doc/evidence.md`), a peer-feedback log (`doc/peer-feedback.md`), and asset credits. Work was tracked
on a GitHub Project (Kanban) board across four weekly milestones, commits follow Conventional Commits
in English, and two playable builds are tagged on GitHub Releases (v1.0, v1.1) with a top-level
`CHANGELOG.md`.

**Version control.** The workflow began on short-lived `feature/*` branches with pull requests
(PRs #1, #18–#24) while the foundation was still risky, then moved to a **main-direct** workflow once
the base was stable (2026-06-04, recorded in `doc/process.md`) — a deliberate decision for a solo
project, not an absence of discipline.

**Time management.** A four-week plan split into hard **P0 / P1 / P2** tiers with a fixed MVP gate:
the must-have core was built and tested first, then should- and could-have features were layered on
top. When time ran short, the slice was protected by **cutting P1 features rather than shipping them
half-built** — the cut list and reasons are in `doc/roadmap.md` and `doc/evidence.md`.

**Responding to feedback.** Three peer-review sessions each produced concrete, traceable changes — the
story reframe (Session 1), the cohesive art reskin + six strains + a real-time raid mode (Session 4),
and the action-brawler control switch + speed/HUD tuning + an in-game tutorial (Session 5). The full
feedback → change chain is in `doc/peer-feedback.md`.

**Independent work and honesty.** This is a solo project; the design, decisions, scene construction and
testing are my own; AI-assisted implementation is disclosed (§7); and the licensed art is kept out of
the public repository per its licence.

**Honest self-assessment.** My weakest area was **testing rigour**. Verification was manual, in-editor
playtesting, and combat balance was tuned by feel rather than through a formal balancing study or any
automated tests. This caught the issues that mattered — the bugs and feel problems all surfaced in play
and in peer sessions — but it leaves regressions easy to reintroduce and balance hard to reason about
precisely. With more time I would add a small set of automated checks for the core systems (save/load
round-trips, crop growth, combat resolution) and run a structured balance pass over the `GameConfig`
values instead of tuning by hand.

---

### Where to find the evidence

- **Play it:** the Windows build in [Releases](https://github.com/RainYans/game-programming/releases/tag/v1.1).
- **Process & change log:** `doc/evidence.md`, `doc/peer-feedback.md`, `doc/testing/`, and the GitHub
  commit history.
- **Design bible:** `doc/vision.md`, `doc/design/`, `doc/roadmap.md`.
- **Credits & ethics:** `doc/asset-credits.md`, `doc/accessibility.md`,
  `doc/reference-and-contribution.md`.
