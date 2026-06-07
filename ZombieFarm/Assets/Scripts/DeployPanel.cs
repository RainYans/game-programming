using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Squad-select screen (opened from the level-select panel). The panel chrome (title, card grid,
/// squad bar, buttons) and a card TEMPLATE are real, inspectable GameObjects built in the scene
/// (NOT generated from scratch in code). At runtime one card is Instantiated from the template per
/// harvested monster, so the grid is data-driven while the look stays defined by the editable
/// template. Gameplay (selection, Deploy, BattleHandoff) is unchanged.
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

    [Header("UI refs (real objects in the scene)")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button backdropButton;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private Transform gridRT;        // Grid (GridLayoutGroup) holding the cards
    [SerializeField] private GameObject cardTemplate; // inactive card under Grid, cloned per unit
    [SerializeField] private Image[] squadIcons;      // squad-bar slot icon Images
    [SerializeField] private TMP_Text counterLabel;
    [SerializeField] private Button deployButton;
    [SerializeField] private Button cancelButton;

    private static readonly Color Gold = new Color(0.59f, 0.37f, 0.08f);
    private static readonly Color Hungry = new Color(0.80f, 0.30f, 0.12f);
    private const int FallbackCap = 4;

    private readonly List<Card> cards = new List<Card>();
    private readonly HashSet<string> selected = new HashSet<string>();
    private bool isOpen;
    private MissionData activeMission;

    private struct Card { public string uid; public Outline sel; public Image check; public TMP_Text state; }

    private int Cap => config != null ? Mathf.Max(1, config.squadCap) : FallbackCap;

    private void Awake()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (itemInventory == null) itemInventory = FindFirstObjectByType<ItemInventory>();
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();
        WireOnce(backdropButton, Cancel); WireOnce(cancelButton, Cancel); WireOnce(deployButton, Deploy);
        if (cardTemplate != null) cardTemplate.SetActive(false);
        Hide();
    }

    private void Update()
    {
        if (!isOpen) return;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb[Key.Escape].wasPressedThisFrame) Cancel();
    }

    public void Open(MissionData forMission) { activeMission = forMission != null ? forMission : mission; Open(); }

    public void Open()
    {
        if (content == null) return;
        if (activeMission == null) activeMission = mission;
        if (titleLabel != null) titleLabel.text = "RAID  —  " + (activeMission != null && !string.IsNullOrEmpty(activeMission.cityName) ? activeMission.cityName : "Unknown Farm");
        BuildCards();
        selected.Clear();
        content.transform.SetAsLastSibling();
        content.SetActive(true);
        RefreshAll();
        isOpen = true;
        SetFarmInput(false);
    }

    public void Cancel() => Close();
    private void Close() { isOpen = false; Hide(); SetFarmInput(true); }
    private void OnDisable() { if (isOpen) SetFarmInput(true); isOpen = false; }
    private void Hide() { if (content != null) content.SetActive(false); }

    private void BuildCards()
    {
        cards.Clear();
        if (gridRT == null || cardTemplate == null || inventory == null) return;
        for (int i = gridRT.childCount - 1; i >= 0; i--)
        {
            GameObject ch = gridRT.GetChild(i).gameObject;
            if (ch != cardTemplate) Destroy(ch);
        }
        foreach (ZombieUnit unit in inventory.Units) cards.Add(BuildCard(unit));
    }

    private Card BuildCard(ZombieUnit unit)
    {
        ZombieData data = config != null ? config.FindStrain(unit.strainId) : null;
        string name = data != null && !string.IsNullOrEmpty(data.displayName) ? data.displayName : unit.strainId;
        string role = data != null ? (data.role + (data.passive != Passive.None ? " · " + data.passive : "")) : "";
        int hp = data != null ? data.maxHp : 0, atk = data != null ? data.attack : 0;

        GameObject go = Instantiate(cardTemplate, gridRT);
        go.name = "Card_" + unit.uid;
        go.SetActive(true);

        Transform iconT = go.transform.Find("Slot/Icon");
        Image icon = iconT != null ? iconT.GetComponent<Image>() : null;
        if (icon != null)
        {
            Sprite mon = Resources.Load<Sprite>("Monsters/" + unit.strainId);
            if (mon != null) { icon.sprite = mon; icon.preserveAspect = true; icon.enabled = true; } else icon.enabled = false;
        }

        SetLabel(go, "Name", name);
        SetLabel(go, "Role", role);
        TMP_Text st = Child<TMP_Text>(go, "Stats"); if (st != null) st.text = $"HP {hp}    ATK {atk}";

        Outline sel = go.GetComponent<Outline>(); if (sel != null) sel.enabled = false;
        Image check = Child<Image>(go, "Check"); if (check != null) check.enabled = false;

        Button btn = go.GetComponent<Button>(); if (btn != null) { btn.onClick.RemoveAllListeners(); string uid = unit.uid; btn.onClick.AddListener(() => Toggle(uid)); }
        return new Card { uid = unit.uid, sel = sel, check = check, state = st };
    }

    private static T Child<T>(GameObject card, string child) where T : Component
    {
        Transform t = card.transform.Find(child);
        return t != null ? t.GetComponent<T>() : null;
    }
    private static void SetLabel(GameObject card, string child, string text)
    {
        TMP_Text l = Child<TMP_Text>(card, child);
        if (l != null) l.text = text;
    }

    private void Toggle(string uid)
    {
        if (selected.Contains(uid)) selected.Remove(uid); else if (selected.Count < Cap) selected.Add(uid);
        RefreshAll();
    }

    private void RefreshAll()
    {
        foreach (Card c in cards)
        {
            bool s = selected.Contains(c.uid);
            if (c.sel != null) c.sel.enabled = s;
            if (c.check != null) c.check.enabled = s;
            if (c.state != null)
            {
                ZombieUnit u = inventory != null ? inventory.FindUnit(c.uid) : null;
                bool hungry = u != null && inventory.StateOf(u) == HungerState.Hungry;
                c.state.color = hungry ? Hungry : Gold;
            }
        }
        List<string> list = new List<string>(selected);
        if (squadIcons != null)
            for (int i = 0; i < squadIcons.Length; i++)
            {
                if (squadIcons[i] == null) continue;
                if (i < list.Count)
                {
                    ZombieUnit u = inventory != null ? inventory.FindUnit(list[i]) : null;
                    Sprite mon = u != null ? Resources.Load<Sprite>("Monsters/" + u.strainId) : null;
                    squadIcons[i].sprite = mon; squadIcons[i].enabled = mon != null;
                }
                else squadIcons[i].enabled = false;
            }
        if (counterLabel != null) counterLabel.text = selected.Count + " / " + Cap;
        if (deployButton != null) deployButton.interactable = selected.Count > 0;
    }

    private void Deploy()
    {
        if (selected.Count == 0 || inventory == null) return;
        var squad = new List<BattleHandoff.DeployedUnit>();
        foreach (string uid in selected)
        {
            ZombieUnit unit = inventory.FindUnit(uid); if (unit == null) continue;
            ZombieData data = config != null ? config.FindStrain(unit.strainId) : null; if (data == null) continue;
            bool hungry = inventory.StateOf(unit) == HungerState.Hungry;
            float dealt = hungry && config != null ? Mathf.Max(1f, config.hungryDamageMultiplier) : 1f;
            float taken = hungry && config != null ? Mathf.Max(1f, config.hungryDamageTakenMultiplier) : 1f;
            squad.Add(new BattleHandoff.DeployedUnit { uid = uid, data = data, damageMultiplier = dealt, damageTakenMultiplier = taken });
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

    private void SetFarmInput(bool enabled) { if (avatarMovement != null) avatarMovement.enabled = enabled; if (avatarInteraction != null) avatarInteraction.enabled = enabled; }
    private static void WireOnce(Button btn, UnityEngine.Events.UnityAction action) { if (btn == null) return; btn.onClick.RemoveListener(action); btn.onClick.AddListener(action); }
}
