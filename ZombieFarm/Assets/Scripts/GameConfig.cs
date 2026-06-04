using System.Collections.Generic;
using UnityEngine;

/// Cross-cutting, designer-tunable economy numbers for M4 in one asset. Per-strain stats
/// stay on CropData / ZombieData; this holds the bootstrap values and the seed catalog —
/// which doubles as the id -> CropData registry the save system uses to rebuild crops.
[CreateAssetMenu(fileName = "GameConfig", menuName = "ZombieFarm/Game Config")]
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

    [Header("Strains & combat")]
    [Tooltip("Every strain's ZombieData, for resolving a saved strain id back to its stats " +
             "(deploy screen, battle). Populated by Tools > Zombie Farm > Setup Zombie Strains.")]
    public List<ZombieData> allStrains = new List<ZombieData>();

    [Tooltip("Max zombies the player can deploy in one squad.")]
    [Min(1)] public int squadCap = 4;

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

    /// Stable id of the Rotten Onion combat item — the single shop item wired end-to-end for
    /// the MVP gate (buy -> carry -> throw -> consume). Shared by the shop, deploy, and battle.
    public const string RottenOnionId = "rotten_onion";

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
}
