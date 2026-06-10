# Reference, Inspiration & Creative Contribution

*Honest account of what inspired Monster Farm, what was borrowed, and what is my own. Pairs with
the project report and the evidence in [evidence.md](evidence.md).*

## How the idea started (honest origin)

The seed of the project was a **"zombie farm" planting loop** — the simple idea of *growing* your
fighters on a farm the way you grow crops, then sending them out to fight. I prototyped that loop
first (planting, inventory, a basic battle) with placeholder/mixed art.

Later I decided the art mattered: I found the **Cute Fantasy** pixel set (kenmi-art), liked its
bright, cohesive look, and **re-themed the whole game from "Zombie Farm" to "Monster Farm"** around
it. The combat then grew from a basic auto-battle into a directly-controlled **action-brawler**
(you lead a hero and a squad). So the game is a deliberate **mix of a farm-sim loop and a
lead-your-squad action raid**, not a clone of any single title.

## Reference Transformation Table

| Inspiration / reference | What I borrowed | What I changed / added / removed | Result in Monster Farm |
|---|---|---|---|
| **Farm-sim loop** (Stardew-Valley-style planting/harvest, shop, day-to-day base) | The calm tile-grid plant→grow→harvest loop, a shop, and an inventory | Removed seasons/relationships/large open farm; the "crops" are **monsters you raise to fight**, not produce to sell | A compact farm that exists to **feed a combat squad**, giving the cosy loop real stakes |
| **"Grow creatures, then send them to battle"** (creature-raising / monster-collector idea, the original Zombie-Farm pitch) | The core fantasy of owning and raising your own fighters | Turned vague "zombies" into **six distinct strains with unique passives** + a hunger risk system | Squad identity: *which* monsters you bring changes the fight, not just the numbers |
| **Lead-a-squad action games** (e.g. Pikmin / Overlord — a hero who commands little units) | One directly-controlled leader + a following squad that fights | Added a real **hero melee attack** (swing, dash, items), simplified command to **right-click the whole squad** (no RTS selection), made the squad auto-fight | An action-brawler where the player both **fights and commands**, instead of pure auto-battle or pure RTS |
| **Top-down clear-the-area level design** (room-by-room dungeon/level pacing) | Linear "one screen, one scene" rooms with gated progression | Each room is a **distinct theme** (farm / river+bridge / hedge garden / village square); gates open on clear; minimap | City1 reads as a short, escalating raid rather than one flat arena |
| **Cute Fantasy art set** (kenmi-art, see [asset-credits.md](asset-credits.md)) | Tiles, character/monster sprites, props, UI frames | Sliced/recombined into **RuleTiles, animated monster strips, themed decor prefabs, and a parchment HUD**; mapped each strain to a species; built the levels | A single cohesive pixel look across farm + raid (the art is licensed and credited; the *game* built from it is mine) |

## Creative Contribution Statement

**What inspired my game?** The idea of *growing your fighters like crops* (a "zombie farm" planting
loop), then leading them into battle.

**What did I borrow as inspiration?** The calm farm-sim loop (plant/harvest/shop/inventory), the
creature-raising fantasy, and the lead-a-squad action pattern — plus the Cute Fantasy art set for
all visuals (licensed + credited).

**What did I change?** Re-themed the whole game (zombies → monsters), replaced RTS-style combat
micro with an action-brawler scheme, and rebuilt the battle into a four-room linear raid.

**What did I add?** Six strains with unique passives, a hunger risk system, a directly-controlled
hero with a melee swing + dash + throwable items, area-gated levels with a minimap, hunger-affected
combat, and a full themed HUD + result screen.

**What did I remove?** Seasons/relationships/large open-world farming, isometric view, and the
fiddly select-then-command micro from the early combat.

**What makes my version different?** It **fuses a cosy farm-raising loop with a hands-on action
raid using one shared squad and save** — you grow the exact monsters you then fight beside, and you
are an active fighter, not a spectator of an auto-battle.

**What is my own creative contribution?** The **design and direction** are entirely mine — the
concept, the core loops, the six strains and their passives, the hunger and permadeath systems, the
combat feel, and the level layout — as is all the **Unity construction** (scene building, tilemaps,
level layout, UI) and the **integration, debugging, and iteration** driven by playtesting and
feedback. For the **scripting**, I worked with an AI coding assistant: I decided what each system
needed and how it should be structured, directed the implementation, then reviewed, modified, and
wired the code into the project — and I understand it well enough to explain and extend any of it.
The **art** is the only fully external creative asset (Cute Fantasy pack, licensed and credited).

**Where is the evidence?** Scenes (`Farm.unity`, `Battle.unity`), the scripts in
the project report, the GitHub commit history and the change log in
[evidence.md](evidence.md).

## Use of AI

I'm open about how the project was built. The division of work:

- **Mine (design + build + judgement):** every design and architecture decision — which systems to
  build, how they fit together, the data model (ScriptableObjects for strains/crops/missions, a
  single `GameConfig`), the cross-scene data flow, and the combat/economy tuning. All the Unity
  work — scene and level construction, tilemaps, UI layout, and wiring every component — plus the
  testing-and-iteration loop driven by playtesting and peer feedback.
- **AI-assisted (implementation):** I used an AI coding assistant to help turn those decisions into
  C#. I directed what to write, then reviewed, modified, debugged, and integrated the result. I can
  open any script and explain what each part does and why it's structured that way.

In short, the AI accelerated *writing the code*; the *design, structure, decisions, Unity build, and
the finished game* are mine. The only external creative asset is the Cute Fantasy art, credited in
[asset-credits.md](asset-credits.md).
