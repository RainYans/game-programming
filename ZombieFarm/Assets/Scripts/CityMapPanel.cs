using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Farm-side city-selection map, opened at the WarCamp. Shows the world's cities as nodes on a
/// branch — available / locked / cleared — with a one-line difficulty hint each. Picking an
/// available city opens the DeployPanel for it; a placeholder node (no MissionData yet) just
/// says "Coming soon". Cleared state is read from CityProgress and unlocks adjacent nodes.
///
/// Like ShopPanelUI, the node visuals are built at runtime from the serialized `cities` list, so
/// the editor setup only fills data (id / mission / position / unlock rule), not per-node UI.
public class CityMapPanel : MonoBehaviour
{
    [Header("Data / scene refs")]
    [SerializeField] private DeployPanel deployPanel;
    [SerializeField] private CityProgress cityProgress;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;

    [Header("UI (wired by the setup menu; editable in the scene)")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button backdropButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform nodeParent;

    [Header("Cities")]
    [SerializeField] private List<CityNodeData> cities = new List<CityNodeData>();

    private static readonly Color AvailableColor = new Color(0.85f, 0.68f, 0.25f);
    private static readonly Color LockedColor    = new Color(0.30f, 0.32f, 0.38f);
    private static readonly Color ClearedColor   = new Color(0.35f, 0.70f, 0.40f);
    private static readonly Color LineColor      = new Color(1f, 1f, 1f, 0.18f);

    private readonly List<Node> nodes = new List<Node>();
    private bool isOpen;
    private bool built;
    private static Sprite discSprite;

    [System.Serializable]
    public class CityNodeData
    {
        public string cityId = "city1";
        [Tooltip("The city to deploy to. Leave empty for a placeholder node (shows 'Coming soon').")]
        public MissionData mission;
        [Tooltip("Used only when 'mission' is empty (placeholder nodes).")]
        public string title = "Unknown City";
        [Tooltip("Used only when 'mission' is empty.")]
        public string hint = "";
        [Tooltip("Anchored position of the node inside the map area.")]
        public Vector2 mapPos;
        [Tooltip("Available without any prerequisite (the first city).")]
        public bool availableAtStart;
        [Tooltip("This node unlocks once ALL of these city ids are cleared.")]
        public string[] unlockAfter = new string[0];
    }

    private struct Node
    {
        public CityNodeData data;
        public Image background;
        public TMP_Text statusLabel;
        public Button button;
    }

    private void Awake()
    {
        if (deployPanel == null) deployPanel = FindFirstObjectByType<DeployPanel>();
        if (cityProgress == null) cityProgress = FindFirstObjectByType<CityProgress>();
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();

        if (content == null || nodeParent == null)
        {
            Debug.LogWarning("[CityMapPanel] UI not wired. Run Tools > Zombie Farm > Setup City Map.");
            return;
        }

        WireOnce(backdropButton, Close);
        WireOnce(closeButton, Close);
        Hide();
    }

    private void Update()
    {
        if (!isOpen) return;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb[Key.Escape].wasPressedThisFrame) Close();
    }

    public void Open()
    {
        if (content == null) return;
        if (!built) BuildNodes();
        isOpen = true;
        content.SetActive(true);
        SetFarmInput(false);
        RefreshStates();
        SfxManager.Play(SfxKind.ButtonClick);
    }

    public void Close()
    {
        isOpen = false;
        Hide();
        SetFarmInput(true);
    }

    private void Hide()
    {
        if (content != null) content.SetActive(false);
    }

    // --- node building ------------------------------------------------------

    private void BuildNodes()
    {
        built = true;
        if (nodeParent == null) return;

        // Connecting lines first so they sit behind the nodes.
        foreach (CityNodeData c in cities)
        {
            if (c.unlockAfter == null) continue;
            foreach (string prereqId in c.unlockAfter)
            {
                CityNodeData from = cities.Find(x => x.cityId == prereqId);
                if (from != null) DrawLine(from.mapPos, c.mapPos);
            }
        }

        foreach (CityNodeData c in cities)
            nodes.Add(BuildNode(c));
    }

    private Node BuildNode(CityNodeData c)
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        var root = NewUI("Node_" + c.cityId, nodeParent);
        var rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = rootRT.anchorMax = rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.anchoredPosition = c.mapPos;
        rootRT.sizeDelta = new Vector2(170, 150);

