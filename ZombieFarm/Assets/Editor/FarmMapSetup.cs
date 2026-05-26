using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// Repaints the farm to a larger layout on the GroundTilemap: a big walkable GroundTile area
/// with a modest, centered FieldTile (plantable) block. Run from:
/// Tools > Zombie Farm > Rebuild Farm Map. Editor-only.
///
/// NOTE: this CLEARS and repaints the GroundTilemap (placeholder tiles only). The plantable
/// area is kept modest on purpose so the planned "spend currency to expand plots" feature
/// still has a job. Adjust the two sizes below and re-run to resize.
public static class FarmMapSetup
{
    private const int GroundSize = 20; // ground is GroundSize x GroundSize cells (a diamond in iso)
    private const int FieldSize = 6;   // central plantable block; the rest is open ground
                                       // (room for buildings + future trees/landscaping)

    [MenuItem("Tools/Zombie Farm/Rebuild Farm Map")]
    public static void RebuildMap()
    {
        GameObject groundGo = GameObject.Find("GroundTilemap");
        if (groundGo == null)
        {
            Debug.LogWarning("[FarmMapSetup] No 'GroundTilemap' GameObject found. Aborting.");
            return;
        }
        Tilemap ground = groundGo.GetComponent<Tilemap>();
        if (ground == null)
        {
            Debug.LogWarning("[FarmMapSetup] 'GroundTilemap' has no Tilemap component. Aborting.");
            return;
        }

        TileBase groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/GroundTile.asset");
        TileBase fieldTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/FieldTile.asset");
        if (groundTile == null || fieldTile == null)
        {
            Debug.LogWarning("[FarmMapSetup] Could not load GroundTile/FieldTile from Assets/Tiles/. Aborting.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(ground, "Rebuild Farm Map");
        ground.ClearAllTiles();

        int fieldStart = (GroundSize - FieldSize) / 2;
        int fieldEnd = fieldStart + FieldSize - 1;

        for (int x = 0; x < GroundSize; x++)
        {
            for (int y = 0; y < GroundSize; y++)
            {
                bool isField = x >= fieldStart && x <= fieldEnd && y >= fieldStart && y <= fieldEnd;
                ground.SetTile(new Vector3Int(x, y, 0), isField ? fieldTile : groundTile);
            }
        }

        EditorUtility.SetDirty(ground);
        EditorSceneManager.MarkSceneDirty(groundGo.scene);

        Debug.Log($"[FarmMapSetup] Rebuilt: {GroundSize}x{GroundSize} ground with a centered " +
                  $"{FieldSize}x{FieldSize} plantable field (cells {fieldStart}..{fieldEnd}). " +
                  "Edit GroundSize/FieldSize in FarmMapSetup.cs and re-run to resize. Save the scene (Ctrl+S).");
    }
}
