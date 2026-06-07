# Design — Art Production Pipeline

> ⛔ **SUPERSEDED by the v2 pixel pivot — see [direction.md](direction.md).** This file is the
> earlier *isometric + Kenney + AI-generated* art plan and is **no longer the direction**.
> Current art = the single **Ninja Adventure** pixel pack (top-down). Kept for history only.

How we produce, style, and integrate the game's art (and the matching SFX). This is the
**production** companion to [presentation.md](presentation.md) (which sets the art *direction*).
Grounded in the actual runtime systems (`ZombieData` strains, `CropInstance` growth, `FarmRoamer`,
`BattleAgent`, hunger `Full/Hungry`, shop items) so every listed asset maps to real code.

> **Cohesion note:** the project mixes **Kenney clean-flat isometric** tiles/UI (3D-rendered look)
> with **AI-generated cartoon** characters. See §1 for the rules that keep that from clashing.

## 0. Status — START HERE (for the dedicated art session)

**Mechanics are done (MVP gate hit); art is the only remaining layer. Keep it time-boxed.**

**Decisions locked this round (do NOT re-litigate):**
- **Keep ISOMETRIC.** Do not switch perspective — it's a costly re-architecture (tilemap/
  camera/movement/scene-setup) for ~zero art benefit. Characters are 2D billboards on iso
  ground (standard; the reference game does this).
- **Direction = cohesive cartoon.** Environment + UI = **Kenney (CC0)**. Characters/units =
  **one cohesive ANIMATED pack** — pick ONE and commit, don't re-explore:
  - Leading free option: **CraftPix "Chibi" family** (cohesive line; many packs in
    `D:\unity资源\CraftPix`). Confirmed cohesive (Elemental Spirits + Mercenaries match).
  - Leading paid option: **EPStar "2D Monster Bundle"** (10 monsters, Unity prefabs +
    Animator ready — least integration work).
- **De-risk first:** wire ONE character end-to-end (`BattleUnitAnimator` → `BattleAgent`)
  and run it in-engine BEFORE doing all 9. Once one walks+attacks, the rest is repetition.

**Already staged (CC0, safe — just needs applying):**
- `Assets/Art/Buildings/` (Kenney houses + iso buildings), `Assets/Art/Props/` (fences/trees).
  Apply via `Tools > Zombie Farm > Building Art...` and `Dress Farm Ground` — **not yet applied
  to the scene** (2 clicks gets an instant farm upgrade).
- `Assets/Resources/SFX/*.ogg` (real clips) + `SfxManager` wired — **already live**.

**Immediate next steps:**
1. Apply building art + dress the farm (the two editor tools above).
2. Pick the character pack; wire ONE character as the spike; verify in-engine.
3. Then map all 6 strains + 3 wild + 3 NPCs (mapping table in §3/below); swap `FarmRoamer` +
   deploy/shop icons too.

**⚠️ Licensing:** `D:\unity资源\CraftPix` is a redistribution-site dump — for the deliverable,
use CraftPix's genuine free *freebies* (download from craftpix.net) or buy the packs you ship.
The Kenney art already in the project (Buildings/Props/SFX) is CC0 and safe.

---

## 1. Art Direction Decision — "Pragmatic Hybrid" (LOCKED)

We do **not** repaint the whole game to one bespoke style. Instead:

- **Environment + UI:** keep **Kenney** assets as-is (already downloaded, CC0, cohesive):
  isometric landscape/farm/city/buildings tiles + the parchment/wood **UI Pack (Adventure/RPG)**.
- **Characters + zombies:** **AI-generated** in a friendly cartoon style.
- We **accept a small style gap** between the two — normal for indie/jam scope, fine on video.

**Cohesion rules (make AI units sit on Kenney tiles without clashing) — bake these into prompts:**
1. **Angle:** render units at a slight **3/4 top-down** angle (not pure side-on) to match the iso ground.
2. **Lighting:** soft light from **top-left**, matching Kenney's tile shading.
3. **Shading:** **flat cel-shading**, minimal gradients; a **thin, consistent outline** (not heavy PvZ ink).
4. **Palette:** medium saturation, friendly; avoid grim/desaturated. Sample Kenney greens/browns.
5. **Seating:** every unit gets a **soft contact shadow ellipse** at its feet so it grounds on the tile.
6. **Scale:** fix a reference height — a standing zombie ≈ **1 tile tall**; the avatar slightly taller.
7. **Pivot:** export with pivot at **bottom-center** (feet) so depth-sort on axis `(0,1,0)` reads right.

