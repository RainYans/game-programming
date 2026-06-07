using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// Editor window to drop real building art onto the farm's Home / Shop / Lab / WarCamp objects.
/// Drag a Sprite into each slot and hit Apply — it swaps the SpriteRenderer (keeping the Building
/// component, the collider, and the grid position), clears the placeholder tint, and (optionally)
/// re-pivots the sprite to bottom-center so the building sits ON the tile instead of sinking in.
///
/// Candidate sprites live in `Assets/Art/Buildings/` (Kenney City Kit Suburban houses +
/// Isometric Buildings — all CC0). Open from: Tools > Zombie Farm > Building Art...
/// Selections + scale persist via EditorPrefs, so you can tweak and re-Apply freely.
public class BuildingArtWindow : EditorWindow
{
    private Sprite home, shop, lab, warCamp;
    private float scale = 1f;
    private bool pivotBottom = true;

    private const string Pref = "ZF_BuildingArt_";

    [MenuItem("Tools/Zombie Farm/Building Art...")]
    public static void Open() => GetWindow<BuildingArtWindow>("Building Art");

    private void OnEnable()
    {
        home = LoadSlot("Home"); shop = LoadSlot("Shop");
        lab = LoadSlot("Lab"); warCamp = LoadSlot("WarCamp");
        scale = EditorPrefs.GetFloat(Pref + "scale", 1f);
        pivotBottom = EditorPrefs.GetBool(Pref + "pivot", true);
    }

    private static Sprite LoadSlot(string slot)
    {
        string path = EditorPrefs.GetString(Pref + slot, "");
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SaveSlot(string slot, Sprite s) =>
        EditorPrefs.SetString(Pref + slot, s != null ? AssetDatabase.GetAssetPath(s) : "");

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Drag a building sprite into each slot (candidates: Assets/Art/Buildings/), then Apply. " +
            "Only the visual changes — colliders + walk-up-E interaction stay intact.", MessageType.Info);

        EditorGUI.BeginChangeCheck();
        home = (Sprite)EditorGUILayout.ObjectField("Home", home, typeof(Sprite), false);
        shop = (Sprite)EditorGUILayout.ObjectField("Shop", shop, typeof(Sprite), false);
        lab = (Sprite)EditorGUILayout.ObjectField("Lab", lab, typeof(Sprite), false);
        warCamp = (Sprite)EditorGUILayout.ObjectField("WarCamp", warCamp, typeof(Sprite), false);
        scale = EditorGUILayout.Slider("Scale", scale, 0.25f, 4f);
        pivotBottom = EditorGUILayout.Toggle("Pivot at bottom (seat on tile)", pivotBottom);
        if (EditorGUI.EndChangeCheck())
        {
            SaveSlot("Home", home); SaveSlot("Shop", shop);
            SaveSlot("Lab", lab); SaveSlot("WarCamp", warCamp);
            EditorPrefs.SetFloat(Pref + "scale", scale);
            EditorPrefs.SetBool(Pref + "pivot", pivotBottom);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply to scene", GUILayout.Height(32))) Apply();
    }

    private void Apply()
    {
        int n = ApplyOne("Home", home) + ApplyOne("Shop", shop)
              + ApplyOne("Lab", lab) + ApplyOne("WarCamp", warCamp);
        if (n > 0) EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[BuildingArt] Applied art to {n} building(s). Save the scene (Ctrl+S).");
    }

    private int ApplyOne(string goName, Sprite sprite)
    {
        if (sprite == null) return 0;
        if (pivotBottom) sprite = RepivotBottom(sprite);

        GameObject go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[BuildingArt] No '{goName}' object in the open scene."); return 0; }

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = Undo.AddComponent<SpriteRenderer>(go);
        Undo.RecordObject(sr, "Building Art");
        Undo.RecordObject(go.transform, "Building Art");
        sr.sprite = sprite;
        sr.color = Color.white;     // drop the placeholder tint
        sr.sortingOrder = 10;
        go.transform.localScale = Vector3.one * scale;
        EditorUtility.SetDirty(sr);
        EditorUtility.SetDirty(go);
        return 1;
    }

    /// Set the sprite's import alignment to bottom-center, reimport, and return the rebuilt Sprite.
    private static Sprite RepivotBottom(Sprite sprite)
    {
        string path = AssetDatabase.GetAssetPath(sprite);
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return sprite;

        var s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        if (s.spriteAlignment != (int)SpriteAlignment.BottomCenter)
        {
            s.spriteMode = (int)SpriteImportMode.Single;
            s.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            ti.SetTextureSettings(s);
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? sprite;
    }
}
