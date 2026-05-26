using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// One-shot editor helper: drops VISIBLE placeholder scenery (a wall ring, mountains, a river)
/// around the painted ground, plus the invisible containment boundary. Everything is a colored
/// placeholder with a collider — swap the SpriteRenderer's sprite for real art later, or drag
/// pieces around. Run from: Tools > Zombie Farm > Setup Scenery (Placeholder). Idempotent.
public static class ScenerySetup
{
    private const string SceneryRoot = "Scenery";

    [MenuItem("Tools/Zombie Farm/Setup Scenery (Placeholder)")]
    public static void SetupScenery()
    {
        GameObject groundGo = GameObject.Find("GroundTilemap");
        Tilemap ground = groundGo != null ? groundGo.GetComponent<Tilemap>() : null;
        GameObject gridGo = GameObject.Find("FarmGrid");
        Grid grid = gridGo != null ? gridGo.GetComponent<Grid>() : null;
        if (ground == null || grid == null)
        {
            Debug.LogWarning("[ScenerySetup] Need 'GroundTilemap' and 'FarmGrid'. Aborting.");
            return;
        }

        // Continuous, gap-free containment wall (invisible). The visible wall posts sit on it.
        MapBoundarySetup.SetupBoundary();

        ground.CompressBounds();
        BoundsInt cb = ground.cellBounds;
        Vector3 bottom = grid.GetCellCenterWorld(new Vector3Int(cb.xMin, cb.yMin, 0));
        Vector3 right = grid.GetCellCenterWorld(new Vector3Int(cb.xMax - 1, cb.yMin, 0));
        Vector3 top = grid.GetCellCenterWorld(new Vector3Int(cb.xMax - 1, cb.yMax - 1, 0));
        Vector3 left = grid.GetCellCenterWorld(new Vector3Int(cb.xMin, cb.yMax - 1, 0));
        Vector3 center = (bottom + right + top + left) * 0.25f;

        // Fresh root (idempotent).
        GameObject old = GameObject.Find(SceneryRoot);
        if (old != null) Undo.DestroyObjectImmediate(old);
        var root = new GameObject(SceneryRoot);
        Undo.RegisterCreatedObjectUndo(root, "Create Scenery");

        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var wallColor = new Color(0.45f, 0.42f, 0.40f);
        var rockColor = new Color(0.52f, 0.52f, 0.56f);

        // Wall ring (visible only; collision is the MapBoundary loop above).
        PlaceWall(root.transform, bottom, right, sprite, wallColor);
        PlaceWall(root.transform, right, top, sprite, wallColor);
        PlaceWall(root.transform, top, left, sprite, wallColor);
        PlaceWall(root.transform, left, bottom, sprite, wallColor);

        // Mountains — solid, tucked just inside three corners.
        CreateProp(root.transform, "Mountain", Vector3.Lerp(top, center, 0.14f), rockColor, 2.6f, 7, true, sprite);
        CreateProp(root.transform, "Mountain", Vector3.Lerp(right, center, 0.14f), rockColor, 2.2f, 7, true, sprite);
        CreateProp(root.transform, "Mountain", Vector3.Lerp(left, center, 0.14f), rockColor, 2.0f, 7, true, sprite);

        // River is now a painted RiverTile on the tilemap (see TileArtSetup), not a prop here.

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Debug.Log("[ScenerySetup] Placeholder walls / mountains created under 'Scenery', plus the " +
                  "invisible boundary. (River is a painted RiverTile now.) Drag pieces to taste " +
                  "and swap sprites for real art later. Save the scene (Ctrl+S).");
    }

    private static void PlaceWall(Transform parent, Vector3 a, Vector3 b, Sprite sprite, Color color)
    {
        int n = Mathf.Max(2, Mathf.RoundToInt(Vector3.Distance(a, b) / 1.2f));
        for (int i = 0; i <= n; i++)
        {
            Vector3 p = Vector3.Lerp(a, b, (float)i / n);
            CreateProp(parent, "Wall", p, color, 0.5f, 6, false, sprite);
        }
    }

    private static void CreateProp(Transform parent, string name, Vector3 pos, Color color,
                                   float scale, int order, bool solid, Sprite sprite)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;

        if (solid)
        {
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 0.9f);
        }
    }
}
