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

    [Header("Seed shop")]
    public List<ShopEntry> seedCatalog = new List<ShopEntry>();

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
}