## 2. Style Bible / Anchor

Lock these **before** batch-generating; reuse on every prompt (see §7 prompt library).

- **Setting:** a cute, hopeful **post-apocalyptic zombie farm** — friendly engineered zombies,
  non-grim ruined cities. (Full setting in [vision.md](../vision.md).)
- **View:** 2D isometric world; units are 3/4 billboards facing the camera.
- **Palette:** warm earth + friendly greens (match Kenney); per-strain accent colors below.
- **Resolution:** generate large (≥1024²), clean up, then downscale to target. **Transparent PNG**.
- **Per-category style buckets** (save a separate reference/style per bucket so they stay cohesive):
  `characters` (humans), `zombies`, `vfx`, plus the fixed Kenney `environment` + `ui`.

**Per-strain identity (colors come from `ZombieData.color`; silhouettes must read apart):**

| id | Name | Role / passive | Color | Silhouette cue |
|----|------|----------------|-------|----------------|
| `brute` | Brute | Tank / ThickHide | green-grey | huge, broad, armored hide |
| `mauler` | Mauler | Damage / Bloodlust | red | lean, big claws, hunched |
| `runner` | Runner | Skirmisher / Evasion | yellow | skinny, long legs, mid-sprint |
| `spitter` | Spitter | Ranged / Corrosion | green | bloated, acid sacs, spitting |
| `shaman` | Shaman | Support / Aura | blue | robed, glowing totem, calm |
| `bomber` | Bomber | Burst / SelfDetonate | purple | round, swollen, lit fuse |

Wild enemies (`wild_normal` / `wild_runner` / `wild_brute`): same base zombie, **feral & a bit
menacing** (not domesticated), recolored grey-green / lean / bulky respectively.

## 3. Full Asset Manifest

Priority tiers: **P0** = needed to stop looking like a prototype on the demo video; **P1** = target
polish; **P2** = stretch. Each row maps to a system/backlog issue.

