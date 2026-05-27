using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// One-shot editor helper: builds the SeedPickPopup dialog as a real, editable hierarchy under
/// the scene's Canvas and wires the SeedPickPopup component's references. Because the panel
/// lives in the scene (not generated at runtime), you can restyle it freely in the editor —
/// only the seed rows are filled at runtime from the row template.
///
/// Run from: Tools > Zombie Farm > Setup Seed Pick Popup.
/// Idempotent-ish: if the popup already exists it only re-wires the data refs and leaves your
/// layout edits alone. Delete the SeedPickPopup object first if you want a clean rebuild.
public static class SeedPickPopupSetup
{
    private const string PopupName = "SeedPickPopup";

    [MenuItem("Tools/Zombie Farm/Setup Seed Pick Popup")]
    public static void SetupSeedPickPopup()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("No Canvas",
                "Couldn't find a Canvas in the open scene. Open the farm scene first.", "OK");
            return;
        }

        Transform existing = canvas.transform.Find(PopupName);
        if (existing != null)
        {
            var existingPopup = existing.GetComponent<SeedPickPopup>();
            if (existingPopup != null) WireDataRefs(existingPopup);
            EditorSceneManager.MarkSceneDirty(existing.gameObject.scene);
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("[SeedPickPopupSetup] Popup already exists — re-wired data refs and kept " +
                      "your layout. Delete the SeedPickPopup object first for a clean rebuild.");
            return;
        }

        GameObject root = BuildHierarchy(canvas);
        var popup = root.GetComponent<SeedPickPopup>();
        WireDataRefs(popup);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;

        Debug.Log("[SeedPickPopupSetup] Built SeedPickPopup under Canvas. Restyle it in the " +
                  "scene as you like (the rows clone 'SeedRowTemplate' at runtime). Make sure an " +
                  "EventSystem exists. Save the scene (Ctrl+S).");
    }

    private static GameObject BuildHierarchy(Canvas canvas)
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        GameObject root = NewUI(PopupName, canvas.transform);
        Stretch(root.GetComponent<RectTransform>());
        root.transform.SetAsLastSibling(); // draw above the rest of the HUD (modal)
        var popup = root.AddComponent<SeedPickPopup>();

        // Content (toggled at runtime)
        GameObject content = NewUI("Content", root.transform);
        Stretch(content.GetComponent<RectTransform>());

        // Backdrop: dim + click-to-cancel
        GameObject backdrop = NewUI("Backdrop", content.transform);
        Stretch(backdrop.GetComponent<RectTransform>());
        var backdropImg = backdrop.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, 0.55f);
        var backdropBtn = backdrop.AddComponent<Button>();
        backdropBtn.transition = Selectable.Transition.None;

        // Panel: centered dialog
        GameObject panel = NewUI("Panel", content.transform);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(420, 360);
        panel.AddComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.98f);
        var panelLayout = panel.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(20, 20, 18, 18);
        panelLayout.spacing = 10;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        // Title
        GameObject title = NewUI("Title", panel.transform);
        var titleTmp = title.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Choose a seed to plant";
        titleTmp.fontSize = 24;
        titleTmp.color = new Color(1f, 0.95f, 0.75f);
        titleTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) titleTmp.font = font;
        SetHeight(title, 44, fixedHeight: true);

        // List (rows go here; absorbs the middle space)
        GameObject list = NewUI("List", panel.transform);
        var listLayout = list.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 8;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        SetHeight(list, 0, fixedHeight: false); // flexibleHeight = 1

        // Row template (cloned per seed at runtime)
        GameObject template = NewUI("SeedRowTemplate", list.transform);
        template.AddComponent<Image>().color = new Color(0.18f, 0.24f, 0.20f, 1f);
        var rowBtn = template.AddComponent<Button>();
        SetHeight(template, 48, fixedHeight: true);
        GameObject rowLabel = NewUI("Label", template.transform);
        Stretch(rowLabel.GetComponent<RectTransform>());
        var rowTmp = rowLabel.AddComponent<TextMeshProUGUI>();
        rowTmp.text = "Strain    x0";
        rowTmp.fontSize = 20;
        rowTmp.color = Color.white;
        rowTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) rowTmp.font = font;

        // Cancel
        GameObject cancel = NewUI("CancelBtn", panel.transform);
        cancel.AddComponent<Image>().color = new Color(0.30f, 0.18f, 0.18f, 1f);
        var cancelBtn = cancel.AddComponent<Button>();
        SetHeight(cancel, 44, fixedHeight: true);
        GameObject cancelLabel = NewUI("Label", cancel.transform);
        Stretch(cancelLabel.GetComponent<RectTransform>());
        var cancelTmp = cancelLabel.AddComponent<TextMeshProUGUI>();
        cancelTmp.text = "Cancel";
        cancelTmp.fontSize = 18;
        cancelTmp.color = Color.white;
        cancelTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) cancelTmp.font = font;

        // Wire UI refs on the component.
        var so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdropButton").objectReferenceValue = backdropBtn;
        so.FindProperty("rowParent").objectReferenceValue = list.transform;
        so.FindProperty("rowTemplate").objectReferenceValue = rowBtn;
        so.FindProperty("cancelButton").objectReferenceValue = cancelBtn;
        so.ApplyModifiedProperties();

        return root;
    }

    private static void WireDataRefs(SeedPickPopup popup)
    {
        var so = new SerializedObject(popup);
        AssignConfig(so, "config");
        AssignIfEmpty(so, "farmActions", Object.FindFirstObjectByType<FarmActions>());
        AssignIfEmpty(so, "seedInventory", Object.FindFirstObjectByType<SeedInventory>());
        AssignIfEmpty(so, "avatarMovement", Object.FindFirstObjectByType<AvatarController>());
        AssignIfEmpty(so, "avatarInteraction", Object.FindFirstObjectByType<AvatarInteraction>());
        so.ApplyModifiedProperties();
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

    /// fixedHeight=true pins min+preferred to h; false makes the element flexible (fills space).
    private static void SetHeight(GameObject go, float h, bool fixedHeight)
    {
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        if (fixedHeight)
        {
            le.minHeight = h;
            le.preferredHeight = h;
            le.flexibleHeight = 0;
        }
        else
        {
            le.flexibleHeight = 1;
        }
    }

    private static void AssignConfig(SerializedObject so, string propName)
    {
        SerializedProperty p = so.FindProperty(propName);
        if (p == null || p.objectReferenceValue != null) return;
        string[] guids = AssetDatabase.FindAssets("t:GameConfig");
        if (guids.Length > 0)
            p.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameConfig>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static void AssignIfEmpty(SerializedObject so, string propName, Object value)
    {
        SerializedProperty p = so.FindProperty(propName);
        if (p != null && p.objectReferenceValue == null) p.objectReferenceValue = value;
    }
}
