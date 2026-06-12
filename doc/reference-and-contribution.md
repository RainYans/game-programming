# Reference, Inspiration & Creative Contribution

*Honest account of what inspired Monster Farm, what was borrowed, and what is my own. Pairs with
the evidence in [evidence.md](evidence.md).*

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

I worked from **one game reference** plus the licensed art set; everything else is my own design.

| Reference | What I borrowed | What I changed / added / removed | Result in Monster Farm |
|---|---|---|---|
| **Zombie Farm** (the casual "grow zombies on a farm" game) — my one game reference | The core fantasy of **growing your own fighters on a farm**, like crops | Re-themed zombies → **monsters**; turned a pure farm/management game into a **farm-sim + real-time action raid** where you *lead the monsters you grew into battle*; added **six strains with unique passives**, a **hunger risk** system, and **permadeath** | A grow-then-fight hybrid where the farm exists to feed a squad you actively fight beside — not a farming game, not a clone |
| **Cute Fantasy** art set (kenmi-art, see [asset-credits.md](asset-credits.md)) — the licensed visuals | Tiles, character/monster sprites, props, UI frames | Sliced/recombined into **RuleTiles, animated monster strips, themed decor prefabs, and a parchment HUD**; mapped each strain to a species; built every scene + level | A single cohesive pixel look across farm + raid — the art is licensed/credited, but the *game* built from it is mine |

**Everything else is my own, not taken from a specific title.** The real-time action combat, the
lead-the-squad controls (left-click hero swing / right-click command-all), the four-room level
pacing, and the economy/hunger tuning I designed **myself by iterating on what felt fun** — drawing
on broadly familiar genre conventions (farm management; action games with a hero + helpers) rather
than studying any particular game. That originality is what makes it read differently from the
typical farming / RPG projects built on this same art set.

## Creative Contribution Statement

**What inspired my game?** The idea of *growing your fighters like crops* (a "zombie farm" planting
loop), then leading them into battle.

**What did I borrow as inspiration?** One game — **Zombie Farm** (the casual "grow zombies on a
farm" game) — for the grow-your-fighters-on-a-farm fantasy, plus the **Cute Fantasy** art set for
all visuals (licensed + credited). The rest of the design I worked out myself, by feel.

**What did I change?** Re-themed the whole game (zombies → monsters), replaced RTS-style combat
micro with an action-brawler scheme, and rebuilt the battle into a four-room linear raid.

**What did I add?** Six strains with unique passives, a hunger risk system, a directly-controlled
hero with a melee swing + dash + throwable items, area-gated levels with a minimap, hunger-affected
combat, and a full themed HUD + result screen.

**What did I remove?** Seasons/relationships/large open-world farming, isometric view, and the
fiddly select-then-command micro from the early combat.

**What makes my version different?** Most farming games, you grow crops to **sell**; here you grow
monsters to **fight** — and you pick up a sword and fight **beside** them. The hook no other game on
this art set has: the monsters you spend time raising are the **exact ones you can lose forever** in
a raid (permadeath). Because you're risking units you're genuinely attached to, **caring about your
monsters becomes a real mechanic, not just flavour** — every deploy is a true decision. The same
squad and save carry over from the cosy farm into the hands-on raid, and you're an active fighter,
not a spectator of an auto-battle.

**What is my own creative contribution?** The **design and direction** are entirely mine — the
concept, the core loops, the six strains and their passives, the hunger and permadeath systems, the
combat feel, and the level layout. I **designed every scene** and reworked and hand-tuned them, and
ran the whole **testing-and-iteration loop** off playtests and peer feedback. For the **scripting**,
I worked with an AI coding assistant: I decided what each system needed and how it should be
structured, directed the implementation, then reviewed, modified, debugged, and integrated it. The
**external creative assets** are the Cute Fantasy art (Kenmi), the UI/title fonts (Pixel Operator,
Alagard), and the SFX (Ninja Adventure) — all licensed and credited in
[asset-credits.md](asset-credits.md); everything else is my own.

**Where is the evidence?** Scenes (`Farm.unity`, `Battle.unity`), the scripts (organised by
system under `Assets/Scripts/`), the GitHub commit history and the change log in
[evidence.md](evidence.md).

## Use of AI

I'm open about how the project was built. The division of work:

- **Mine (design + build + judgement):** every design and architecture decision — which systems to
  build, how they fit together, the data model (ScriptableObjects for strains/crops/missions, a
  single `GameConfig`), the cross-scene data flow, and the combat/economy tuning; the **scene design,
  layout, and hand-tuning** (see the note below); and the whole **testing-and-iteration loop** driven
  by playtests and peer feedback.
- **AI-assisted (implementation):** I used an AI assistant to help turn those decisions into C# and
  to build basic scene content faster. The real work is in **directing** it — deciding what to build,
  judging whether the result is actually right, fixing it when it isn't, integrating the pieces into
  one coherent game, and iterating from playtests. AI sped up the implementation; the design, the
  decisions, the debugging, and the polish are mine.

**On the Unity scenes:** I designed each scene first — what it contains, the layout, how it should
look and play. I used AI to build a **basic version** quickly, then **reworked and hand-tuned it
myself:** the actual layout, positioning, spacing, sizing, and the many rounds of fine-tuning that
make a scene read and play right are manual work — AI can get you a rough frame, not the feel. So
scene construction is **AI-assisted at the basic stage, but designed and hand-tuned by me** — the
design and the finished result are mine.

In short, the AI accelerated *writing the code*; the *design, structure, decisions, Unity build, and
the finished game* are mine. The external creative assets are the Cute Fantasy art, the fonts (Pixel
Operator, Alagard), and the SFX (Ninja Adventure) — all licensed and credited in
[asset-credits.md](asset-credits.md).
