using System;
using System.Collections.Generic;
using UnityEngine;

/// Input-agnostic farm operations. The walking avatar (AvatarInteraction) dispatches Interact
/// here; the logic stays decoupled from input.
public class FarmActions : MonoBehaviour
{
    /// Raised when the player interacts with empty, plantable soil — the UI (SeedPickPopup)
    /// listens and asks which strain to sow, then calls Plant(cell, seed). If nothing is
    /// listening, Interact falls back to planting the defaultSeed so the farm still works.
    public event Action<Vector3Int> PlantRequested;

    [SerializeField] private GridManager gridManager;
    [SerializeField] private Inventory inventory;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private CropInstance cropPrefab;
    [SerializeField] private CropData defaultSeed;
    [SerializeField] private Transform cropParent;

    private readonly Dictionary<Vector3Int, CropInstance> crops = new Dictionary<Vector3Int, CropInstance>();

    /// Read-only view of planted crops by cell, for the save system to serialize.
    public IReadOnlyDictionary<Vector3Int, CropInstance> Crops => crops;

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
    }

    /// Decide what an interaction on a cell means: harvest a ripe crop, else plant on empty
    /// soil. Dispatched by the walking avatar (AvatarInteraction).
    public void Interact(Vector3Int cell)
    {
        if (crops.TryGetValue(cell, out CropInstance crop))
        {
            if (crop.IsRipe) Harvest(cell);
        }
        else if (PlantRequested != null)
        {
            // Defer the seed choice to the UI (seed-pick popup).
            PlantRequested.Invoke(cell);
        }
        else
        {
            // No UI wired — keep the farm usable by sowing the default strain.
            Plant(cell, defaultSeed);
        }
    }

    public bool Plant(Vector3Int cell, CropData seed)
    {
        if (seed == null || cropPrefab == null) return false;
        if (seedInventory == null || seedInventory.Get(seed.id) <= 0)
        {
            var toast = FindFirstObjectByType<MessageToast>();
            if (toast != null) toast.Show($"No {seed?.displayName} seeds! Buy more at the shop.");
            return false;
        }
        if (!gridManager.TryOccupy(cell)) return false;

        Vector3 pos = gridManager.CellCenterToWorld(cell);
        CropInstance crop = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
        crop.Init(seed, DateTime.UtcNow);
        crops[cell] = crop;

        // Consume the seed last: this fires SeedInventory.Changed (which autosaves), and by
        // now the new crop is already in the dictionary so the save captures it.
        seedInventory.TryRemove(seed.id, 1);
        SfxManager.Play(SfxKind.Plant);
        return true;
    }

    public bool Harvest(Vector3Int cell)
    {
        if (!crops.TryGetValue(cell, out CropInstance crop) || !crop.IsRipe) return false;

        CropData data = crop.Data;
        crops.Remove(cell);
        gridManager.Free(cell);
        Destroy(crop.gameObject);

        // Add to inventory last: this fires Inventory.Changed (which autosaves), and by now
        // the harvested crop is already gone from the dictionary so the save reflects that.
        inventory.Add(data.id, data.yieldCount);
        SfxManager.Play(SfxKind.Harvest);
        Debug.Log($"Harvested {data.displayName} x{data.yieldCount}. Inventory total: {inventory.Total}");
        return true;
    }

    /// Re-create a crop from saved state without consuming a seed (used by SaveManager.Load).
    public void SpawnSavedCrop(Vector3Int cell, CropData seed, DateTime plantedUtc)
    {
        if (seed == null || cropPrefab == null) return;
        if (!gridManager.TryOccupy(cell)) return;

        Vector3 pos = gridManager.CellCenterToWorld(cell);
        CropInstance crop = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
        crop.Init(seed, plantedUtc);
        crops[cell] = crop;
    }

    /// Remove every planted crop and free its cell (used by SaveManager.Load before restoring).
    public void ClearAllCrops()
    {
        foreach (KeyValuePair<Vector3Int, CropInstance> kv in crops)
        {
            gridManager.Free(kv.Key);
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        }
        crops.Clear();
    }
}
