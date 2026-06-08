using System.Collections.Generic;
using UnityEngine;

/// Cross-cutting, designer-tunable economy numbers for M4 in one asset. Per-strain stats
/// stay on CropData / ZombieData; this holds the bootstrap values and the seed catalog —
/// which doubles as the id -> CropData registry the save system uses to rebuild crops.
[CreateAssetMenu(fileName = "GameConfig", menuName = "MonsterFarm/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Fresh-save bootstrap")]
    [Min(0)] public int startingResources = 0;
    public List<SeedStack> startingSeeds = new List<SeedStack>();

    [Header("Hunger")]
    [Tooltip("Farm-side idle time before a Full zombie drifts to Hungry (stronger). " +
             "A freshly harvested unit starts Full and becomes Hungry after this many seconds.")]
    [Min(1f)] public float hungerDelaySeconds = 60f;

    [Tooltip("Combat damage multiplier applied to a unit deployed while Hungry (Full = 1x). " +
             "Per the design pillar, letting a zombie get hungry makes it hit harder in battle. " +
             "Snapshotted at deploy time. Tune in the M4 balancing pass.")]
    [Min(1f)] public float hungryDamageMultiplier = 1.35f;

    [Tooltip("Incoming-damage multiplier for a unit deployed while Hungry (Full = 1x). The " +
             "trade-off for the attack bonus: a hungry zombie hits harder but is more fragile, " +
             "so letting the whole squad starve is a real risk. Snapshotted at deploy.")]
    [Min(1f)] public float hungryDamageTakenMultiplier = 1.25f;

    [Header("Strains & combat")]
    [Tooltip("Every strain's ZombieData, for resolving a saved strain id back to its stats " +
             "(deploy screen, battle). Populated by Tools > Monster Farm > Setup Zombie Strains.")]
    public List<ZombieData> allStrains = new List<ZombieData>();

    [Tooltip("Max zombies the player can deploy in one squad.")]
    [Min(1)] public int squadCap = 4;

    [Header("Combat tuning")]
    [Tooltip("Battle balance numbers (engagement ranges, attack cadence, the six passives). " +
             "Read by BattleAgent each raid; carried into the Battle scene via BattleHandoff.")]
    public CombatTuning combat = new CombatTuning();

    /// Resolve a strain id to its ZombieData (stats + passive). Null if unknown.
    public ZombieData FindStrain(string id)
    {
        foreach (ZombieData z in allStrains)
            if (z != null && z.id == id) return z;
        return null;
    }

    [Header("Seed shop")]
    public List<ShopEntry> seedCatalog = new List<ShopEntry>();

    [Header("Item shop (combat items)")]
    [Tooltip("Combat items the Shop sells. The Rotten Onion (id below) is the one wired for " +
             "the MVP gate — bought here, carried into a raid, thrown in the field.")]
    public List<ItemEntry> itemCatalog = new List<ItemEntry>();

    /// Stable id of the Rotten Onion combat item — repel item, bought -> carried -> thrown.
    public const string RottenOnionId = "rotten_onion";

    /// Stable id of the Freeze Canister combat item — freeze item, same buy/carry/throw loop.
    public const string FreezeCanisterId = "freeze_canister";

    /// Resolve a CropData by its id, searching the catalog then the starting seeds.
    /// Used when loading a save to turn a stored crop id back into its definition.
    public CropData FindSeed(string id)
    {
        foreach (ShopEntry e in seedCatalog)
            if (e.seed != null && e.seed.id == id) return e.seed;
        foreach (SeedStack s in startingSeeds)
            if (s.seed != null && s.seed.id == id) return s.seed;
        return null;
    }

    [System.Serializable]
    public struct SeedStack
    {
        public CropData seed;
        [Min(0)] public int count;
    }

    [System.Serializable]
    public struct ShopEntry
    {
        public CropData seed;
        [Min(0)] public int price;
    }

    [System.Serializable]
    public struct ItemEntry
    {
        public string id;
        public string displayName;
        [Min(0)] public int price;
    }

    /// All battle balance numbers in one place (was a block of consts in BattleAgent). Defaults
    /// here equal the original constants, so behaviour is unchanged until you tune them.
    [System.Serializable]
    public class CombatTuning
    {
        [Header("Engagement")]
        [Tooltip("How far a unit notices and chases an enemy.")]
        public float aggroRange = 4f;
        [Tooltip("Melee attack reach.")]
        public float meleeReach = 0.7f;
        [Tooltip("Ranged attack reach.")]
        public float rangedReach = 4f;
        [Tooltip("Seconds between attacks.")]
        public float attackInterval = 1.0f;

        [Header("Passives")]
        [Tooltip("Brute ThickHide — flat damage subtracted from each hit taken.")]
        public int thickHideReduction = 2;
        [Tooltip("Mauler Bloodlust — extra damage per consecutive same-target hit (0.2 = +20%).")]
        public float bloodlustPerHitBonus = 0.20f;
        public int bloodlustMaxStacks = 5;
        [Tooltip("Runner Evasion — chance to dodge a hit entirely (0.3 = 30%).")]
        public float evasionChance = 0.30f;
        [Tooltip("Spitter Corrosion — extra damage the debuffed target takes (0.5 = +50%).")]
        public float corrosionExtraDamage = 0.5f;
        public float corrosionDuration = 4f;
        [Tooltip("Shaman Aura — heal pulse interval / amount / radius for nearby allies.")]
        public float auraTickInterval = 2f;
        public int auraHealAmount = 2;
        public float auraRadius = 2.5f;
        [Tooltip("Bomber SelfDetonate — AoE radius / damage dealt to foes on death.")]
        public float detonateRadius = 1.8f;
        public int detonateDamage = 8;
    }
}
