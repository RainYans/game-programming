using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Farm-side deploy screen: opened at the WarCamp, it lists the player's roaming zombies and
/// lets them pick a squad (up to GameConfig.squadCap), then loads the Battle scene with that
/// squad + the mission via BattleHandoff. Hand-built panel (editable in the scene); rows are
/// cloned from a template per owned unit at open time. Same modal pattern as SeedPickPopup.
public class DeployPanel : MonoBehaviour
{
    [Header("Data / scene refs")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemInventory itemInventory;
    [SerializeField] private GameConfig config;
    [SerializeField] private MissionData mission;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;
    [SerializeField] private string battleSceneName = "Battle";

    [Header("UI (wired by the setup menu; editable in the scene)")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button backdropButton;
    [SerializeField] private Transform rowParent;
    [SerializeField] private Button rowTemplate;
    [SerializeField] private TMP_Text counterLabel;
    [SerializeField] private Button deployButton;
    [SerializeField] private Button cancelButton;

    private static readonly Color RowNormal = new Color(0.16f, 0.18f, 0.24f, 1f);
    private static readonly Color RowSelected = new Color(0.25f, 0.45f, 0.28f, 1f);
    private const int FallbackCap = 4;

    private readonly List<Row> rows = new List<Row>();
    private readonly HashSet<string> selected = new HashSet<string>();
    private bool isOpen;
    private MissionData activeMission; // the city being deployed to; defaults to the serialized one

    private struct Row
    {
        public string uid;
        public Button button;
        public Image bg;
        public TMP_Text label;
    }

    private int Cap => config != null ? Mathf.Max(1, config.squadCap) : FallbackCap;

    private void Awake()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (itemInventory == null) itemInventory = FindFirstObjectByType<ItemInventory>();
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();

        if (content == null || rowParent == null || rowTemplate == null)
        {
            Debug.LogWarning("[DeployPanel] UI not wired. Run Tools > Zombie Farm > Setup Deploy Panel.");
            return;
        }

        WireOnce(backdropButton, Cancel);
        WireOnce(cancelButton, Cancel);
        WireOnce(deployButton, Deploy);
        rowTemplate.gameObject.SetActive(false);
        Hide();
    }

    private void Update()
    {
        if (!isOpen) return;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb[Key.Escape].wasPressedThisFrame) Cancel();
    }

    /// Open for a specific city (called by the CityMapPanel). Falls back to the serialized
    /// mission when none is passed, so opening the panel directly still works.
    public void Open(MissionData forMission)
    {
        activeMission = forMission != null ? forMission : mission;
        Open();
    }

    public void Open()
    {
        if (content == null) return;
        if (activeMission == null) activeMission = mission;
        BuildRows();
        selected.Clear();
        isOpen = true;
        content.SetActive(true);
        SetFarmInput(false);
        RefreshAll();
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

    private void BuildRows()
    {
        foreach (Row r in rows) if (r.button != null) Destroy(r.button.gameObject);
        rows.Clear();
        if (inventory == null) return;

        foreach (ZombieUnit unit in inventory.Units)
        {
            GameObject go = Instantiate(rowTemplate.gameObject, rowParent);
            go.name = "Unit_" + unit.uid;
            go.SetActive(true);

            var btn = go.GetComponent<Button>();
            var bg = go.GetComponent<Image>();
            var label = go.GetComponentInChildren<TMP_Text>(true);

            string uid = unit.uid;
            if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => Toggle(uid)); }
            if (label != null) label.text = DescribeUnit(unit);

            rows.Add(new Row { uid = uid, button = btn, bg = bg, label = label });
        }
    }

    private string DescribeUnit(ZombieUnit unit)
    {
        string name = unit.strainId;
        ZombieData data = config != null ? config.FindStrain(unit.strainId) : null;
        if (data != null && !string.IsNullOrEmpty(data.displayName)) name = data.displayName;
        string hunger = inventory != null && inventory.StateOf(unit) == HungerState.Hungry ? "Hungry" : "Full";
        return $"{name} — {hunger}";
    }

    private void Toggle(string uid)
    {
        if (selected.Contains(uid)) selected.Remove(uid);
        else if (selected.Count < Cap) selected.Add(uid);
        RefreshAll();
    }

    private void RefreshAll()
    {
        foreach (Row r in rows)
            if (r.bg != null) r.bg.color = selected.Contains(r.uid) ? RowSelected : RowNormal;

        if (counterLabel != null) counterLabel.text = $"Squad: {selected.Count} / {Cap}";
        if (deployButton != null) deployButton.interactable = selected.Count > 0;
    }

    private void Deploy()
    {
        if (selected.Count == 0 || inventory == null) return;

        var squad = new List<BattleHandoff.DeployedUnit>();
        foreach (string uid in selected)
        {
            ZombieUnit unit = inventory.FindUnit(uid);
            if (unit == null) continue;
            ZombieData data = config != null ? config.FindStrain(unit.strainId) : null;
            if (data == null) continue;

            // Snapshot hunger -> a damage multiplier at deploy time, so the field result is fixed
            // regardless of how long the raid runs.
            bool hungry = inventory.StateOf(unit) == HungerState.Hungry;
            float dealtMult = hungry && config != null ? Mathf.Max(1f, config.hungryDamageMultiplier) : 1f;
            float takenMult = hungry && config != null ? Mathf.Max(1f, config.hungryDamageTakenMultiplier) : 1f;
            squad.Add(new BattleHandoff.DeployedUnit
            {
                uid = uid, data = data,
                damageMultiplier = dealtMult, damageTakenMultiplier = takenMult,
            });
        }
        if (squad.Count == 0) return;

        BattleHandoff.SetDeployment(squad, activeMission != null ? activeMission : mission);
        BattleHandoff.Config = config;
        BattleHandoff.OnionsCarried = itemInventory != null ? itemInventory.Get(GameConfig.RottenOnionId) : 0;
        BattleHandoff.FreezesCarried = itemInventory != null ? itemInventory.Get(GameConfig.FreezeCanisterId) : 0;
        BattleHandoff.ClearResult();
        Close();
        SceneManager.LoadScene(battleSceneName);
    }

    private void SetFarmInput(bool enabled)
    {
        if (avatarMovement != null) avatarMovement.enabled = enabled;
        if (avatarInteraction != null) avatarInteraction.enabled = enabled;
    }

    private static void WireOnce(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }
}
