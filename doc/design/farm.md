# Design — Farm & Avatar

> ⚠️ **Direction updated — see [direction.md](direction.md).** View pivoted **isometric →
> top-down pixel** (art = **Cute Fantasy** pack); theme = **Monster Farm** ("zombie" =
> "monster", display only). Farm logic (planting, growth, roaming, hunger) is unchanged — it's
> reskinned and re-laid-out on a square grid, not rewritten.

> **🔧 Build note (final submission).** **Plot expansion was cut** — the farm ships with a fixed plot
> set. Everything else here (avatar movement, walk-up plant/harvest, roaming monsters, buildings incl.
> the Lab) is in the build. See [evidence.md](../evidence.md).

The farm is the home base and the hub of the game. The player controls an avatar (a
"doctor-farmer") who walks the base to plant, tend, harvest, shop, and deploy.

## Camera & View

- **Top-down** 2D view (pivoted from the original isometric foundation). URP 2D, Transparency
  Sort Mode = Custom Axis `(0, 1, 0)` so taller objects sort correctly.
- Camera follows the avatar; scroll wheel zooms within clamped bounds.

## Avatar

- **Movement:** WASD, in top-down space. The camera follows.
- **Interaction:** the avatar must be **near** a target to act on it. A context interact
  key (E) triggers the relevant action — plant/harvest a plot, open a building.
- **Why an avatar:** it turns the farm from a menu into a place and makes positioning part
  of play. The earlier click-only model is retired. The existing `FarmActions` layer is
  already input-agnostic, so the avatar dispatches the same plant/harvest/openShop actions
  the prototype's clicks did — no rewrite of action logic.

## Plots & Planting

- Two tilemaps: a decorative **Ground/paths** layer (not plantable) and the plantable
  **FarmPlot** layer that `GridManager` queries. This gives the base a built look without an
  empty grid.
- **Plant:** walk to a plot edge → press interact → a small popup lists the seed types the
  player owns → choose one → the seed is planted and begins growing.
- **Grow:** the plot advances through growth stages (seed → growing → ripe) driven by
  real-time, persisted as a planted timestamp so growth continues across saves.
  (Implemented as `CropData` + `CropInstance`.)
- **Harvest:** walk to a ripe plot → press interact → the grown monster is produced.

## Grown Monsters Roam the Farm

When a monster is harvested it does **not** go into an abstract inventory counter — it
**spawns as a roaming unit on the farm**. These wandering monsters are the player's standing
army: at deploy time the player picks from the monsters currently on the farm.

- Roaming monsters wander idly and show their **strain** and **hunger state** (see
  [zombies.md](zombies.md)) so the player can read their farm at a glance.
- This visually connects "what I grew" with "what I can deploy," and gives the base life.

> **Implementation note / migration:** the foundation stores harvested monsters as
> `Inventory` counts. Moving to roaming farm units is a Week-1 task. A count-based fallback
> is acceptable for the MVP gate if spawning roamers slips, but roaming units are the target.

## Plot Expansion

The farm starts with a limited number of plots. The player can spend currency to **unlock
additional plots**, increasing how many monsters can grow at once. (Economy in
[economy.md](economy.md).)

## Buildings

Buildings sit around the plot as click-to-open placeholders the avatar walks up to:

- **Shop** — buy seeds and combat items.
- **Lab** — spend currency to improve strain stats.
- **Deploy point / city gate** — opens the city-selection map to start a raid.

Buildings are flat UI panels for now (no walkable interiors). `BuildingInteraction` handles
the proximity-open. Interiors and NPCs are explicitly out of scope (see
[Icebox in roadmap.md](../roadmap.md#icebox-p2)).

## Open Questions

- **Over-hunger downside on the farm:** if a roaming monster goes too long without feeding,
  does it lose HP / risk starving? (See the hunger mechanic in [zombies.md](zombies.md) —
  this is the one balance hook still to confirm.)
- Exact interact key and whether harvest is proximity-auto or key-gated (currently: key).
