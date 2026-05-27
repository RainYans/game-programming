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
    [SerializeField] private FarmActions farmActions;
    [SerializeField] private GameConfig config;
    [SerializeField] private string fileName = "save.json";

    /// Bump when the save layout changes incompatibly. Load tolerates older/missing versions
    /// (fields are added additively), and warns if a save is from a newer build.
    private const int CurrentSaveVersion = 1;

    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
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
    }

    private void Unsubscribe()
    {
        if (wallet != null) wallet.Changed -= Save;
        if (inventory != null) inventory.Changed -= Save;
        if (seedInventory != null) seedInventory.Changed -= Save;
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
        public List<CropEntry> crops = new List<CropEntry>();
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
