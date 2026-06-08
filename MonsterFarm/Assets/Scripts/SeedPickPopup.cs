using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Modal seed picker shown when the avatar interacts with empty soil. Listens to
/// FarmActions.PlantRequested, lists the plantable strains from GameConfig.seedCatalog (with
/// the owned count, disabled at zero), and plants the chosen strain at the requested cell.
///
/// Rows are generated in code (Cute Fantasy parchment skin + harvested-monster icon) so the
/// look matches the shop. Gameplay (PlantRequested/Plant/cancel, farm-input pause) is unchanged.
public class SeedPickPopup : MonoBehaviour
{
    [Header("Data / scene refs")]
    [SerializeField] private GameConfig config;
    [SerializeField] private FarmActions farmActions;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;

    [Header("UI (wired by the setup menu; editable in the scene)")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button backdropButton;
    [SerializeField] private Transform rowParent;
    [SerializeField] private GameObject seedRowTemplate; // editable row template, cloned per seed
    [SerializeField] private Button rowTemplate;   // legacy; no longer cloned
    [SerializeField] private Button cancelButton;

    [Header("Skin (appearance only — no gameplay)")]
    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private Sprite panelFrame;     // parchment 9-slice for the dialog
    [SerializeField] private Sprite rowSprite;      // wood/parchment 9-slice per row
    [SerializeField] private Sprite iconSlotSprite; // inset behind the monster icon
    [SerializeField] private Sprite buttonSprite;   // cancel button
    [SerializeField] private IconEntry[] icons = new IconEntry[0];

    [System.Serializable]
    public struct IconEntry { public string id; public Sprite sprite; }

    private static readonly Color Ink = new Color(0.29f, 0.19f, 0.11f);
    private static readonly Color InkSoft = new Color(0.47f, 0.37f, 0.24f);
    private static readonly Color Cream = new Color(0.97f, 0.93f, 0.84f);

    private readonly List<Row> rows = new List<Row>();
    private Vector3Int targetCell;
    private bool isOpen;

    private struct Row
    {
        public string id;
        public Button button;
        public TMP_Text nameLabel;
        public TMP_Text countLabel;
        public Image icon;
        public CropData seed;
    }

    private void Awake()
    {
        if (farmActions == null) farmActions = FindFirstObjectByType<FarmActions>();
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();

        if (content == null || rowParent == null)
        {
            Debug.LogWarning("[SeedPickPopup] UI not wired. Run Tools > Monster Farm > Setup Seed Pick Popup.");
            return;
        }

        if (backdropButton != null)
        {
            backdropButton.onClick.RemoveListener(Cancel);
            backdropButton.onClick.AddListener(Cancel);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(Cancel);
            cancelButton.onClick.AddListener(Cancel);
        }

        if (seedRowTemplate != null) seedRowTemplate.SetActive(false);
        StyleStatics();
        BuildRows();
        Hide();
    }

    private void OnEnable()
    {
        if (farmActions != null) farmActions.PlantRequested += Open;
    }

    private void OnDisable()
    {
        if (farmActions != null) farmActions.PlantRequested -= Open;
        // Safety: never leave the farm input disabled if this popup is torn down while open.
        if (isOpen) SetFarmInput(true);
        isOpen = false;
    }

    private void Update()
    {
        if (!isOpen) return;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb[Key.Escape].wasPressedThisFrame) Cancel();
    }

    public void Open(Vector3Int cell)
    {
        targetCell = cell;
        isOpen = true;
        if (content != null) content.SetActive(true);
        SetFarmInput(false);
        Refresh();
    }

    public void Cancel() => Close();

    private void Close()
    {
        isOpen = false;
        Hide();
        SetFarmInput(true);
    }

    private void Hide()
    {
        if (content != null) content.SetActive(false);
    }

    private void Pick(CropData seed)
    {
        if (seed == null) { Close(); return; }
        Vector3Int cell = targetCell;
        Close();
        farmActions?.Plant(cell, seed);
    }

    private Sprite IconFor(string id)
    {
        if (icons != null)
            foreach (IconEntry e in icons)
                if (e.id == id) return e.sprite;
        return null;
    }