### 3.1 Characters (humans) — AI
| Asset | States/frames needed | Tier | Wires into |
|-------|----------------------|------|-----------|
| Farmer **avatar** | front + side (flip for L/R); later walk 4–6f | P0 | `AvatarController` sprite (#64) |
| **Shopkeeper** NPC | single front idle (2f ping-pong opt.) | P1 | stands by Shop building |
| **Doctor/Professor** NPC | single front idle | P1 | stands by Lab building |

### 3.2 Zombies — AI (the stars)
Per the 6 strains + 3 wild. Each unit, by context:
| Context | What's needed | Tier |
|--------|---------------|------|
| Farm roam | idle (+ walk later); **Full vs Hungry** look (lively vs droopy/gaunt) | P0 idle, P1 hunger variant + walk |
| Seed growth | shared **seed mound → sprout**, then **strain-specific "emerging" ripe** sprite | P0 (3 stages) |
| Battle | **idle** (P0) → walk / attack / death (P1) | P0 idle, P1 anims |
| Special | spitter **acid projectile**; bomber **explosion**; shaman **aura** ring | P1 |
| Icon | one **portrait/thumbnail** per strain | P0 (deploy panel + shop cards) |

### 3.3 Crops / seeds (the "planted zombie" growth) — AI or Kenney foliage
`CropInstance` has 3 stages (currently colored dots). Replace with sprites:
`seed` (tilled mound + seed) → `growing` (sprout/hand reaching up) → `ripe` (zombie emerging).
**P0**, shared base + per-strain ripe tint. Also a **seed-packet icon** per strain for the
seed-pick popup + shop (P0).

### 3.4 Environment — Kenney (already downloaded, CC0)
| Need | Pack | Tier |
|------|------|------|
| Richer farm ground (grass/dirt/water/paths/fences/crop rows) | **landscapeTiles** (already in project — only ~6 of 128 tiles used!) | P0 (no download) |
| 4 buildings (Home / Shop / Lab / WarCamp) | **cityKitSuburban** (Home) + **isometric-buildings** (Shop/Lab/WarCamp) | P0 (#66) |
| Battle "ruined city" | **isometric-city** + **isometric-buildings**, arranged + tinted/desaturated as rubble | P1 (#67) |
| Decor (trees/fences/props) | landscapeTiles + Kenney Foliage | P1 |

### 3.5 UI — Kenney UI Pack (Adventure/RPG = parchment/wood, matches the reference)
Parchment panels, wood buttons, page tabs, scroll dialog box, currency icon (single currency —
`Wallet.Resources`), **stat star icons** (力量/生命/速度 → attack/HP/speed), seed-packet icons,
hunger bar, **objective ribbon** (#59), shop cards, result/prep panels. **P0/P1** (#69).

### 3.6 VFX & SFX
- VFX (P1): onion repel cloud, freeze, hit, death poof, bomber explosion, heal aura, acid bolt, harvest sparkle.
- SFX (P1, #68): swap `SfxManager` synth blips for **Kenney Interface/UI/Impact Sounds** (CC0, downloaded).

### 3.7 Story / text (P1)
- Opening narrative (from [vision.md](../vision.md)).
- **Per-strain flavor blurb** (scroll-dialog on first plant, like the reference "哈牛!" popup) — short, playful.
- Task hint strings (#59), building tooltips, result-screen copy. (UI text language: match current build;
  localize later.)

## 4. Animation Spec (when we add motion, P1+)

| Action | Frames | Loop | Timing | Notes |
|--------|--------|------|--------|-------|
| Idle | 2–4 | ping-pong | 400–500ms/f | "home base" pose |
| Walk | 4–6 | standard | 100–150ms/f | even timing |
| Attack | 3–6 | one-shot | hold impact 150–200ms | anticipation + follow-through frame |
| Death | 4–6 | one-shot | — | end frame can fade out |

- Run at **8–12 fps** (stylized). Isometric ⇒ at least front+side (flip); 4/8-dir is stretch.
- **Scope reality:** 9 units × 4 actions is large — at **P0 a single idle sprite per unit is enough**
  (battle code already does hit-flash, scale, death-destroy). Add walk/attack/death per-unit at P1.

## 5. AI Generation Workflow (end-to-end)

1. **Lock a style anchor:** generate/pick ONE hero asset you love; it defines palette/outline/lighting.
2. **Consistency method** (pick one): character-reference (Leonardo / Midjourney `--cref`), or a trained
   model / LoRA (Scenario, ComfyUI) for best consistency at volume. Reuse the anchor + a **fixed seed**.
3. **Generate base poses:** front + 3/4 + side per character, on transparent bg, following §1 rules.
4. **Animate (P1):** feed the static sprite to a sprite-sheet tool (AutoSprite / Spritesheets.ai /
   Scenario video / PixelLab for directional) → walk/attack/etc. as a sheet + JSON.
5. **Manual cleanup:** Photoshop/GIMP — transparent, trim, unify canvas + **bottom-center pivot**, fix palette.
6. **Import to Unity:** consistent **PPU** (match tiles), slice sheet, Animation/Animator or simple frame-swap.
7. **Test in-engine**, iterate; **log every asset in [asset-credits.md](../asset-credits.md)**.

**Tools (researched):** Scenario (consistency + sheets), Leonardo/Midjourney (character ref),
AutoSprite / Spritesheets.ai (single sprite → animated sheet, engine export), PixelLab (iso multi-dir,
pixel-leaning), ComfyUI + LoRA (free/local, advanced). Start with ONE character + ONE animation to gauge control.

## 6. Naming & Integration Conventions

- **Folders:** `Assets/Art/Characters/`, `Assets/Art/Zombies/`, `Assets/Art/Crops/`, `Assets/Art/UI/`,
  `Assets/Art/Tiles/`, `Assets/Audio/`.
- **File names = ids** so wiring is automatic: `brute_idle.png`, `mauler_attack.png`,
  `wild_normal_idle.png`, `seed_mound.png`, `sprout.png`, `brute_ripe.png`, `avatar_front.png`.
- I (agent) wire: `BattleAgent` (replace generated square → strain sprite), `FarmRoamer` (roam sprite +
  Full/Hungry swap), `CropInstance` (3 growth-stage sprites), `DeployPanel`/`ShopPanelUI` (icons),
  building objects (Home/Shop/Lab/WarCamp), UI panels (Kenney 9-slice).

## 7. Prompt Library (reuse verbatim)

**Shared prefix (every character/zombie) — tuned cute/simple for small-size readability:**
> `cute cartoon mobile-game sprite, chibi proportions with a big head and small body, friendly dorky
> engineered zombie, big expressive eyes, bold simple shapes with minimal interior detail, clean
> medium-thick outline, flat cel-shaded colors, soft top-left lighting, isometric 3/4 top-down view,
> full body single character centered, soft round contact shadow at the feet, transparent background,
> reads clearly at small size, consistent proportions and scale`

> Direction = **cute/simple** (matches the reference game + reads in-battle), NOT the gritty
> armored-warrior look. **Anchor = the approved chibi Brute** (chunky, goofy grin, simple shapes).
> Prop rule: melee strains (Brute/Mauler) may carry **one simple, cute** weapon/claw (e.g. Brute's
> spiked club); ranged/support (Spitter/Shaman/Bomber) use their theme element (acid / totem / gas),
> no weapons — keep props simple so they read small. The first detailed/gritty render is kept only
> as a shop/codex portrait.

**Humans** (chibi but the farmer is a **rugged cowboy**, sturdier build than the dorky zombies):
farmer `+ a rugged tough cowboy farmer, brown cowboy hat, light stubble, confident smirk, bandana,
denim vest over rolled-up sleeves, work gloves, rugged boots, holding a pitchfork, confident stance`;
shopkeeper `+ cheerful shopkeeper, apron, flat cap, coin pouch, welcoming`; doctor `+ scientist
professor, white lab coat, round glasses, holding a beaker/clipboard`.

**6 strains** (append to prefix; cute/weaponless, silhouettes kept distinct):
Brute `the Brute (tank): big and chubby, broad rounded shoulders, thick green-grey skin, one small
patched armor scrap, goofy tough grin, sturdy and slow`; Mauler `the Mauler (bruiser): lean and scrappy,
oversized simple claws, eager snarling grin, hunched forward, red skin`; Runner `the Runner (speedster):
skinny with long noodly legs, wide eyes, mid-dash pose, yellow skin`; Spitter `the Spitter (ranged):
round and bloated, puffy cheeks, a little green acid drip, green skin`; Shaman `the Shaman (support):
small, simple robe, holding a tiny glowing totem, calm cute face, soft aura, blue skin`; Bomber
`the Bomber (exploder): round balloon-like body puffed with glowing gas, tiny lit fuse on top, nervous
grin, purple skin`.

**Wild enemies:** `+ feral wild zombie, ragged, slightly menacing but still cartoonish` (grey-green /
lean / bulky variants).

**Consistency tips:** same prefix + same seed across a batch; generate front/side together; high-res →
downscale; keep one reference image pinned for character-reference mode.

## 8. Build Tiers (maps to backlog)

- **P0 (stop looking like a prototype):** 6 strain idle sprites (#65) + farmer avatar (#64) + seed 3
  stages + Full/Hungry variant; enrich farm ground from existing Kenney tiles; building sprites (#66);
  Kenney UI on panels (#69).
- **P1:** walk/attack/death anims; shopkeeper + doctor; battle ruined-city dressing (#67); VFX; real SFX
  (#68); story/flavor text.
- **P2:** 4/8-dir animations, richer VFX, cutscene art.

## 9. Licensing

- Kenney packs = **CC0** (no attribution required; log anyway).
- AI-generated art = original; note the tool/model used.
- **Every** imported asset → a row in [asset-credits.md](../asset-credits.md) (source + license).
- Avoid pirate "free Unity asset" redistribution sites; only CC0 / CC-BY (with credit) / purchased / AI.
