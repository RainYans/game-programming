using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// Assigns sliced sprites from the "Isometric Assets 1" sheet to GroundTile / FieldTile, and
/// sets the sheet's Pixels Per Unit so each tile's diamond fills one cell. Run from:
/// Tools > Zombie Farm > Apply Farm Sheet Tiles. Editor-only.
///
/// The sheet must already be sliced (Sprite Mode = Multiple, Grid By Cell Size 256x256). The
/// grid-slice's default center pivot is ~correct for these tiles, so we don't touch pivots.
public static class FarmSheetTiles
{
    private const string SheetPath = "Assets/Art/Tiles/FarmTile/Isometric Assets 1.png";
    private const string GrassSprite = "Isometric Assets 1_97";
    private const string DirtSprite = "Isometric Assets 1_99";

    private const string GroundTileAsset = "Assets/Tiles/GroundTile.asset";
    private const string FieldTileAsset = "Assets/Tiles/FieldTile.asset";

    // The tile's diamond is ~220px wide inside the 256 cell. PPU = diamond width makes it one
    // cell wide. Lower this if tiles leave gaps; raise it if they overlap.
    private const float PixelsPerUnit = 222f;

    [MenuItem("Tools/Zombie Farm/Apply Farm Sheet Tiles")]
    public static void Apply()
    {
        var importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[FarmSheetTiles] Sheet not found / not a texture: {SheetPath}");
            return;
        }
        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogWarning("[FarmSheetTiles] Slice the sheet first (Sprite Mode = Multiple, " +
                             "Grid By Cell Size 256x256). Aborting.");
            return;
        }

        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.SaveAndReimport();

        Sprite grass = FindSprite(GrassSprite);
        Sprite dirt = FindSprite(DirtSprite);
        if (grass == null || dirt == null)
        {
            Debug.LogWarning($"[FarmSheetTiles] Couldn't find '{GrassSprite}' / '{DirtSprite}' " +
                             "among the sliced sprites — check the names.");
            return;
        }

        AssignSprite(GroundTileAsset, grass);
        AssignSprite(FieldTileAsset, dirt);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FarmSheetTiles] GroundTile = grass (_97), FieldTile = dirt (_99). The painted " +
                  "map updates automatically. If tiles leave gaps/overlap, tweak PixelsPerUnit in " +
                  "FarmSheetTiles.cs; if they sit too high/low, nudge the GroundTilemap's Tile Anchor.");
    }

    private static Sprite FindSprite(string spriteName)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetRepresentationsAtPath(SheetPath))
            if (o is Sprite s && s.name == spriteName) return s;
        return null;
    }

    private static void AssignSprite(string tilePath, Sprite sprite)
    {
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null) { Debug.LogWarning($"[FarmSheetTiles] Tile not found: {tilePath}"); return; }
        tile.sprite = sprite;
        tile.color = Color.white;
        EditorUtility.SetDirty(tile);
    }
}