        // Circular node icon (placeholder disc, tinted per state).
        var discGo = NewUI("Disc", root.transform);
        var discRT = discGo.GetComponent<RectTransform>();
        discRT.anchorMin = discRT.anchorMax = discRT.pivot = new Vector2(0.5f, 1f);
        discRT.anchoredPosition = new Vector2(0f, 0f);
        discRT.sizeDelta = new Vector2(76, 76);
        var disc = discGo.AddComponent<Image>();
        disc.sprite = EnsureDisc();
        disc.color = AvailableColor;
        var button = discGo.AddComponent<Button>();
        var cb = button.colors;
        cb.highlightedColor = new Color(1f, 1f, 1f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        button.colors = cb;
        string id = c.cityId;
        button.onClick.AddListener(() => OnNodeClicked(id));

        string title = c.mission != null && !string.IsNullOrEmpty(c.mission.cityName) ? c.mission.cityName : c.title;
        MakeText(root.transform, title, 19, new Color(1f, 0.96f, 0.8f), font,
            new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(180, 26), FontStyles.Bold);

        var status = MakeText(root.transform, "", 14, Color.white, font,
            new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(180, 22), FontStyles.Normal);

        string hint = c.mission != null && !string.IsNullOrEmpty(c.mission.mapHint) ? c.mission.mapHint : c.hint;
        MakeText(root.transform, hint, 12, new Color(0.75f, 0.80f, 0.88f), font,
            new Vector2(0.5f, 1f), new Vector2(0f, -124f), new Vector2(190, 30), FontStyles.Italic);

        return new Node { data = c, background = disc, statusLabel = status, button = button };
    }

    private void DrawLine(Vector2 a, Vector2 b)
    {
        var go = NewUI("Link", nodeParent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        Vector2 mid = (a + b) * 0.5f;
        // Connect at the disc tops (nodes are pivoted near their disc); keep it simple — center to center.
        rt.anchoredPosition = mid;
        float len = Vector2.Distance(a, b);
        rt.sizeDelta = new Vector2(len, 5f);
        float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
        rt.localEulerAngles = new Vector3(0f, 0f, angle);
        var img = go.AddComponent<Image>();
        img.color = LineColor;
        go.transform.SetAsFirstSibling();
    }

    // --- state --------------------------------------------------------------

    private void RefreshStates()
    {
        foreach (Node n in nodes)
        {
            bool cleared = cityProgress != null && cityProgress.IsCleared(n.data.cityId);
            bool available = cleared || IsAvailable(n.data);
            bool hasContent = n.data.mission != null;

            Color col = cleared ? ClearedColor : available ? AvailableColor : LockedColor;
            if (n.background != null) n.background.color = col;

            if (n.statusLabel != null)
            {
                if (cleared) { n.statusLabel.text = "Cleared ✓"; n.statusLabel.color = ClearedColor; }
                else if (!available) { n.statusLabel.text = "Locked"; n.statusLabel.color = new Color(0.6f, 0.62f, 0.68f); }
                else if (!hasContent) { n.statusLabel.text = "Coming soon"; n.statusLabel.color = new Color(0.7f, 0.72f, 0.78f); }
                else { n.statusLabel.text = "Available"; n.statusLabel.color = new Color(1f, 0.9f, 0.55f); }
            }

            // Always clickable so locked/placeholder nodes can explain themselves via a toast.
            if (n.button != null) n.button.interactable = true;
        }
    }

    private bool IsAvailable(CityNodeData c)
    {
        if (c.availableAtStart) return true;
        if (c.unlockAfter == null || c.unlockAfter.Length == 0) return false;
        if (cityProgress == null) return false;
        foreach (string prereq in c.unlockAfter)
            if (!cityProgress.IsCleared(prereq)) return false;
        return true;
    }

    private void OnNodeClicked(string cityId)
    {
        CityNodeData c = cities.Find(x => x.cityId == cityId);
        if (c == null) return;

        bool cleared = cityProgress != null && cityProgress.IsCleared(cityId);
        bool available = cleared || IsAvailable(c);

        if (!available) { Toast($"Locked — clear an earlier city first."); return; }
        if (c.mission == null) { Toast("Coming soon — this city isn't built yet."); return; }

        SfxManager.Play(SfxKind.ButtonClick);
        Close();
        if (deployPanel != null) deployPanel.Open(c.mission);
    }

    // --- helpers ------------------------------------------------------------

    private void SetFarmInput(bool enabled)
    {
        if (avatarMovement != null) avatarMovement.enabled = enabled;
        if (avatarInteraction != null) avatarInteraction.enabled = enabled;
    }

    private void Toast(string msg)
    {
        var toast = FindFirstObjectByType<MessageToast>();
        if (toast != null) toast.Show(msg);
        else Debug.Log("[CityMapPanel] " + msg);
    }

    private static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text MakeText(Transform parent, string text, float size, Color color,
        TMP_FontAsset font, Vector2 anchor, Vector2 pos, Vector2 sizeDelta, FontStyles style)
    {
        var go = NewUI("Label", parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = true;
        if (font != null) tmp.font = font;
        return tmp;
    }

    private static void WireOnce(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    private static Sprite EnsureDisc()
    {
        if (discSprite != null) return discSprite;
        const int size = 64;
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        float cx = size * 0.5f, cy = size * 0.5f, r = size * 0.46f;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float a = Mathf.Clamp01(1f - (d - r + 1f));
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        discSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return discSprite;
    }
}
