using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// One-shot editor helper: builds an invisible collision wall around the painted ground so the
/// avatar can't walk off the map. It reads the GroundTilemap's painted bounds and lays a closed
/// EdgeCollider2D loop just outside them. Run from: Tools > Zombie Farm > Setup Map Boundary.
/// Editor-only; idempotent. Reshape the loop afterwards with the collider's "Edit Collider".
///
/// This is the *function* of a wall (containment). The visible look (walls / mountains / rivers)
/// is art — add those sprites later once assets are in.
public static class MapBoundarySetup
{
    private const string BoundaryName = "MapBoundary";
    private const float Margin = 1.08f; // push the wall slightly outside the painted tiles

    [MenuItem("Tools/Zombie Farm/Setup Map Boundary")]
    public static void SetupBoundary()
    {
        GameObject groundGo = GameObject.Find("GroundTilemap");
        Tilemap ground = groundGo != null ? groundGo.GetComponent<Tilemap>() : null;
        GameObject gridGo = GameObject.Find("FarmGrid");
        Grid grid = gridGo != null ? gridGo.GetComponent<Grid>() : null;
        if (ground == null || grid == null)
        {
            Debug.LogWarning("[MapBoundarySetup] Need 'GroundTilemap' and 'FarmGrid'. Aborting.");
            return;
        }

        ground.CompressBounds(); // tighten cellBounds to the actually-painted cells
        BoundsInt cb = ground.cellBounds;

        // The 4 corner cells of the painted rectangle map to the 4 vertices of the iso diamond.
        Vector3 bottom = grid.GetCellCenterWorld(new Vector3Int(cb.xMin, cb.yMin, 0));
        Vector3 right = grid.GetCellCenterWorld(new Vector3Int(cb.xMax - 1, cb.yMin, 0));
        Vector3 top = grid.GetCellCenterWorld(new Vector3Int(cb.xMax - 1, cb.yMax - 1, 0));
        Vector3 left = grid.GetCellCenterWorld(new Vector3Int(cb.xMin, cb.yMax - 1, 0));

        Vector3 center = (bottom + right + top + left) * 0.25f;
        Vector2 P(Vector3 v) => (Vector2)(center + (v - center) * Margin);

        GameObject boundary = GameObject.Find(BoundaryName);
        if (boundary == null)
        {
            boundary = new GameObject(BoundaryName);
            Undo.RegisterCreatedObjectUndo(boundary, "Create MapBoundary");
        }
        boundary.transform.position = Vector3.zero; // keep at origin so collider points == world

        EdgeCollider2D edge = boundary.GetComponent<EdgeCollider2D>();
        if (edge == null) edge = Undo.AddComponent<EdgeCollider2D>(boundary);

        // Closed loop: repeat the first point at the end so the avatar is fully enclosed.
        edge.points = new[] { P(bottom), P(right), P(top), P(left), P(bottom) };

        EditorUtility.SetDirty(boundary);
        EditorSceneManager.MarkSceneDirty(boundary.scene);
        Selection.activeGameObject = boundary;

        Debug.Log("[MapBoundarySetup] Boundary wall created around the painted ground. The avatar " +
                  "now collides with it. Fine-tune with the EdgeCollider2D's 'Edit Collider', then " +
                  "save the scene (Ctrl+S). Re-run after painting more ground to refit it.");
    }
}
