using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// One-shot editor helper: creates Assets/Scenes/Battle.unity for the real-time combat core
/// (Slice 1). Builds an ortho camera that follows a WASD leader (reusing AvatarController), a
/// BattleManager wired with a hardcoded test battle (the three starting strains vs a few wild
/// zombies), spawn points, and a result label. Run from: Tools > Zombie Farm > Setup Battle
/// Scene. Editor-only. Re-running rebuilds the scene from scratch.
public static class BattleSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Battle.unity";
    private const string StrainFolder = "Assets/ScriptableObject/Strains";

    [MenuItem("Tools/Zombie Farm/Setup Battle Scene")]
    public static void SetupBattleScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Sprite placeholder = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // --- Camera (follows the leader) ---
        var camGo = new GameObject("Main Camera", typeof(Camera));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.16f); // dirt-ish arena floor
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        var follow = camGo.AddComponent<BattleCameraFollow>();

        // --- Leader (the avatar; WASD via the shared AvatarController) ---
        var leader = new GameObject("Leader");
        var leaderSr = leader.AddComponent<SpriteRenderer>();
        leaderSr.sprite = placeholder;
        leaderSr.color = new Color(0.95f, 0.9f, 0.45f);
        leaderSr.sortingOrder = 6;
        leader.transform.localScale = Vector3.one * 0.8f;
        var rb = leader.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        leader.AddComponent<CircleCollider2D>().radius = 0.5f;
        leader.AddComponent<AvatarController>();
        follow.SetTarget(leader.transform);

        // --- Spawn points ---
        var squadSpawn = new GameObject("SquadSpawn");
        squadSpawn.transform.position = new Vector3(-2f, -3f, 0f);
        var enemySpawn = new GameObject("EnemySpawn");
        enemySpawn.transform.position = new Vector3(3f, 4f, 0f);

        // --- Result label canvas ---
        var canvasGo = new GameObject("BattleCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var labelGo = new GameObject("ResultLabel", typeof(RectTransform));
        labelGo.transform.SetParent(canvasGo.transform, false);
        var lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = new Vector2(0f, -40f);
        lrt.sizeDelta = new Vector2(900f, 90f);
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 42;
        label.text = string.Empty;
        if (TMP_Settings.defaultFontAsset != null) label.font = TMP_Settings.defaultFontAsset;

        // --- BattleManager (wired with a hardcoded Slice 1 test battle) ---
        var mgrGo = new GameObject("BattleManager");
        var mgr = mgrGo.AddComponent<BattleManager>();
        var so = new SerializedObject(mgr);
        so.FindProperty("leader").objectReferenceValue = leader.transform;
        so.FindProperty("squadSpawn").objectReferenceValue = squadSpawn.transform;
        so.FindProperty("enemySpawn").objectReferenceValue = enemySpawn.transform;
        so.FindProperty("resultLabel").objectReferenceValue = label;
        SetSquad(so, LoadStrain("Brute"), LoadStrain("Mauler"), LoadStrain("Runner"));
        SetEnemies(so, ResolveEnemy(), 5);
        so.ApplyModifiedProperties();

        // --- Save scene + register in Build Settings (for later scene transitions) ---
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EnsureInBuildSettings(ScenePath);

        Debug.Log("[BattleSceneSetup] Built Battle.unity. Press Play in this scene: WASD moves " +
                  "the leader, the squad follows and auto-attacks, enemies advance. Clear them = " +
                  "Victory; lose the squad = Defeat. (Hardcoded test battle for now.)");
    }

    private static ZombieData LoadStrain(string displayName)
    {
        var z = AssetDatabase.LoadAssetAtPath<ZombieData>($"{StrainFolder}/Zombie_{displayName}.asset");
        if (z == null) Debug.LogWarning($"[BattleSceneSetup] Missing {displayName} — run Setup Zombie Strains first.");
        return z;
    }

    /// Prefer a dedicated wild-zombie ZombieData (note the asset is named "WildZombile"); fall
    /// back to the Runner strain so the test battle still has an enemy.
    private static ZombieData ResolveEnemy()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:ZombieData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains("wild")) return AssetDatabase.LoadAssetAtPath<ZombieData>(path);
        }
        return LoadStrain("Runner");
    }

    private static void SetSquad(SerializedObject so, params ZombieData[] strains)
    {
        SerializedProperty list = so.FindProperty("testSquad");
        var valid = new List<ZombieData>();
        foreach (ZombieData z in strains) if (z != null) valid.Add(z);
        list.arraySize = valid.Count;
        for (int i = 0; i < valid.Count; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = valid[i];
    }

    private static void SetEnemies(SerializedObject so, ZombieData enemy, int count)
    {
        SerializedProperty list = so.FindProperty("testEnemies");
        if (enemy == null) { list.arraySize = 0; return; }
        list.arraySize = 1;
        SerializedProperty el = list.GetArrayElementAtIndex(0);
        el.FindPropertyRelative("zombie").objectReferenceValue = enemy;
        el.FindPropertyRelative("count").intValue = count;
    }

    private static void EnsureInBuildSettings(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene s in scenes) if (s.path == path) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
