using System;
using System.Collections.Generic;
using UnityEngine;

/// Input-agnostic farm operations. Click input (TileInteraction) dispatches here today;
/// a walking avatar could call the same Plant/Harvest later without changing this layer.
public class FarmActions : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Inventory inventory;
    [SerializeField] private TileInteraction tileInteraction;
    [SerializeField] private CropInstance cropPrefab;
    [SerializeField] private CropData defaultSeed;
    [SerializeField] private Transform cropParent;

    private readonly Dictionary<Vector3Int, CropInstance> crops = new Dictionary<Vector3Int, CropInstance>();

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (tileInteraction == null) tileInteraction = FindFirstObjectByType<TileInteraction>();
    }

    private void OnEnable()
    {
        if (tileInteraction != null) tileInteraction.CellClicked += OnCellClicked;
    }

    private void OnDisable()
    {
        if (tileInteraction != null) tileInteraction.CellClicked -= OnCellClicked;
    }

    /// Decide what a click on a cell means: harvest a ripe crop, else plant on empty soil.
    private void OnCellClicked(Vector3Int cell)
    {
        if (crops.TryGetValue(cell, out CropInstance crop))
        {
            if (crop.IsRipe) Harvest(cell);
        }
        else
        {
            Plant(cell, defaultSeed);
        }
    }

    public bool Plant(Vector3Int cell, CropData seed)
    {
        if (seed == null || cropPrefab == null) return false;
        if (!gridManager.TryOccupy(cell)) return false;

        Vector3 pos = gridManager.CellCenterToWorld(cell);
        CropInstance crop = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
        crop.Init(seed, DateTime.UtcNow);
        crops[cell] = crop;
        return true;
    }

    public bool Harvest(Vector3Int cell)
    {
        if (!crops.TryGetValue(cell, out CropInstance crop) || !crop.IsRipe) return false;

        inventory.Add(crop.Data.id, crop.Data.yieldCount);
        crops.Remove(cell);
        gridManager.Free(cell);
        Destroy(crop.gameObject);
        Debug.Log($"Harvested {crop.Data.displayName} x{crop.Data.yieldCount}. Inventory total: {inventory.Total}");
        return true;
    }
}
