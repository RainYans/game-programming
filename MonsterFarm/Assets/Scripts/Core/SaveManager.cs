using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// Persists the whole game state to JSON in persistentDataPath and restores it on launch.
/// Autosaves whenever the wallet, inventory, or seed stock changes — which covers harvest,
/// mission reward, purchase, and planting. Loads before subscribing, so restoring state
/// never re-triggers a save.
public class SaveManager : MonoBehaviour
{
    [SerializeField] private Wallet wallet;
    [SerializeField] private Inventory inventory;          // harvested zombies
    [SerializeField] private SeedInventory seedInventory;  // seed stock
    [SerializeField] private ItemInventory itemInventory;  // combat items (e.g. Rotten Onion)
    [SerializeField] private CityProgress cityProgress;    // which cities are cleared
    [SerializeField] private LabManager labManager;        // per-strain Lab upgrade levels
    [SerializeField] private FarmActions farmActions;
    [SerializeField] private GameConfig config;
    [SerializeField] private string fileName = "save.json";

    /// Bump when the save layout changes incompatibly. Load tolerates older/missing versions
    /// (fields are added additively), and warns if a save is from a newer build.
    private const int CurrentSaveVersion = 2;

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    /// Default save location, usable from scenes that have no SaveManager instance (e.g. the main
    /// menu deciding whether "Continue" is available, or "New Game" wiping the slot).
    public const string DefaultFileName = "save.json";
    public static string DefaultSavePath => Path.Combine(Application.persistentDataPath, DefaultFileName);
    public static bool HasSave() => File.Exists(DefaultSavePath);
    public static void DeleteSave()
    {
        try { if (File.Exists(DefaultSavePath)) File.Delete(DefaultSavePath); }
        catch (Exception ex) { Debug.LogWarning($"Could not delete save: {ex.Message}"); }
    }

    private void Awake()
    {
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
        if (itemInventory == null) itemInventory = FindFirstObjectByType<ItemInventory>();
        if (cityProgress == null) cityProgress = FindFirstObjectByType<CityProgress>();
        if (labManager == null) labManager = FindFirstObjectByType<LabManager>();
        if (farmActions == null) farmActions = FindFirstObjectByType<FarmActions>();
    }

    private void Start()
    {
        if (File.Exists(SavePath)) Load();
        else ApplyDefaults();
        Subscribe();
    }

    private void OnDestroy() => Unsubscribe();

    private void OnApplicationQuit() => Save();

    private void Subscribe()
    {
        if (wallet != null) wallet.Changed += Save;
        if (inventory != null) inventory.Changed += Save;
        if (seedInventory != null) seedInventory.Changed += Save;
        if (itemInventory != null) itemInventory.Changed += Save;
        if (cityProgress != null) cityProgress.Changed += Save;
        if (labManager != null) labManager.Changed += Save;
    }

    private void Unsubscribe()
    {
        if (wallet != null) wallet.Changed -= Save;
        if (inventory != null) inventory.Changed -= Save;
        if (seedInventory != null) seedInventory.Changed -= Save;
        if (itemInventory != null) itemInventory.Changed -= Save;
        if (cityProgress != null) cityProgress.Changed -= Save;
        if (labManager != null) labManager.Changed -= Save;
    }

