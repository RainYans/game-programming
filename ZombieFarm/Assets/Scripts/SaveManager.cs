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
        var data = new SaveData { resources = wallet != null ? wallet.Resources : 0 };

        if (inventory != null)
            foreach (KeyValuePair<string, int> kv in inventory.Entries)
                data.inventory.Add(new CountEntry { id = kv.Key, count = kv.Value });

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

        if (wallet != null) wallet.SetResources(data.resources);

        if (inventory != null)
        {
            inventory.Clear();
            foreach (CountEntry e in data.inventory) inventory.Add(e.id, e.count);
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
        public int resources;
        public List<CountEntry> inventory = new List<CountEntry>();
        public List<CountEntry> seeds = new List<CountEntry>();
        public List<CropEntry> crops = new List<CropEntry>();
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
