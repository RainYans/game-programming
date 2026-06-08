using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// One-shot editor helper (run with the FARM scene open): builds the CityMapPanel under the
/// Canvas, fills it with City 1 (real) + two locked placeholder cities, adds a CityProgress to
/// the Systems object, routes the WarCamp through the map, and wires the new refs on
/// UIManager / SaveManager / BattleResultApplier. The panel shell is a real editable hierarchy;
/// the node visuals are generated at runtime from the serialized city list.
///
/// Run from: Tools > Monster Farm > Setup City Map. Re-running rebuilds the panel.
public static class CityMapSetup
{
    private const string PanelName = "CityMapPanel";

    [MenuItem("Tools/Monster Farm/Setup City Map")]
    public static void SetupCityMap()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("No Canvas",
                "Couldn't find a Canvas in the open scene. Open the farm scene first.", "OK");
            return;
        }

        Transform existing = canvas.transform.Find(PanelName);
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        MissionData city1 = EnsureCity1();
        CityProgress progress = EnsureCityProgress();

        GameObject root = BuildHierarchy(canvas, out CityMapPanel panel, out RectTransform mapArea,
            out Button backdropBtn, out Button closeBtn, out GameObject content);

        WireRefs(panel, content, backdropBtn, closeBtn, mapArea, progress);
        PopulateCities(panel, city1);
        RouteWarCamp(panel);
        WireProgressConsumers(progress);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;

        Debug.Log("[CityMapSetup] CityMapPanel built under Canvas (City 1 + 2 locked placeholders), " +
                  "CityProgress added to Systems, WarCamp routed through the map. Walk to the WarCamp " +
                  "+ E to open it. Save the scene (Ctrl+S).");
    }

    // --- data ---------------------------------------------------------------

    private static MissionData EnsureCity1()
    {
        string[] guids = AssetDatabase.FindAssets("t:MissionData");
        MissionData chosen = null;
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var m = AssetDatabase.LoadAssetAtPath<MissionData>(path);
            if (m == null) continue;
            if (chosen == null) chosen = m;
            if (path.ToLowerInvariant().Contains("city1")) { chosen = m; break; }
        }
        if (chosen == null) { Debug.LogWarning("[CityMapSetup] No MissionData (City1) found."); return null; }

        if (string.IsNullOrEmpty(chosen.id)) chosen.id = "city1";
        if (string.IsNullOrEmpty(chosen.mapHint))
            chosen.mapHint = "Wild grunts, runners & a brute — a gentle first push.";
        EditorUtility.SetDirty(chosen);
        AssetDatabase.SaveAssets();
        return chosen;
    }

    private static CityProgress EnsureCityProgress()
    {
        Inventory inv = Object.FindFirstObjectByType<Inventory>();
        GameObject host = inv != null ? inv.gameObject : GameObject.Find("Systems");
        if (host == null) { Debug.LogWarning("[CityMapSetup] No Systems object for CityProgress."); return null; }
        CityProgress p = host.GetComponent<CityProgress>();
        if (p == null) p = host.AddComponent<CityProgress>();
        return p;
    }

    // --- hierarchy ----------------------------------------------------------

    private static GameObject BuildHierarchy(Canvas canvas, out CityMapPanel panel,
        out RectTransform mapArea, out Button backdropBtn, out Button closeBtn, out GameObject content)
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        GameObject root = NewUI(PanelName, canvas.transform);
        Stretch(root.GetComponent<RectTransform>());
        root.transform.SetAsLastSibling();
        panel = root.AddComponent<CityMapPanel>();

        content = NewUI("Content", root.transform);
        Stretch(content.GetComponent<RectTransform>());

        GameObject backdrop = NewUI("Backdrop", content.transform);
        Stretch(backdrop.GetComponent<RectTransform>());
        backdrop.AddComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f, 0.78f);
        backdropBtn = backdrop.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;

        GameObject frame = NewUI("Frame", content.transform);
        var frameRT = frame.GetComponent<RectTransform>();
        frameRT.anchorMin = frameRT.anchorMax = frameRT.pivot = new Vector2(0.5f, 0.5f);
        frameRT.sizeDelta = new Vector2(820, 540);
        frame.AddComponent<Image>().color = new Color(0.09f, 0.12f, 0.18f, 0.99f);

        // Title
        var title = NewUI("Title", frame.transform);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -22f);
        titleRT.sizeDelta = new Vector2(760, 40);
        var titleTmp = title.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Choose a city to reclaim";
        titleTmp.fontSize = 26;
        titleTmp.color = new Color(1f, 0.95f, 0.75f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.fontStyle = FontStyles.Bold;
        if (font != null) titleTmp.font = font;

        // Map area (the node parent)
        GameObject area = NewUI("MapArea", frame.transform);
        var areaRT = area.GetComponent<RectTransform>();
        areaRT.anchorMin = areaRT.anchorMax = areaRT.pivot = new Vector2(0.5f, 0.5f);
        areaRT.anchoredPosition = new Vector2(0f, 10f);
        areaRT.sizeDelta = new Vector2(760, 400);
        mapArea = areaRT;

        // Close button (bottom)
        GameObject closeGo = NewUI("CloseBtn", frame.transform);
        var closeRT = closeGo.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.5f, 0f);
        closeRT.anchorMax = new Vector2(0.5f, 0f);
        closeRT.pivot = new Vector2(0.5f, 0f);
        closeRT.anchoredPosition = new Vector2(0f, 16f);
        closeRT.sizeDelta = new Vector2(180, 44);
        closeGo.AddComponent<Image>().color = new Color(0.30f, 0.18f, 0.18f);
        closeBtn = closeGo.AddComponent<Button>();
        var closeLabel = NewUI("Label", closeGo.transform);
        Stretch(closeLabel.GetComponent<RectTransform>());
        var closeTmp = closeLabel.AddComponent<TextMeshProUGUI>();
        closeTmp.text = "Close";
        closeTmp.fontSize = 18;
        closeTmp.color = Color.white;
        closeTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) closeTmp.font = font;

        return root;
    }

    // --- wiring -------------------------------------------------------------

    private static void WireRefs(CityMapPanel panel, GameObject content, Button backdropBtn,
        Button closeBtn, RectTransform mapArea, CityProgress progress)
    {
        var so = new SerializedObject(panel);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdropButton").objectReferenceValue = backdropBtn;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("nodeParent").objectReferenceValue = mapArea;
        so.FindProperty("deployPanel").objectReferenceValue = Object.FindFirstObjectByType<DeployPanel>();
        so.FindProperty("cityProgress").objectReferenceValue = progress;
        so.FindProperty("avatarMovement").objectReferenceValue = Object.FindFirstObjectByType<AvatarController>();
        so.FindProperty("avatarInteraction").objectReferenceValue = Object.FindFirstObjectByType<AvatarInteraction>();
        so.ApplyModifiedProperties();
    }

    private static void PopulateCities(CityMapPanel panel, MissionData city1)
    {
        var so = new SerializedObject(panel);
        SerializedProperty list = so.FindProperty("cities");
        list.arraySize = 3;

        SetCity(list.GetArrayElementAtIndex(0), "city1", city1, "Fallen City 1", "",
            new Vector2(-240f, 40f), availableAtStart: true, unlockAfter: new string[0]);

        SetCity(list.GetArrayElementAtIndex(1), "city2", null, "Fallen City 2",
            "Locked until City 1 falls.", new Vector2(170f, 120f),
            availableAtStart: false, unlockAfter: new[] { "city1" });

        SetCity(list.GetArrayElementAtIndex(2), "city3", null, "Fallen City 3",
            "Deeper in the dead zone.", new Vector2(210f, -120f),
            availableAtStart: false, unlockAfter: new[] { "city1" });

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(panel);
    }

    private static void SetCity(SerializedProperty e, string id, MissionData mission, string title,
        string hint, Vector2 pos, bool availableAtStart, string[] unlockAfter)
    {
        e.FindPropertyRelative("cityId").stringValue = id;
        e.FindPropertyRelative("mission").objectReferenceValue = mission;
        e.FindPropertyRelative("title").stringValue = title;
        e.FindPropertyRelative("hint").stringValue = hint;
        e.FindPropertyRelative("mapPos").vector2Value = pos;
        e.FindPropertyRelative("availableAtStart").boolValue = availableAtStart;

        SerializedProperty ua = e.FindPropertyRelative("unlockAfter");
        ua.arraySize = unlockAfter.Length;
        for (int i = 0; i < unlockAfter.Length; i++)
            ua.GetArrayElementAtIndex(i).stringValue = unlockAfter[i];
    }

    private static void RouteWarCamp(CityMapPanel panel)
    {
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        if (ui == null) return;
        var so = new SerializedObject(ui);
        SerializedProperty p = so.FindProperty("cityMapPanel");
        if (p != null) { p.objectReferenceValue = panel; so.ApplyModifiedProperties(); EditorUtility.SetDirty(ui); }
    }

    private static void WireProgressConsumers(CityProgress progress)
    {
        WireField(Object.FindFirstObjectByType<SaveManager>(), progress);
        WireField(Object.FindFirstObjectByType<BattleResultApplier>(), progress);
    }

    private static void WireField(Object target, CityProgress value)
    {
        if (target == null) return;
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty("cityProgress");
        if (p != null && p.objectReferenceValue == null)
        {
            p.objectReferenceValue = value;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    // --- helpers ------------------------------------------------------------

    private static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
