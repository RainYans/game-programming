# Coursework 3 — Professionalism Portfolio

> Source for `2617486_CW3_ProfessionalismPortfolio.pdf`.

**Student:** Yanshuo Liu · **Student ID:** 2617486 · **Module:** Games Programming
**Game:** Monster Farm · **Engine:** Unity 2022.3.62f3 (URP 2D, New Input System)

---

## 1. GitHub repository link

**https://github.com/RainYans/game-programming** (public; default branch `main`).

The game is in **`MonsterFarm/`**. The repository is a course monorepo and also contains two earlier
in-class activities (`2D_Game_Improvement/`, `SolarSystem/`) that are **not** part of this submission.
Process and design records live under [`doc/`](https://github.com/RainYans/game-programming/tree/main/doc).

## 2. Development log (progress over time)

Development ran steadily from mid-May to mid-June across four weekly milestones (M1–M4), not in a
last-minute burst. Each milestone shipped a tested slice before the next began.

| Date (2026) | Stage | What was built |
|---|---|---|
| May 19–20 | Plan + M-W1 | README, scope sort (MoSCoW), Week-1 testing log; isometric farm scaffold (grid, camera, tile interaction) |
| May 20–22 | M2 economy | Farming loop (plant → grow → harvest → inventory); Shop & buy UI; building interaction; **save system**; on-screen counters |
| May 25–27 | Re-scope + M1 | Design docs restructured for the two-mode game; six **strains** + **hunger** + per-unit roamers; farm rebuilt around a **walking avatar** (top-down, Cinemachine); real-time battle core (slice 1) |
| Jun 4 | M3/M4 combat | Combat — deploy, stages, **permadeath**, the six passives, items, mouse control; hunger-in-combat; city-selection map; tuning consolidated in `GameConfig`; **switched to main-direct workflow** |
| Jun 8 | Raid + theme | Battle **rebuilt into a four-room action-brawler raid**; project renamed **Zombie Farm → Monster Farm**; cohesive **Cute Fantasy** art reskin; first submission docs |
| Jun 10–12 | Front-end + polish | Main-menu → storybook intro → farm flow; onboarding + combat tutorial; **audio, Lab, Bestiary, key-rebinding, unit collision, a boss, city map**; framerate-correct movement → **v1.0 release** |
| Jun 17 | Onboarding + feel | How-to-Play manual, ground-trail onboarding, combat-feel polish → **v1.1 release** |
| Jun 18 | Finalise | Submission docs, CHANGELOG, licence scope note |

Two tagged builds on GitHub Releases (**v1.0**, **v1.1**) and a top-level `CHANGELOG.md` record the
release history. Full detail: the GitHub commit history + [`doc/roadmap.md`](../roadmap.md).

## 3. Summary of important commits

| Commit | Date | Why it matters |
|---|---|---|
| `ab0051e` | May 20 | Week-1 foundation — isometric farm grid, camera, tile interaction |
| `a13cba6` | May 20 | Core farming loop (plant → grow → harvest → inventory) |
| `9935050` | May 22 | Shop & buy UI, building interaction, **save system**, inventories |
| `06fb824` | May 26 | Farm rebuilt around a walking **avatar** + Cinemachine camera (view pivot) |
| `2db2c7f` | May 27 | Six **strains**, **hunger**, seed-pick popup, per-unit roamers |
| `cd30b4c` | Jun 4 | Combat — deploy, stages/prep, **permadeath**, passives, items, mouse control |
| `1c8f604` | Jun 4 | Combat tuning into `GameConfig`, over-hunger trade-off; **fixed bomber detonate crash**; pruned dead prototype code |
| `5d7fd34` | Jun 4 | Process change: feature-branch → **main-direct** workflow (recorded in `process.md`) |
| `7b86b70` | Jun 8 | Battle **rebuilt into a four-room raid**; project renamed to **MonsterFarm** |
| `bdb8a79` | Jun 10 | Front-end flow (main menu, story intro, onboarding + combat tutorial) |
| `11ac812` | Jun 11 | Tightened asset citation/licensing; **untracked licensed art** from the public tree |
| `8cb0c97` | Jun 12 | Final polish — audio / Lab / key-rebinding / Bestiary / collision; scripts reorganised into system folders |
| `8381ce3` | Jun 12 | **Framerate-independent** monster movement → tagged **v1.0** |
| `e569e4a` | Jun 17 | How-to-Play manual + ground-trail onboarding + combat polish → tagged **v1.1** (the build commit) |

## 4. Evidence of planning and task management

- **MoSCoW + tiered scope.** Features were sorted Must / Should / Could / Cut-first, and every chunk
  was given hard **P0 / P1 / P2** acceptance tiers with a fixed MVP gate, so "done" meant a tier
  chosen up front, not a moving target ([`doc/roadmap.md`](../roadmap.md),
  [`doc/process.md`](../process.md) "Definition of Done — Per-Chunk Tiers").
- **Four weekly milestones** (M1 Avatar/Monsters, M2 Economy/Lab, M3 Combat, M4 Progression/Polish),
  with combat deliberately scoped as the heaviest week and a thin P0.
- **GitHub Project (Kanban)** board (`Backlog → Ready → In progress → In review → Done`) with Stories,
  Tasks and milestones ([`doc/backlog.md`](../backlog.md), [`doc/process.md`](../process.md)).
- **Scope protected by cutting, not half-shipping.** When time ran short, P1 features (task system,
  plot expansion, branching map, extra items) were cut to keep the shipped systems polished — the cut
  list and reasons are in [`doc/roadmap.md`](../roadmap.md) and [`doc/evidence.md`](../evidence.md).

## 5. Evidence of response to feedback

Three peer-review sessions, each producing concrete, traceable changes
([`doc/peer-feedback.md`](../peer-feedback.md)):

| Session | Feedback | What I changed |
|---|---|---|
| **S1 — Week-1 kickoff** | Setting/story unclear | Reframed the world and direction (story-first) |
| **S4 — Art & content** | "Art looks crude/placeholder; too few monster types; farm feels empty" | Reskinned the whole game to the cohesive **Cute Fantasy** set; built **six strains with unique passives**; scattered ~110 farm decorations; added the **real-time raid** mode |
| **S5 — Combat feel & level** | "Combat passive/fiddly; WASD+mouse-select split; units too slow; HP cards too small; no guidance" | Switched to an **action-brawler** scheme (left-click swing / right-click command-all, no select step); retuned speeds (unit 0.6→1.1, hero 3→2.8); fixed a `scale 0.114` HUD bug; added onboarding + a combat tutorial |

The combat rebuild (S5) is the clearest case of the game improving *because of* feedback, not despite it.

## 6. Testing log and bug-fixing evidence

Testing was **manual in-editor verification + structured peer playtests**, logged per milestone under
[`doc/testing/`](../testing/). Representative **test → diagnose → fix** loops:

| Log | Symptom | Diagnosis → Fix |
|---|---|---|
| `testing/week-1.md` | Tile highlight never appeared | `ScreenToWorldPoint` left `z = -10`, breaking `WorldToCell` → zero the world `z` |
| `testing/m4-mvp-gate.md` | Bomber self-detonate threw "Collection was modified" | Mutating the live agent list mid-enumeration → iterate over a snapshot copy |
| `testing/m5-polish.md` | Monsters crawled *worse* at high FPS | `MovePosition` driven from `Update()` overwrote the target between physics steps → switch to velocity / `FixedUpdate` |
| `testing/playtest-s5.md` | Squad HP cards rendered tiny | A leftover `scale 0.114` on the SquadHud container → reset to 1 |

Build-readiness was smoke-tested from a fresh save (all five scenes wired in order; no serious console
errors — only benign font warnings); the checklist is in [`doc/evidence.md`](../evidence.md).

## 7. Screenshots / short evidence of progress over time

- In-repo screenshots: [`doc/screenshots/farm.png`](../screenshots/farm.png),
  [`doc/screenshots/battle.png`](../screenshots/battle.png) (also shown in the README).
- Two tagged release builds you can run: **[v1.0](https://github.com/RainYans/game-programming/releases/tag/v1.0)**
  and **[v1.1](https://github.com/RainYans/game-programming/releases/tag/v1.1)**.
- A narrated gameplay demo video (see the CW2 `2617486_CW2_DemoVideo.txt`).
- The clearest progress evidence is the **commit history itself** — idea → features → problems solved
  → polish, spread across the module.

## 8. How the project changed during development

It started as a **"Zombie Farm" planting loop** — isometric, placeholder/mixed art, an auto-resolve
battle. Three turning points shaped the final game:

1. **Art & theme pivot** — adopting the cohesive **Cute Fantasy** set and re-theming *Zombie Farm →
   Monster Farm*, moving from isometric to **top-down**. Because gameplay logic was kept view-agnostic,
   this was a presentation swap plus a few localised edits, not a rewrite.
2. **Combat rebuild** (the biggest change) — playtesters found the auto-resolve / RTS-micro combat
   passive and fiddly, so it was redesigned into a hands-on **action-brawler** with a fighting hero
   and command-the-whole-squad controls.
3. **Onboarding & polish** — audio, a Lab, a Bestiary, key rebinding, unit collision, a boss, a city
   map, framerate-correct movement, and a How-to-Play manual + tutorials so a new player can learn the
   game.

The plan-vs-built differences are tracked openly in [`doc/evidence.md`](../evidence.md), and the design
bible carries dated build-notes wherever the shipped game diverges from the original design.

## 9. External assets / templates / tutorials / AI declaration

**Templates / tutorials:** **none.** The game is built from scratch on stock Unity packages; no project
starter template or tutorial code was copied in.

**Game reference:** one — **Zombie Farm** — used only for the "grow your fighters on a farm" fantasy.

**External creative assets:** Cute Fantasy art (Kenmi), fonts (Pixel Operator, Alagard), audio (Ninja
Adventure SFX + three OpenGameArt music tracks). All are licensed and credited (section 10). The
structured per-resource and AI declarations are in **Appendix A**.

**AI:** an LLM-based AI coding assistant helped turn my design decisions into C# and stand up basic
scene content; the design, architecture, scene construction/tuning, debugging, and testing are mine.
Full structured declaration in **Appendix A**.

## 10. Credits and licences

| Resource | Source | Licence |
|---|---|---|
| **Cute Fantasy RPG** + expansion packs (all in-game art) | Kenmi — kenmi-art.itch.io/cute-fantasy-rpg | Cute Fantasy itch.io licence — commercial use permitted; **may not be resold or redistributed** (so git-ignored, build-only) |
| **Pixel Operator** (UI font) | Jayvee Enaguas (HarvettFox96) | **CC0 1.0** |
| **Alagard** (title/intro font) | Pix3M / Hewett Tsoi | Free commercial use **with credit** (CC-BY on OpenGameArt) |
| **Ninja Adventure** (11 SFX, audio only) | Pixel-Boy & AAA | **CC0 1.0** |
| Music — "One Step at a Time", "Town 3" | Alex McCulloch (OpenGameArt) | **CC0 1.0** |
| Music — "perces" | poinl (OpenGameArt) | **CC0 1.0** |
| URP, 2D Tilemap Extras, Input System, TextMesh Pro, Cinemachine | Unity Package Manager | Unity Companion License |

Full table with usage notes: [`doc/asset-credits.md`](../asset-credits.md). The licensed Cute Fantasy
art is excluded from the public repository per its licence and bundled only in the distributed build.

## 11. Reflection — organisation, time management, independent work, professionalism

- **Organisation.** One readable repo: a design bible (`doc/design/`), per-milestone testing logs
  (`doc/testing/`), an evidence index (`doc/evidence.md`), a peer-feedback log, asset credits, and
  scripts organised by system under `Assets/Scripts/`.
- **Time management.** A four-week plan with hard P0/P1/P2 tiers and a fixed MVP gate; the must-have
  core was built and tested first, then should-/could-have features layered on. Progress is spread
  across the module (May–June), not clustered at the end.
- **Independent work.** A solo project — the design, decisions, scene construction and testing are my
  own; AI-assisted implementation is disclosed; the licensed art is kept out of the public repo.
- **Professionalism.** Conventional Commits in English; feature-branch + PR workflow while the base
  was risky, then a deliberate move to main-direct once stable; two tagged releases with a changelog;
  feedback handled with traceable changes.
- **Honest self-assessment.** My weakest area was **testing rigour** — verification was manual
  in-editor playtesting and balance was tuned by feel rather than a formal study or automated tests.
  It caught the issues that mattered, but with more time I would add automated checks for the core
  systems (save/load round-trips, crop growth, combat resolution) and run a structured balance pass
  over `GameConfig`.

## 12. Known limitations and how I managed them

| Limitation | How I managed it |
|---|---|
| Only **City 1** is a fully built raid; cities 2–3 (Thornwood Hollow, Ashen Reach) exist as map nodes | Deliberate vertical-slice scope — one polished level conveys the experience; the rest reuse the same systems and are cheaper content. Stated as a known issue, not hidden. |
| **First-pass balance** (numbers tuned by feel) | Every tunable lives in one `GameConfig` asset, so a later balance pass is data-only, not a code change. |
| **Accessibility gaps** — no colour-blind palette or text-scale toggle | Mitigated with **text + colour** state (e.g. "(Hungry)"), full **key rebinding**, independent volume sliders, and a large legible font; gaps disclosed in [`doc/accessibility.md`](../accessibility.md). |
| **Display** tuned for 1920×1080; other ratios untested | Disclosed; the build notes the target resolution. |
| Cut features (task system, plot expansion, branching map, Barbed Wire, Fertilizer, Hunger Tonic) | Cut as P1 to protect the slice; recorded honestly in `roadmap.md` / `evidence.md` with build-notes, never described as shipped. |

---

# Appendix A — Structured External Resources / AI Declaration

### Resource 1 — Cute Fantasy RPG (+ expansion packs)
1. **Name:** Cute Fantasy RPG and its expansion packs
2. **Type:** asset (pixel art — image)
3. **Source:** Kenmi — https://kenmi-art.itch.io/cute-fantasy-rpg
4. **Licence / permission:** Cute Fantasy itch.io licence — commercial use permitted; **may not be resold or redistributed, even if modified** (so kept out of the public repo)
5. **What it provided:** all in-game pixel art — tiles, the player character, monsters, animals, buildings, props, UI frames/buttons/ribbons, icons
6. **What I used unchanged:** individual sprites/tiles at the pixel level (I did not repaint the art)
7. **What I modified:** sliced and recombined sprites into Unity **RuleTiles** (grass/hedge/cobble/water/river/field), **animated monster strips**, strain + UI **icons**, **decor prefabs**, and menu/intro backgrounds; mapped each strain to a species
8. **What I created myself:** every scene, level layout, prefab assembly, the parchment HUD composition, and all gameplay/logic
9. **Where it appears:** the Farm and Battle scenes, all monsters/buildings/decor, the HUD and menus
10. **How it is credited:** [`doc/asset-credits.md`](../asset-credits.md) + this portfolio; excluded from the repo and bundled only in the build

### Resource 2 — Pixel Operator (font)
1. **Name:** Pixel Operator
2. **Type:** asset (font)
3. **Source:** Jayvee Enaguas (HarvettFox96) — dafont.com/pixel-operator.font
4. **Licence:** CC0 1.0 (public domain)
5. **What it provided:** the main UI typeface (HUD, shop, tutorial, menus)
6. **What I used unchanged:** the glyph outlines (`Fonts/PixelOperator.ttf`)
7. **What I modified:** baked it into a TextMesh Pro SDF asset (`Fonts/PixelOperator SDF.asset`) for crisp pixel UI
8. **What I created myself:** all UI layout and text content
9. **Where it appears:** nearly all on-screen UI text
10. **How it is credited:** `doc/asset-credits.md` + this portfolio

### Resource 3 — Alagard (font)
1. **Name:** Alagard
2. **Type:** asset (font)
3. **Source:** Pix3M / Hewett Tsoi — dafont.com/alagard.font (also OpenGameArt)
4. **Licence:** free for commercial use **with credit** (CC-BY on OpenGameArt)
5. **What it provided:** the fantasy display typeface for titles and the storybook intro
6. **What I used unchanged:** the glyph outlines
7. **What I modified:** baked into a TMP SDF asset (`Alagard.asset`)
8. **What I created myself:** the titles, intro narration text, and layout
9. **Where it appears:** main-menu title, scene titles, storybook intro
10. **How it is credited:** `doc/asset-credits.md` + this portfolio (credit required by licence)

### Resource 4 — Ninja Adventure (SFX, audio only)
1. **Name:** Ninja Adventure asset pack (sound effects only)
2. **Type:** audio
3. **Source:** Pixel-Boy & AAA — pixel-boy.itch.io/ninja-adventure-asset-pack
4. **Licence:** CC0 1.0
5. **What it provided:** 11 SFX (button click, buy, plant, harvest, dash, hit, death, gate open, item throw, win, lose)
6. **What I used unchanged:** the `.ogg` clips
7. **What I modified:** wiring/trigger points and volume routing through `SfxManager`; no Ninja Adventure visuals are used
8. **What I created myself:** the `SfxManager` system, all call sites, and a sine-wave fallback for missing clips
9. **Where it appears:** farm and battle feedback sounds throughout
10. **How it is credited:** `doc/asset-credits.md` + this portfolio

### Resource 5 — Music (three tracks)
1. **Name:** "One Step at a Time" (menu), "Town 3" (farm), "perces" (battle)
2. **Type:** audio
3. **Source:** Alex McCulloch (first two) and poinl — OpenGameArt
4. **Licence:** CC0 1.0
5. **What it provided:** per-scene background music
6. **What I used unchanged:** the audio files
7. **What I modified:** scene→track mapping, looping and crossfades via `MusicManager`; volume control
8. **What I created myself:** the `MusicManager` system and the audio mix/levels
9. **Where it appears:** menu/intro, farm, and battle/tutorial scenes
10. **How it is credited:** `doc/asset-credits.md` + this portfolio

### Resource 6 — Unity packages
1. **Name:** URP, 2D Tilemap Extras, Input System, TextMesh Pro, Cinemachine
2. **Type:** code (first-party Unity packages)
3. **Source:** Unity Package Manager
4. **Licence:** Unity Companion License
5. **What it provided:** 2D rendering/lights, RuleTile/AnimatedTile, the New Input System, UI text, camera follow
6. **What I used unchanged:** the packages as shipped
7. **What I modified:** configuration only (render settings, input maps, Cinemachine confiner)
8. **What I created myself:** all game code that uses them
9. **Where it appears:** engine-wide
10. **How it is credited:** `doc/asset-credits.md` + this portfolio

### AI assistance declaration
1. **Tool used:** Claude (an LLM-based AI coding assistant).
2. **What I asked:** to turn my system designs into C# and to stand up basic scene content faster — given my decisions on what each system needed and how it should be structured (data model, cross-scene flow, combat/economy rules)
3. **What output I used:** draft implementations of scripts (e.g. combat, save, economy, UI) and rough first-pass scene scaffolding
4. **What I changed:** reviewed, modified, debugged and integrated every piece; reworked and **hand-tuned** all scene layout, spacing, sizing and feel; iterated values from playtests; fixed defects (e.g. the bomber-detonate crash, the framerate-dependent movement, the HUD scale bug)
5. **How I tested it:** manual in-editor verification + structured peer playtests each milestone, logged under `doc/testing/`; bugs surfaced in play were diagnosed and fixed (section 6)
6. **What I understand:** the architecture and data model (ScriptableObjects + a single `GameConfig`), the save/versioning design, how each strain passive and the hunger/permadeath systems work, the combat flow (area-gating, win/lose), and how the systems fit together — I can explain and modify any of it
7. **What I still do not fully understand:** the deepest internals of Unity's physics timestep and TextMesh Pro SDF generation I understand at a practical, working level (enough to use and debug them) rather than from first principles
8. **Where it appears:** across the C# under `MonsterFarm/Assets/Scripts/` and the initial scaffolding of the scenes, all subsequently reworked and tuned by me

*Full narrative account: [`doc/reference-and-contribution.md`](../reference-and-contribution.md) "Use of AI".*
