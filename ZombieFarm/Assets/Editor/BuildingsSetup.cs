using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// One-shot editor helper: removes the old buildings (and their broken labels) and creates
/// clean Shop / Lab / WarCamp placeholders around the field, each with a trigger collider and
/// a Building component so the avatar can open them by walking up and pressing E.
/// Run from: Tools > Zombie Farm > Setup Buildings. Editor-only; idempotent.
///
/// Placeholders use a built-in sprite tinted per building — drop your own art onto each
/// building's SpriteRenderer later. No text labels (they caused the far-click selection bug).
public static class BuildingsSetup
{
    // Cells to place buildings on (the field is the central block; these sit on the surrounding
    // ground). Adjust if you change the map size in FarmMapSetup.
    // A "base camp" row just south of the field (field is cells 7..12 in a 20x20 ground).
    private static readonly Vector3Int HomeCell = new Vector3Int(4, 4, 0);
    private static readonly Vector3Int ShopCell = new Vector3Int(7, 4, 0);
    private static readonly Vector3Int LabCell = new Vector3Int(10, 4, 0);
    private static readonly Vector3Int WarCampCell = new Vector3Int(13, 4, 0);

    [MenuItem("Tools/Zombie Farm/Setup Buildings")]
    public static void SetupBuildings()
    {
        // 1. Remove old + any previously-created buildings (idempotent).
        DeleteIfExists("ShopBuilding");
        DeleteIfExists("BattleGate");
        DeleteIfExists("Home");
        DeleteIfExists("Shop");
        DeleteIfExists("Lab");
        DeleteIfExists("WarCamp");

        // 2. Need the grid to place buildings at cell centers.
        GameObject gridGo = GameObject.Find("FarmGrid");
        Grid grid = gridGo != null ? gridGo.GetComponent<Grid>() : null;
        if (grid == null)
        {
            Debug.LogWarning("[BuildingsSetup] No 'FarmGrid' with a Grid component found. Aborting.");
            return;
        }

        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        CreateBuilding("Home", BuildingType.Home, HomeCell, new Color(1.00f, 0.85f, 0.45f), grid, sprite);
        CreateBuilding("Shop", BuildingType.Shop, ShopCell, new Color(0.40f, 0.80f, 0.45f), grid, sprite);
        CreateBuilding("Lab", BuildingType.Lab, LabCell, new Color(0.45f, 0.60f, 1.00f), grid, sprite);
        CreateBuilding("WarCamp", BuildingType.WarCamp, WarCampCell, new Color(1.00f, 0.50f, 0.40f), grid, sprite);

        EditorSceneManager.MarkSceneDirty(gridGo.scene);
        Debug.Log("[BuildingsSetup] Created Home / Shop / Lab / WarCamp around the field. " +
                  "Walk the avatar up to one and press E to open it (Shop works; WarCamp opens " +
                  "the deploy/battle page; Lab shows a placeholder). Save the scene (Ctrl+S).");
    }

    private static void DeleteIfExists(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) Undo.DestroyObjectImmediate(go);
    }

    private static void CreateBuilding(string name, BuildingType type, Vector3Int cell, Color color,
                                       Grid grid, Sprite sprite)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.position = grid.GetCellCenterWorld(cell);
        go.transform.localScale = Vector3.one * 1.5f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 10;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = false; // solid: blocks the avatar (E-open is distance-based, not the collider)
        col.size = new Vector2(0.33f, 0.33f); // local; x1.5 scale ≈ 0.5 world units (~half a cell)

        var building = go.AddComponent<Building>();
        building.type = type;

        EditorUtility.SetDirty(go);
    }
}