    public void Save()
    {
        var data = new SaveData { version = CurrentSaveVersion, resources = wallet != null ? wallet.Resources : 0 };

        if (inventory != null)
            foreach (ZombieUnit u in inventory.Units)
                data.zombies.Add(new ZombieUnitEntry
                {
                    uid = u.uid,
                    strainId = u.strainId,
                    becameFullAtUtcMs = u.becameFullAtUtcMs
                });

        if (seedInventory != null)
            foreach (KeyValuePair<string, int> kv in seedInventory.Entries)
                data.seeds.Add(new CountEntry { id = kv.Key, count = kv.Value });

        if (itemInventory != null)
            foreach (KeyValuePair<string, int> kv in itemInventory.Entries)
                data.items.Add(new CountEntry { id = kv.Key, count = kv.Value });

        if (cityProgress != null)
            data.clearedCities.AddRange(cityProgress.Cleared);

        if (farmActions != null)
            foreach (KeyValuePair<Vector3Int, CropInstance> kv in farmActions.Crops)
            {
                if (kv.Value == null || kv.Value.Data == null) continue;
                data.crops.Add(new CropEntry
                {
                    x = kv.Key.x,
                    y = kv.Key.y,
                    z = kv.Key.z,
                    cropId = kv.Value.Data.id,
                    plantedUnixMs = new DateTimeOffset(kv.Value.PlantedAtUtc, TimeSpan.Zero).ToUnixTimeMilliseconds()
                });
            }

        if (labManager != null)
            foreach (KeyValuePair<string, int> kv in labManager.Levels)
                data.strainUpgrades.Add(new CountEntry { id = kv.Key, count = kv.Value });

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public void Load()
    {
        SaveData data;
        try
        {
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Save file unreadable ({ex.Message}); applying fresh-save defaults.");
            ApplyDefaults();
            return;
        }
        if (data == null) { ApplyDefaults(); return; }

        if (data.version > CurrentSaveVersion)
            Debug.LogWarning($"Save version {data.version} is newer than supported " +
                             $"{CurrentSaveVersion}; loading anyway, some data may be ignored.");

        if (wallet != null) wallet.SetResources(data.resources);

        if (inventory != null)
        {
            var loaded = new List<ZombieUnit>();
            if (data.zombies.Count > 0)
            {
                foreach (ZombieUnitEntry e in data.zombies)
                {
                    DateTime fullAt = DateTimeOffset.FromUnixTimeMilliseconds(e.becameFullAtUtcMs).UtcDateTime;
                    loaded.Add(new ZombieUnit(e.strainId, fullAt, e.uid));
                }
            }
            else
            {
                // Migrate a pre-hunger save (plain id->count) into individual units, all Full now.
                foreach (CountEntry e in data.inventory)
                    for (int i = 0; i < e.count; i++) loaded.Add(new ZombieUnit(e.id, DateTime.UtcNow));
            }
            inventory.LoadUnits(loaded);
        }

        if (seedInventory != null)
        {
            seedInventory.Clear();
            foreach (CountEntry e in data.seeds) seedInventory.Add(e.id, e.count);
        }

        if (itemInventory != null)
        {
            itemInventory.Clear();
            foreach (CountEntry e in data.items) itemInventory.Add(e.id, e.count);
        }

        if (cityProgress != null)
            cityProgress.LoadCleared(data.clearedCities);

        if (labManager != null)
        {
            var lv = new Dictionary<string, int>();
            foreach (CountEntry e in data.strainUpgrades) lv[e.id] = e.count;
            labManager.LoadLevels(lv);
        }

        if (farmActions != null)
        {
            farmActions.ClearAllCrops();
            foreach (CropEntry c in data.crops)
            {
                CropData seed = config != null ? config.FindSeed(c.cropId) : null;
                if (seed == null) continue;
                DateTime planted = DateTimeOffset.FromUnixTimeMilliseconds(c.plantedUnixMs).UtcDateTime;
                farmActions.SpawnSavedCrop(new Vector3Int(c.x, c.y, c.z), seed, planted);
            }
        }
    }

    /// Seed a brand-new game from GameConfig: starting resources + starting seeds, empty farm.
    private void ApplyDefaults()
    {
        if (config == null) return;

        if (wallet != null) wallet.SetResources(config.startingResources);

        if (seedInventory != null)
        {
            seedInventory.Clear();
            foreach (GameConfig.SeedStack s in config.startingSeeds)
                if (s.seed != null && s.count > 0) seedInventory.Add(s.seed.id, s.count);
        }
    }

    [Serializable]
    public class SaveData
    {
        public int version;   // 0 = pre-versioning legacy save
        public int resources;
        public List<ZombieUnitEntry> zombies = new List<ZombieUnitEntry>();
        public List<CountEntry> inventory = new List<CountEntry>(); // legacy pre-hunger counts; read-only for migration
        public List<CountEntry> seeds = new List<CountEntry>();
        public List<CountEntry> items = new List<CountEntry>(); // combat items (e.g. Rotten Onion)
        public List<string> clearedCities = new List<string>(); // MissionData.ids the player cleared
        public List<CropEntry> crops = new List<CropEntry>();
        public List<CountEntry> strainUpgrades = new List<CountEntry>(); // strainId -> Lab upgrade level
    }

    [Serializable]
    public struct ZombieUnitEntry
    {
        public string uid;
        public string strainId;
        public long becameFullAtUtcMs;
    }

    [Serializable]
    public struct CountEntry
    {
        public string id;
        public int count;
    }

    [Serializable]
    public struct CropEntry
    {
        public int x;
        public int y;
        public int z;
        public string cropId;
        public long plantedUnixMs;
    }
}
