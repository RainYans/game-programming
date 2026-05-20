using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// World <-> cell conversion and per-cell occupancy for the farm grid.
public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap farmTilemap;

    private readonly HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();

    private void Awake()
    {
        if (grid == null) grid = GetComponentInChildren<Grid>();
        if (farmTilemap == null) farmTilemap = GetComponentInChildren<Tilemap>();
    }

    public Vector3Int WorldToCell(Vector3 world) => grid.WorldToCell(world);

    public Vector3 CellCenterToWorld(Vector3Int cell) => grid.GetCellCenterWorld(cell);

    /// True if the cell is part of the painted farm (has a tile).
    public bool IsFarmCell(Vector3Int cell) => farmTilemap != null && farmTilemap.HasTile(cell);

    public bool IsOccupied(Vector3Int cell) => occupied.Contains(cell);

    /// Reserve a cell (e.g. when planting). Fails if not farmable or already taken.
    public bool TryOccupy(Vector3Int cell)
    {
        if (!IsFarmCell(cell) || occupied.Contains(cell)) return false;
        occupied.Add(cell);
        return true;
    }

    public void Free(Vector3Int cell) => occupied.Remove(cell);
}
