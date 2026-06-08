using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// One-shot editor helper: generates the six engineered-zombie strains (ZombieData) and the
/// three starting plantable seeds (CropData), then rewires the GameConfig asset's starting
/// seeds + seed catalog to those three. Numbers are the design-intent profile from
/// design/zombies.md — tune freely in the inspector afterwards.
///
/// Run from: Tools > Monster Farm > Setup Zombie Strains. Editor-only; idempotent (re-running
/// updates the existing assets rather than duplicating them).
public static class ZombieStrainsSetup
{
    private const string StrainFolder = "Assets/ScriptableObject/Strains";

    private struct Strain
    {
        public string id;
        public string displayName;
        public string role;
        public int hp;
        public int attack;
        public float moveSpeed;
        public AttackRange range;
        public Passive passive;
        public bool startActive;   // unlocked at start (also gets a plantable seed)
        public float growSeconds;  // seed grow time (starters only)
        public Color ripe;         // ripe / roamer tint
    }

    private static readonly Strain[] Strains =
    {
        new Strain { id = "brute",   displayName = "Brute",   role = "Tank",       hp = 30, attack = 2, moveSpeed = 1.6f, range = AttackRange.Melee,  passive = Passive.ThickHide,    startActive = true,  growSeconds = 12f, ripe = new Color(0.42f, 0.55f, 0.40f) },
        new Strain { id = "mauler",  displayName = "Mauler",  role = "Damage",     hp = 16, attack = 6, moveSpeed = 2.4f, range = AttackRange.Melee,  passive = Passive.Bloodlust,    startActive = true,  growSeconds = 14f, ripe = new Color(0.82f, 0.27f, 0.20f) },
        new Strain { id = "runner",  displayName = "Runner",  role = "Skirmisher", hp = 10, attack = 4, moveSpeed = 4.2f, range = AttackRange.Melee,  passive = Passive.Evasion,      startActive = true,  growSeconds = 8f,  ripe = new Color(0.85f, 0.80f, 0.25f) },
        new Strain { id = "spitter", displayName = "Spitter", role = "Ranged",     hp = 10, attack = 4, moveSpeed = 2.4f, range = AttackRange.Ranged, passive = Passive.Corrosion,    startActive = false, growSeconds = 12f, ripe = new Color(0.45f, 0.70f, 0.35f) },
        new Strain { id = "shaman",  displayName = "Shaman",  role = "Support",    hp = 12, attack = 1, moveSpeed = 2.4f, range = AttackRange.Melee,  passive = Passive.Aura,         startActive = false, growSeconds = 12f, ripe = new Color(0.45f, 0.55f, 0.80f) },
        new Strain { id = "bomber",  displayName = "Bomber",  role = "Burst",      hp = 16, attack = 4, moveSpeed = 2.4f, range = AttackRange.Melee,  passive = Passive.SelfDetonate, startActive = false, growSeconds = 12f, ripe = new Color(0.55f, 0.35f, 0.65f) },
    };

