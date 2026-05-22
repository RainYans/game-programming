using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// Hover highlight + click detection on farm cells.
/// Uses a dedicated highlight tilemap layer for reliable, visible highlighting.
public class TileInteraction : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera cam;

    public event Action<Vector3Int> CellClicked;

    private Vector3Int lastCell;
    private bool lastWasFarm;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || cam == null || gridManager == null) return;

        Vector3 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        world.z = 0f;
        Vector3Int cell = gridManager.WorldToCell(world);
        bool onFarm = gridManager.IsFarmCell(cell);

        UpdateHighlight(cell, onFarm);

        if (onFarm && mouse.leftButton.wasPressedThisFrame)
            CellClicked?.Invoke(cell);
    }

    private void UpdateHighlight(Vector3Int cell, bool onFarm)
    {
        if (gridManager == null) return;

        // Clear previous highlight
        if (lastWasFarm && lastCell != cell)
            gridManager.ClearHighlight(lastCell);

        // Set new highlight
        if (onFarm && (!lastWasFarm || lastCell != cell))
            gridManager.SetHighlight(cell);

        lastCell = cell;
        lastWasFarm = onFarm;
    }

    private void OnDisable()
    {
        if (gridManager != null && lastWasFarm)
            gridManager.ClearHighlight(lastCell);
        lastWasFarm = false;
    }
}
