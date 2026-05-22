using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// World <-> cell conversion and per-cell occupancy for the farm grid.
public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase fieldTile;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private TileBase highlightTile;

    private readonly HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();

    private void Awake()
    {
        if (grid == null) grid = GetComponentInChildren<Grid>();
        if (tilemap == null) tilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetHighlight(Vector3Int cell)
    {
        if (highlightTilemap != null && highlightTile != null)
            highlightTilemap.SetTile(cell, highlightTile);
    }

    public void ClearHighlight(Vector3Int cell)
    {
        if (highlightTilemap != null)
            highlightTilemap.SetTile(cell, null);
    }

    public Vector3Int WorldToCell(Vector3 world) => grid.WorldToCell(world);

    public Vector3 CellCenterToWorld(Vector3Int cell) => grid.GetCellCenterWorld(cell);

    /// Only cells with a FieldTile (dirt) are plantable.
    public bool IsFarmCell(Vector3Int cell) =>
        tilemap != null && fieldTile != null && tilemap.GetTile(cell) == fieldTile;

    public bool IsOccupied(Vector3Int cell) => occupied.Contains(cell);

    /// Reserve a cell (e.g. when planting). Fails if not farmable or already taken.
    public bool TryOccupy(Vector3Int cell)
    {
        if (!IsFarmCell(cell) || occupied.Contains(cell)) return false;
        occupied.Add(cell);
        return true;
    }

    public void Free(Vector3Int cell) => occupied.Remove(cell);

    /// Set a per-cell colour tint.  Pass Color.white to reset to the tile asset colour.
    public void SetTileColor(Vector3Int cell, Color color)
    {
        if (tilemap != null) tilemap.SetColor(cell, color);
    }
}
