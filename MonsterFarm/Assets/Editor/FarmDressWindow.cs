using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

/// "Dress Farm Ground": scatters decoration props (trees/fences) on the open farm ground,
/// avoiding the central plantable field and the buildings. Deterministic (same seed = same
/// layout) and fully reversible (Clear Props). Optionally mixes ground-tile variants in.
///
/// Edit the FarmDressConfig asset's lists in this window (drag from Assets/Art/Props/), then
/// Scatter. Open from: Tools > Monster Farm > Dress Farm Ground.
public class FarmDressWindow : EditorWindow
{
    private const int GroundSize = 20; // mirror FarmMapSetup
    private const int FieldSize = 6;
    private const string DecoParent = "Decorations";
    private static readonly Vector3Int[] BuildingCells =
    {
        new Vector3Int(4, 4, 0), new Vector3Int(7, 4, 0),
        new Vector3Int(10, 4, 0), new Vector3Int(13, 4, 0),
    };

    private FarmDressConfig config;
    private Editor configEditor;

    [MenuItem("Tools/Monster Farm/Dress Farm Ground")]
    public static void Open() => GetWindow<FarmDressWindow>("Dress Farm Ground");

    private void OnEnable()
    {
        string[] g = AssetDatabase.FindAssets("t:FarmDressConfig");
        if (g.Length > 0) config = AssetDatabase.LoadAssetAtPath<FarmDressConfig>(AssetDatabase.GUIDToAssetPath(g[0]));
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Scatters props on open ground (skips the plantable field + buildings). Deterministic — " +
            "same seed = same layout. Reversible via Clear Props.", MessageType.Info);

        config = (FarmDressConfig)EditorGUILayout.ObjectField("Config", config, typeof(FarmDressConfig), false);
        if (config == null)
        {
            if (GUILayout.Button("Create config asset"))
            {
                var c = CreateInstance<FarmDressConfig>();
                if (!AssetDatabase.IsValidFolder("Assets/Tiles")) AssetDatabase.CreateFolder("Assets", "Tiles");
                AssetDatabase.CreateAsset(c, "Assets/Tiles/FarmDressConfig.asset");
                AssetDatabase.SaveAssets();
                config = c;
            }
            return;
        }