    [MenuItem("Tools/Monster Farm/Setup Zombie Strains")]
    public static void SetupZombieStrains()
    {
        EnsureFolder();

        var seedsById = new Dictionary<string, CropData>();

        var allZombies = new List<ZombieData>();

        foreach (Strain s in Strains)
        {
            ZombieData zombie = LoadOrCreate<ZombieData>($"{StrainFolder}/Zombie_{s.displayName}.asset");
            zombie.id = s.id;
            zombie.displayName = s.displayName;
            zombie.role = s.role;
            zombie.maxHp = s.hp;
            zombie.attack = s.attack;
            zombie.moveSpeed = s.moveSpeed;
            zombie.range = s.range;
            zombie.passive = s.passive;
            zombie.unlockedAtStart = s.startActive;
            zombie.color = s.ripe;
            EditorUtility.SetDirty(zombie);
            allZombies.Add(zombie);

            // Generate a CropData seed for EVERY strain. The 3 starters end up in
            // GameConfig.startingSeeds (free at fresh save); the other 3 only appear in the
            // shop catalog (priced higher). When the task system lands later we'll re-gate
            // them, but for now this lets the player actually use all six.
            CropData seed = LoadOrCreate<CropData>($"{StrainFolder}/Seed_{s.displayName}.asset");
            seed.id = s.id; // seed id == strain id so harvest maps to ZombieData
            seed.displayName = s.displayName;
            seed.growSeconds = s.growSeconds;
            seed.yieldCount = 1;
            seed.seedColor = new Color(0.55f, 0.40f, 0.22f);
            seed.growingColor = new Color(0.40f, 0.70f, 0.30f);
            seed.ripeColor = s.ripe;
            EditorUtility.SetDirty(seed);
            seedsById[s.id] = seed;
        }

        RewireGameConfig(seedsById, allZombies);
        TryPointDefaultSeed(seedsById);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ZombieStrainsSetup] Created/updated 6 strains + 3 starting seeds in " +
                  $"{StrainFolder}, and rewired GameConfig (starting seeds + catalog). " +
                  "To bootstrap a fresh game with the new seeds, delete the old save " +
                  "(Edit interaction / Home building re-saves) or buy seeds in the Shop. " +
                  "Old BasicSeed/BasicZombie assets are now unused and can be deleted.");
    }

    private static void RewireGameConfig(Dictionary<string, CropData> seeds, List<ZombieData> allStrains)
    {
        string[] guids = AssetDatabase.FindAssets("t:GameConfig");
        if (guids.Length == 0) { Debug.LogWarning("[ZombieStrainsSetup] No GameConfig asset found to rewire."); return; }

        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        if (config == null) return;

        config.startingSeeds = new List<GameConfig.SeedStack>
        {
            new GameConfig.SeedStack { seed = seeds["brute"],  count = 3 },
            new GameConfig.SeedStack { seed = seeds["mauler"], count = 3 },
            new GameConfig.SeedStack { seed = seeds["runner"], count = 3 },
        };
        // All 6 strains in the shop; starters cheap, advanced ones priced as premium.
        config.seedCatalog = new List<GameConfig.ShopEntry>
        {
            new GameConfig.ShopEntry { seed = seeds["brute"],   price = 10 },
            new GameConfig.ShopEntry { seed = seeds["runner"],  price = 12 },
            new GameConfig.ShopEntry { seed = seeds["mauler"],  price = 15 },
            new GameConfig.ShopEntry { seed = seeds["spitter"], price = 25 },
            new GameConfig.ShopEntry { seed = seeds["shaman"],  price = 25 },
            new GameConfig.ShopEntry { seed = seeds["bomber"],  price = 30 },
        };
        config.allStrains = new List<ZombieData>(allStrains); // strain id -> stats registry
        if (config.squadCap < 1) config.squadCap = 4;
        EditorUtility.SetDirty(config);
    }

    /// If the open scene has a FarmActions whose default seed is empty or the old basic seed,
    /// point it at the Brute seed so planting works before the seed-pick popup exists.
    private static void TryPointDefaultSeed(Dictionary<string, CropData> seeds)
    {
        if (!seeds.TryGetValue("brute", out CropData brute)) return;

        FarmActions actions = Object.FindFirstObjectByType<FarmActions>();
        if (actions == null) return;

        var so = new SerializedObject(actions);
        SerializedProperty prop = so.FindProperty("defaultSeed");
        if (prop == null) return;

        var current = prop.objectReferenceValue as CropData;
        if (current == null || current.id == "basic")
        {
            prop.objectReferenceValue = brute;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(actions);
            EditorSceneManager.MarkSceneDirty(actions.gameObject.scene);
        }
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObject"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObject");
        if (!AssetDatabase.IsValidFolder(StrainFolder))
            AssetDatabase.CreateFolder("Assets/ScriptableObject", "Strains");
    }
}