    // ---- Skin the scene-built container (dialog / title / backdrop / cancel) ----
    private void StyleStatics()
    {
        if (backdropButton != null)
        {
            var bi = backdropButton.GetComponent<Image>();
            if (bi != null) bi.color = new Color(0f, 0f, 0f, 0.62f);
        }
        Transform panel = rowParent.parent;
        if (panel != null)
        {
            var pi = panel.GetComponent<Image>();
            if (pi != null && panelFrame != null) { pi.sprite = panelFrame; pi.type = Image.Type.Sliced; pi.pixelsPerUnitMultiplier = 4f; pi.color = Color.white; }
            var title = panel.Find("Title")?.GetComponent<TMP_Text>();
            if (title != null) { if (uiFont != null) title.font = uiFont; title.text = "Plant a Monster"; title.color = Ink; title.fontSize = 34; title.alignment = TextAlignmentOptions.Center; }
        }
        if (cancelButton != null)
        {
            var ci = cancelButton.GetComponent<Image>();
            if (ci != null && buttonSprite != null) { ci.sprite = buttonSprite; ci.type = Image.Type.Sliced; ci.pixelsPerUnitMultiplier = 6f; ci.color = Color.white;
                var cbk = cancelButton.colors; cbk.normalColor = Color.white; cbk.highlightedColor = new Color(1f,1f,0.92f); cbk.pressedColor = new Color(0.82f,0.82f,0.78f); cbk.selectedColor = Color.white; cancelButton.colors = cbk; }
            var cl = cancelButton.GetComponentInChildren<TMP_Text>(true);
            if (cl != null) { if (uiFont != null) cl.font = uiFont; cl.text = "Cancel"; cl.color = Cream; cl.fontSize = 26; }
        }
    }

    private void BuildRows()
    {
        rows.Clear();
        if (rowParent == null || seedRowTemplate == null || config == null) return;
        // clear previously built rows (keep the template)
        for (int i = rowParent.childCount - 1; i >= 0; i--)
        {
            GameObject ch = rowParent.GetChild(i).gameObject;
            if (ch != seedRowTemplate) Destroy(ch);
        }
        foreach (GameConfig.ShopEntry e in config.seedCatalog)
        {
            if (e.seed == null) continue;
            rows.Add(BuildRow(e.seed));
        }
    }

    private Row BuildRow(CropData seed)
    {
        GameObject go = Instantiate(seedRowTemplate, rowParent);
        go.name = "Seed_" + seed.id;
        go.SetActive(true);

        Transform iconT = go.transform.Find("IconSlot/Icon");
        Image icon = iconT != null ? iconT.GetComponent<Image>() : null;
        if (icon != null)
        {
            Sprite mon = IconFor(seed.id);
            if (mon != null) { icon.sprite = mon; icon.preserveAspect = true; icon.color = Color.white; icon.enabled = true; }
            else icon.color = new Color(0f, 0f, 0f, 0f);
        }

        TMP_Text name = Child<TMP_Text>(go, "Name"); if (name != null) name.text = seed.displayName;
        TMP_Text count = Child<TMP_Text>(go, "Count"); if (count != null) count.text = "x0";

        Button btn = go.GetComponent<Button>();
        if (btn != null) { btn.onClick.RemoveAllListeners(); CropData captured = seed; btn.onClick.AddListener(() => Pick(captured)); }

        return new Row { id = seed.id, button = btn, nameLabel = name, countLabel = count, icon = icon, seed = seed };
    }

    private static T Child<T>(GameObject row, string child) where T : Component
    {
        Transform t = row.transform.Find(child);
        return t != null ? t.GetComponent<T>() : null;
    }

    private void Refresh()
    {
        foreach (Row r in rows)
        {
            int owned = seedInventory != null ? seedInventory.Get(r.id) : 0;
            if (r.nameLabel != null) r.nameLabel.text = r.seed.displayName;
            if (r.countLabel != null) r.countLabel.text = "x" + owned;
            if (r.button != null) r.button.interactable = owned > 0;
            if (r.icon != null && r.icon.sprite != null) r.icon.color = owned > 0 ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        }
    }

    private void SetFarmInput(bool enabled)
    {
        if (avatarMovement != null) avatarMovement.enabled = enabled;
        if (avatarInteraction != null) avatarInteraction.enabled = enabled;
    }
}