        if (configEditor == null || configEditor.target != config) configEditor = Editor.CreateEditor(config);
        configEditor.OnInspectorGUI();

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Scatter Props", GUILayout.Height(30))) ScatterProps();
            if (GUILayout.Button("Clear Props", GUILayout.Height(30))) ClearProps();
        }
        if (config.varyGround && GUILayout.Button("Apply Ground Variety (Ctrl+Z to revert)")) VaryGround();
    }

    // --- props ---------------------------------------------------------------

    private void ScatterProps()
    {
        Tilemap ground = FindGround(out Grid grid);
        if (ground == null) return;

        var ready = new List<Sprite>();
        if (config.props != null)
            foreach (Sprite s in config.props) if (s != null) ready.Add(RepivotBottom(s));
        if (ready.Count == 0) { Debug.LogWarning("[Dress] No props assigned in the config."); return; }

        Transform parent = EnsureDecoParent();
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);

        int placed = 0;
        for (int x = 0; x < GroundSize; x++)
            for (int y = 0; y < GroundSize; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (IsField(x, y) || IsBuilding(cell) || ground.GetTile(cell) == null) continue;

                int h = Hash(x, y, config.seed);
                if ((h % 1000) / 1000f >= config.propDensity) continue;

                Sprite sprite = ready[(h >> 4) % ready.Count];
                var go = new GameObject("Prop_" + sprite.name);
                Undo.RegisterCreatedObjectUndo(go, "Scatter Props");
                go.transform.SetParent(parent, false);
                Vector3 c = grid.GetCellCenterWorld(cell);
                float ox = (((h >> 8) % 100) / 100f - 0.5f) * 0.3f;
                float oy = (((h >> 12) % 100) / 100f - 0.5f) * 0.3f;
                go.transform.position = c + new Vector3(ox, oy, 0f);
                go.transform.localScale = Vector3.one * config.propScale;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 1; // above ground (~0), below agents (5) / buildings (10)
                placed++;
            }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[Dress] Scattered {placed} prop(s) under '{DecoParent}'. Change the seed to re-roll, " +
                  "Clear Props to remove. Save the scene (Ctrl+S).");
    }

    private void ClearProps()
    {
        var p = GameObject.Find(DecoParent);
        if (p == null) { Debug.Log("[Dress] Nothing to clear."); return; }
        Undo.DestroyObjectImmediate(p);
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[Dress] Cleared decorations. Save the scene.");
    }

    // --- optional ground variety --------------------------------------------

    private void VaryGround()
    {
        Tilemap ground = FindGround(out _);
        if (ground == null) return;
        var groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/GroundTile.asset");
        if (groundTile == null || config.groundVariants == null || config.groundVariants.Count == 0)
        { Debug.LogWarning("[Dress] Need Assets/Tiles/GroundTile.asset + ground variants."); return; }

        var variants = new List<TileBase>();
        foreach (Sprite s in config.groundVariants) if (s != null) variants.Add(SpriteToTile(s));
        if (variants.Count == 0) return;

        Undo.RegisterCompleteObjectUndo(ground, "Vary Ground");
        int changed = 0;
        for (int x = 0; x < GroundSize; x++)
            for (int y = 0; y < GroundSize; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (IsField(x, y) || IsBuilding(cell)) continue;
                if (ground.GetTile(cell) != groundTile) continue; // only touch plain ground
                int h = Hash(x, y, config.seed + 7);
                if ((h % 1000) / 1000f >= config.groundVarietyChance) continue;
                ground.SetTile(cell, variants[(h >> 4) % variants.Count]);
                changed++;
            }
        EditorUtility.SetDirty(ground);
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[Dress] Varied {changed} ground cell(s). Ctrl+Z reverts. Save the scene.");
    }

    // --- helpers -------------------------------------------------------------

    private static Tilemap FindGround(out Grid grid)
    {
        grid = null;
        var go = GameObject.Find("GroundTilemap");
        if (go == null) { Debug.LogWarning("[Dress] No 'GroundTilemap' in the open scene."); return null; }
        var tm = go.GetComponent<Tilemap>();
        grid = go.GetComponentInParent<Grid>();
        if (tm == null || grid == null) { Debug.LogWarning("[Dress] GroundTilemap missing Tilemap/Grid."); return null; }
        return tm;
    }

    private static Transform EnsureDecoParent()
    {
        var p = GameObject.Find(DecoParent);
        if (p == null) { p = new GameObject(DecoParent); Undo.RegisterCreatedObjectUndo(p, "Decorations"); }
        return p.transform;
    }

    private static bool IsField(int x, int y)
    {
        int s = (GroundSize - FieldSize) / 2, e = s + FieldSize - 1;
        return x >= s && x <= e && y >= s && y <= e;
    }

    private static bool IsBuilding(Vector3Int cell)
    {
        foreach (Vector3Int b in BuildingCells) if (b == cell) return true;
        return false;
    }

    private static int Hash(int x, int y, int seed)
    {
        unchecked { return ((x * 73856093) ^ (y * 19349663) ^ (seed * 83492791)) & 0x7fffffff; }
    }

    private static Tile SpriteToTile(Sprite s)
    {
        const string dir = "Assets/Tiles/Landscape";
        if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/Tiles", "Landscape");
        string path = $"{dir}/{s.name}.asset";
        var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (t == null)
        {
            t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = s;
            AssetDatabase.CreateAsset(t, path);
        }
        return t;
    }

    /// Re-pivot a sprite to bottom-center (so props seat on the cell) and return the rebuilt Sprite.
    private static Sprite RepivotBottom(Sprite sprite)
    {
        string path = AssetDatabase.GetAssetPath(sprite);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return sprite;
        var st = new TextureImporterSettings();
        ti.ReadTextureSettings(st);
        if (st.spriteAlignment != (int)SpriteAlignment.BottomCenter)
        {
            st.spriteMode = (int)SpriteImportMode.Single;
            st.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            ti.SetTextureSettings(st);
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? sprite;
    }
}
