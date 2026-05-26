using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// One-shot editor helper: creates the FarmRoamerSpawner in the open scene, gives it a
/// PolygonCollider2D wander area (pre-shaped as an isometric diamond) and assigns a GameConfig
/// asset for tinting if one exists. Run from: Tools > Zombie Farm > Setup Farm Roamers.
/// Editor-only; idempotent.
public static class FarmRoamerSetup
{
    private const string SpawnerName = "FarmRoamerSpawner";

    [MenuItem("Tools/Zombie Farm/Setup Farm Roamers")]
    public static void SetupFarmRoamers()
    {
        GameObject go = GameObject.Find(SpawnerName);
        if (go == null)
        {
            go = new GameObject(SpawnerName);
            Undo.RegisterCreatedObjectUndo(go, "Create FarmRoamerSpawner");
        }

        FarmRoamerSpawner spawner = go.GetComponent<FarmRoamerSpawner>();
        if (spawner == null) spawner = Undo.AddComponent<FarmRoamerSpawner>(go);

        // Wander area: a PolygonCollider2D you can freely reshape with "Edit Collider".
        PolygonCollider2D poly = go.GetComponent<PolygonCollider2D>();
        if (poly == null)
        {
            poly = Undo.AddComponent<PolygonCollider2D>(go);
            poly.isTrigger = true; // shape only, no physics
            // Start as an isometric diamond (vertices up / right / down / left) to drag from.
            poly.points = new[]
            {
                new Vector2(0f, 2.5f),
                new Vector2(5f, 0f),
                new Vector2(0f, -2.5f),
                new Vector2(-5f, 0f),
            };
        }

        var so = new SerializedObject(spawner);
        AssignIfEmpty(so, "wanderArea", poly);

        SerializedProperty configProp = so.FindProperty("config");
        if (configProp != null && configProp.objectReferenceValue == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:GameConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                configProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
            }
        }
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;

        Debug.Log("[FarmRoamerSetup] FarmRoamerSpawner ready. Select it, then on the " +
                  "PolygonCollider2D click 'Edit Collider' and drag the points to match your " +
                  "farm. Play and harvest to see roamers. Save the scene (Ctrl+S).");
    }

    private static void AssignIfEmpty(SerializedObject so, string propName, Object value)
    {
        SerializedProperty p = so.FindProperty(propName);
        if (p != null && p.objectReferenceValue == null) p.objectReferenceValue = value;
    }
}
