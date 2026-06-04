using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// One-shot editor helper (run with the FARM scene open): builds the DeployPanel under the
/// Canvas, adds a BattleResultApplier to the Systems object, routes the WarCamp through the new
/// panel, and registers the Farm + Battle scenes in Build Settings. The panel is a real, editable
/// hierarchy; only the unit rows are cloned at runtime from a template.
///
/// Run from: Tools > Zombie Farm > Setup Deploy Panel. Re-running rebuilds the panel.
public static class DeployPanelSetup
{
    private const string PanelName = "DeployPanel";

    [MenuItem("Tools/Zombie Farm/Setup Deploy Panel")]
    public static void SetupDeployPanel()
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

        GameObject root = BuildHierarchy(canvas, out DeployPanel panel);
        WireDataRefs(panel);
        EnsureResultApplier();
        WireUIManager(panel);
        EnsureScenesInBuild();

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;

        Debug.Log("[DeployPanelSetup] DeployPanel built under Canvas, BattleResultApplier added, " +
                  "WarCamp routed to it, Farm + Battle registered in Build Settings. Walk to the " +
                  "WarCamp + E to deploy. Save the scene (Ctrl+S).");
    }

    private static GameObject BuildHierarchy(Canvas canvas, out DeployPanel panel)
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        GameObject root = NewUI(PanelName, canvas.transform);
        Stretch(root.GetComponent<RectTransform>());
        root.transform.SetAsLastSibling();
        panel = root.AddComponent<DeployPanel>();

        GameObject content = NewUI("Content", root.transform);
        Stretch(content.GetComponent<RectTransform>());

        GameObject backdrop = NewUI("Backdrop", content.transform);
        Stretch(backdrop.GetComponent<RectTransform>());
        backdrop.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        var backdropBtn = backdrop.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;

        GameObject dialog = NewUI("Panel", content.transform);
        var dialogRT = dialog.GetComponent<RectTransform>();
        dialogRT.anchorMin = dialogRT.anchorMax = dialogRT.pivot = new Vector2(0.5f, 0.5f);
        dialogRT.sizeDelta = new Vector2(460, 440);
        dialog.AddComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.98f);
        var layout = dialog.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 18, 18);
        layout.spacing = 10;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        MakeText(dialog.transform, "Deploy a squad", 24, new Color(1f, 0.95f, 0.75f), font, 40, true);
        var counter = MakeText(dialog.transform, "Squad: 0 / 4", 18, new Color(0.8f, 0.9f, 1f), font, 28, true);

        GameObject list = NewUI("List", dialog.transform);
        var listLayout = list.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 6;
        listLayout.childControlWidth = listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        SetFlexibleHeight(list);

        // Unit row template (cloned per owned zombie at runtime).
        GameObject template = NewUI("UnitRowTemplate", list.transform);
        template.AddComponent<Image>().color = new Color(0.16f, 0.18f, 0.24f, 1f);
        var rowBtn = template.AddComponent<Button>();
        SetFixedHeight(template, 46);
        var rowLabel = MakeText(template.transform, "Strain — Full", 18, Color.white, font, 0, false);
        Stretch(rowLabel.rectTransform);

        var deployBtn = MakeButton(dialog.transform, "Deploy", font, new Color(0.22f, 0.40f, 0.20f), 46);
        var cancelBtn = MakeButton(dialog.transform, "Cancel", font, new Color(0.30f, 0.18f, 0.18f), 42);

        var so = new SerializedObject(panel);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdropButton").objectReferenceValue = backdropBtn;
        so.FindProperty("rowParent").objectReferenceValue = list.transform;
        so.FindProperty("rowTemplate").objectReferenceValue = rowBtn;
        so.FindProperty("counterLabel").objectReferenceValue = counter;
        so.FindProperty("deployButton").objectReferenceValue = deployBtn;
        so.FindProperty("cancelButton").objectReferenceValue = cancelBtn;
        so.ApplyModifiedProperties();

        return root;
    }

    private static void WireDataRefs(DeployPanel panel)
    {
        var so = new SerializedObject(panel);
        AssignAsset(so, "config", "t:GameConfig");
        AssignMission(so, "mission");
        AssignScene(so, "inventory", Object.FindFirstObjectByType<Inventory>());
        AssignScene(so, "avatarMovement", Object.FindFirstObjectByType<AvatarController>());
        AssignScene(so, "avatarInteraction", Object.FindFirstObjectByType<AvatarInteraction>());
        so.ApplyModifiedProperties();
    }

    private static void EnsureResultApplier()
    {
        Inventory inv = Object.FindFirstObjectByType<Inventory>();
        GameObject host = inv != null ? inv.gameObject : GameObject.Find("Systems");
        if (host == null) { Debug.LogWarning("[DeployPanelSetup] No Systems/Inventory object for BattleResultApplier."); return; }
        if (host.GetComponent<BattleResultApplier>() == null) host.AddComponent<BattleResultApplier>();
    }

    private static void WireUIManager(DeployPanel panel)
    {
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        if (ui == null) return;
        var so = new SerializedObject(ui);
        SerializedProperty p = so.FindProperty("deployPanel");
        if (p != null) { p.objectReferenceValue = panel; so.ApplyModifiedProperties(); }
    }

    private static void EnsureScenesInBuild()
    {
        var wanted = new[] { "Assets/Scenes/Farm.unity", "Assets/Scenes/Battle.unity" };
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string path in wanted)
        {
            if (!System.IO.File.Exists(path)) continue;
            bool present = scenes.Exists(s => s.path == path);
            if (!present) scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // --- helpers ------------------------------------------------------------

    private static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text MakeText(Transform parent, string text, float size, Color color,
        TMP_FontAsset font, float fixedHeight, bool fixedHeightOn)
    {
        var go = NewUI("Label", parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (font != null) tmp.font = font;
        if (fixedHeightOn) SetFixedHeight(go, fixedHeight);
        return tmp;
    }

    private static Button MakeButton(Transform parent, string text, TMP_FontAsset font, Color color, float height)
    {
        var go = NewUI(text + "Btn", parent);
        go.AddComponent<Image>().color = color;
        var btn = go.AddComponent<Button>();
        SetFixedHeight(go, height);
        var label = MakeText(go.transform, text, 18, Color.white, font, 0, false);
        Stretch(label.rectTransform);
        return btn;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetFixedHeight(GameObject go, float h)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = h;
        le.flexibleHeight = 0;
    }

    private static void SetFlexibleHeight(GameObject go)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.flexibleHeight = 1;
    }

    private static void AssignAsset(SerializedObject so, string prop, string filter)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p == null || p.objectReferenceValue != null) return;
        string[] guids = AssetDatabase.FindAssets(filter);
        if (guids.Length > 0)
            p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static void AssignMission(SerializedObject so, string prop)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p == null || p.objectReferenceValue != null) return;
        string[] guids = AssetDatabase.FindAssets("t:MissionData");
        string chosen = null;
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (chosen == null) chosen = path;
            if (System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains("city1")) { chosen = path; break; }
        }
        if (chosen != null) p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<MissionData>(chosen);
    }

    private static void AssignScene(SerializedObject so, string prop, Object value)
    {
        SerializedProperty p = so.FindProperty(prop);
        if (p != null && p.objectReferenceValue == null) p.objectReferenceValue = value;
    }
}
