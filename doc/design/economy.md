# Design — Economy & Buildings

> **🔧 Build note (final submission).** Adjusted for scope: **plot expansion** and the **Fertilizer /
> Hunger Tonic** consumables were **cut**; combat items shipped as **Rotten Onion + Freeze Canister**
> only (Barbed Wire cut). Currency, the Shop (seeds + items), and the **Lab** upgrades are all in the
> build. See [evidence.md](../evidence.md).

A single currency ties the loop together: win raids → earn currency → spend it on seeds,
combat items, strain upgrades, and plot expansion → field a stronger army → win harder raids.

## Currency

- **One currency** (working name: "resources"). No secondary currencies in scope.
- **Earned:** primarily from completing combat stages / cities; secondarily from task
  rewards (see [progression.md](progression.md)).
- **Spent:** seeds, items, Lab upgrades, plot expansion.
- Held in `Wallet` (implemented); balance is persisted.

## Shop

The Shop sells two things:

### 1. Seeds

- One purchasable seed per monster strain the player has **unlocked**.
- Prices differ by strain power. Buying a seed lets the player plant that strain.
- Catalog and prices live in `GameConfig` (`seedCatalog`, already scaffolded).

### 2. Combat Items

Items are **field-control tools**, not raw damage — consistent with the combat pillar
(see [combat.md](combat.md)). Two shipped for v1 (Barbed Wire is designed but deferred to P1):

| Item | Type | Effect |
|------|------|--------|
| **Rotten Onion** | Thrown, area | The stench **repels / drives wild monsters away** from the target area — clears space, breaks a push. |
| **Freeze Canister** | Thrown, area | **Freezes** wild monsters in the area for a short time (and may increase damage taken). |
| **Barbed Wire** *(P1 — not in this build)* | Deployed, placed | **Slows** wild monsters crossing it; used to reroute the horde into a kill zone. |

- Items are **placed at a target area** in combat — they do **not** require selecting
  monsters first.
- Two more consumables come from the farming/hunger theme and exist in the design but are
  lower priority:
  - **Fertilizer** — speeds up crop growth (used on the farm, not in combat).
  - **Hunger Tonic** — re-applies the Hungry (strong) state to your squad mid-
    battle.

> **Stretch item pool (P2, not built for v1):** UV lamp, noise maker, pheromone spray,
> auto-turret, electric net, blood-bait (risk/reward), mystery serum (random effect),
> infection suppressant (power for risk), EMP, decoy, drone, fuel-drum combos. See
> [Icebox in roadmap.md](roadmap.md#icebox-p2).

## Lab

Spend currency to **upgrade strain base stats** (HP / Attack / etc.). Upgrades are
per-strain and permanent. The Lab is the main currency sink that makes the army stronger
between raids. (Upgrade tiers and costs: `GameConfig`, tuned during balancing.)

## Plot Expansion

Spend currency to **unlock additional farm plots**, raising the cap on simultaneously
growing monsters. A second currency sink that scales the player's throughput.

## Buildings as Access Points

Each economic system is reached by walking the avatar to a building and opening its panel:

- **Shop** → seeds + items (`ShopController` / `ShopPanelUI` / `ItemStore`, scaffolded).
- **Lab** → strain upgrades.
- **Deploy point** → city-selection map.

Buildings are flat UI panels (no interiors) for v1.

## Tunable Numbers

All economy numbers — starting resources, starting seeds, seed prices, item prices, upgrade
costs and effects, plot-expansion costs, reward amounts — live in a single **`GameConfig`**
ScriptableObject (implemented) so balancing happens in one place.

## Persistence

Wallet balance, owned seeds, unlocked strains, purchased upgrades, and unlocked plots are
all written to the save file. (Save system in [process.md](../process.md) /
`SaveManager`, implemented and extended as new state is added.)
