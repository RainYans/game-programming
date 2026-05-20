using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// Hover highlight + click detection on farm cells. Reports the clicked cell via
/// CellClicked; gameplay actions (plant/harvest) subscribe to it later.
public class TileInteraction : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer highlight;

    public event Action<Vector3Int> CellClicked;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (highlight != null) highlight.enabled = false;
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || cam == null || gridManager == null) return;

        Vector3 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        world.z = 0f; // tilemap sits on the z=0 plane; keep cell lookup on it
        Vector3Int cell = gridManager.WorldToCell(world);
        bool onFarm = gridManager.IsFarmCell(cell);

        UpdateHighlight(cell, onFarm);

        if (onFarm && mouse.leftButton.wasPressedThisFrame)
            CellClicked?.Invoke(cell);
    }

    private void UpdateHighlight(Vector3Int cell, bool onFarm)
    {
        if (highlight == null) return;
        highlight.enabled = onFarm;
        if (onFarm)
            highlight.transform.position = gridManager.CellCenterToWorld(cell);
    }
}
