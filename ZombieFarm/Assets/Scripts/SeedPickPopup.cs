using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Modal seed picker shown when the avatar interacts with empty soil. Listens to
/// FarmActions.PlantRequested, lists the plantable strains from GameConfig.seedCatalog (with
/// the owned count, disabled at zero), and plants the chosen strain at the requested cell.
///
/// The panel is a hand-built hierarchy in the scene (so you can restyle it in the editor) —
/// see Tools > Zombie Farm > Setup Seed Pick Popup, which creates and wires it. Only the seed
/// rows are data-driven: at runtime one row is cloned from `rowTemplate` per catalog entry.
public class SeedPickPopup : MonoBehaviour
{
    [Header("Data / scene refs")]
    [SerializeField] private GameConfig config;
    [SerializeField] private FarmActions farmActions;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;

    [Header("UI (wired by the setup menu; editable in the scene)")]
    [Tooltip("The dialog container toggled on/off. This component's GameObject stays active so " +
             "it keeps listening; only this child is shown/hidden.")]
    [SerializeField] private GameObject content;
    [Tooltip("Dimmed full-screen button behind the dialog; clicking it cancels.")]
    [SerializeField] private Button backdropButton;
    [Tooltip("Container the seed rows are added under (give it a VerticalLayoutGroup).")]
    [SerializeField] private Transform rowParent;
    [Tooltip("Inactive template row cloned once per catalog seed. A Button with a TMP_Text child.")]
    [SerializeField] private Button rowTemplate;
    [SerializeField] private Button cancelButton;

    private readonly List<Row> rows = new List<Row>();
    private Vector3Int targetCell;
    private bool isOpen;

    private struct Row
    {
        public string id;
        public Button button;
        public TMP_Text label;
        public CropData seed;
    }

    private void Awake()
    {
        if (farmActions == null) farmActions = FindFirstObjectByType<FarmActions>();
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();

        if (content == null || rowParent == null || rowTemplate == null)
        {
            Debug.LogWarning("[SeedPickPopup] UI not wired. Run Tools > Zombie Farm > " +
                             "Setup Seed Pick Popup.");
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
        // Close first (re-enables farm input) so nothing double-fires, then plant.
        Vector3Int cell = targetCell;
        Close();
        farmActions?.Plant(cell, seed);
    }

    private void BuildRows()
    {
        rows.Clear();
        rowTemplate.gameObject.SetActive(false);
        if (config == null) return;

        foreach (GameConfig.ShopEntry e in config.seedCatalog)
        {
            if (e.seed == null) continue;

            GameObject go = Instantiate(rowTemplate.gameObject, rowParent);
            go.name = "Seed_" + e.seed.id;
            go.SetActive(true);

            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<TMP_Text>(true);

            CropData captured = e.seed;
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Pick(captured));
            }

            rows.Add(new Row { id = e.seed.id, button = btn, label = label, seed = e.seed });
        }
    }

    private void Refresh()
    {
        foreach (Row r in rows)
        {
            int owned = seedInventory != null ? seedInventory.Get(r.id) : 0;
            if (r.label != null) r.label.text = $"{r.seed.displayName}    x{owned}";
            if (r.button != null) r.button.interactable = owned > 0;
        }
    }

    private void SetFarmInput(bool enabled)
    {
        if (avatarMovement != null) avatarMovement.enabled = enabled;
        if (avatarInteraction != null) avatarInteraction.enabled = enabled;
    }
}
