using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// Imports the chosen Kenney isometric block tiles at the right scale/pivot and assigns them to
/// GroundTile / FieldTile. Run from: Tools > Zombie Farm > Apply Ground & Field Tiles.
///
/// These are CUBE tiles (a 2:1 diamond top face + block sides below). To tessellate on the
/// isometric grid, the sprite pivot must sit at the center of the TOP face and PPU must make
/// that face one cell wide. We compute both from the texture size, so the painted map updates
/// to grass/dirt automatically. If tiles look offset after running, nudge the pivot in the
/// Sprite Editor (the formula assumes the top diamond spans the full image width).
public static class TileArtSetup
{
    private const string GrassPng = "Assets/Art/Tiles/PNG/landscapeTiles_067.png";
    private const string DirtPng = "Assets/Art/Tiles/PNG/landscapeTiles_083.png";
    private const string RiverPng = "Assets/Art/Tiles/PNG/landscapeTiles_066.png";

    // How many cells wide the tile's top face renders as. 1.0 = exactly one cell. If the tiles
    // leave gaps (smaller than a cell), bump this a little (e.g. 1.05–1.1) so they slightly
    // overlap and cover the seams, then re-run the menu.
    private const float TileFill = 1.03f;
    private const string GroundTileAsset = "Assets/Tiles/GroundTile.asset";
    private const string FieldTileAsset = "Assets/Tiles/FieldTile.asset";
    private const string RiverTileAsset = "Assets/Tiles/RiverTile.asset";

    [MenuItem("Tools/Zombie Farm/Apply Ground & Field Tiles")]
    public static void ApplyTiles()
    {
        Sprite grass = ConfigureTileSprite(GrassPng);
        Sprite dirt = ConfigureTileSprite(DirtPng);
        Sprite river = ConfigureTileSprite(RiverPng);
        if (grass == null || dirt == null || river == null)
        {
            Debug.LogWarning("[TileArtSetup] Couldn't load one of the tile PNGs — check the paths.");
            return;
        }

        AssignSprite(GroundTileAsset, grass);
        AssignSprite(FieldTileAsset, dirt);
        AssignSprite(RiverTileAsset, river);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[TileArtSetup] GroundTile = grass (067), FieldTile = dirt (083). The painted " +
                  "map should now show the new tiles. If they look offset/overlapping, open the " +
                  "PNG's Sprite Editor and drag the pivot to the center of the flat top face.");
    }

    /// Set a block tile's import to Single sprite, PPU = width (top face = 1 cell wide), and a
    /// custom pivot at the top-face center; returns the resulting sprite.
    private static Sprite ConfigureTileSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (tex == null || importer == null) return null;

        int w = tex.width;
        int h = tex.height;
        // Top diamond face is w x (w/2) at the top of the image; its center sits this far up.
        float pivotY = Mathf.Clamp01((h - w / 4f) / h);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        // Lower PPU => tile renders bigger. PPU = width makes the top face exactly one cell;
        // dividing by TileFill (>1) scales it up to close seams.
        importer.spritePixelsPerUnit = w / Mathf.Max(0.01f, TileFill);

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = new Vector2(0.5f, pivotY);
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void AssignSprite(string tilePath, Sprite sprite)
    {
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null)
        {
            Debug.LogWarning($"[TileArtSetup] Tile asset not found: {tilePath}");
            return;
        }
        tile.sprite = sprite;
        tile.color = Color.white; // drop the placeholder tint; the art has its own colors
        EditorUtility.SetDirty(tile);
    }
}
