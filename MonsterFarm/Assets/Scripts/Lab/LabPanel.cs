using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Lab screen — spend resources to permanently upgrade a strain's HP + attack. Two-column layout
/// (built once as real, editable scene objects): LEFT a selectable list of monsters (icon + name),
/// RIGHT the selected monster's details + an Upgrade button. One left-row is cloned per strain.
public class LabPanel : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private LabManager lab;
    [SerializeField] private GameConfig config;
    [SerializeField] private Wallet wallet;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;

    [Header("UI — frame")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button backdropButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text walletLabel;

    [Header("UI — left list")]
    [SerializeField] private Transform listParent;
    [SerializeField] private GameObject listRowTemplate; // icon + name + selection outline

    [Header("UI — right detail")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailLevel;
    [SerializeField] private TMP_Text detailStats;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeLabel;

    private struct Row { public string id; public Outline outline; }
    private readonly List<Row> rows = new List<Row>();
    private string selectedId;
    private bool built, isOpen;

    private void Awake()
    {
        if (lab == null) lab = FindFirstObjectByType<LabManager>();
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();

        if (backdropButton != null) { backdropButton.onClick.RemoveListener(Close); backdropButton.onClick.AddListener(Close); }
        if (closeButton != null) { closeButton.onClick.RemoveListener(Close); closeButton.onClick.AddListener(Close); }
        if (upgradeButton != null) { upgradeButton.onClick.RemoveListener(UpgradeSelected); upgradeButton.onClick.AddListener(UpgradeSelected); }
        if (listRowTemplate != null) listRowTemplate.SetActive(false);
        Hide();
    }

    private void OnEnable()
    {
        if (lab != null) lab.Changed += Refresh;
        if (wallet != null) wallet.Changed += Refresh;
    }

    private void OnDisable()
    {
        if (lab != null) lab.Changed -= Refresh;
        if (wallet != null) wallet.Changed -= Refresh;
        if (isOpen) SetFarmInput(true);
        isOpen = false;
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
        BuildRows();
        if (string.IsNullOrEmpty(selectedId) && rows.Count > 0) selectedId = rows[0].id;
        content.transform.SetAsLastSibling();
        content.SetActive(true);
        isOpen = true;
        SetFarmInput(false);
        Refresh();
    }

    public void Close()
    {
        isOpen = false;
        Hide();
        SetFarmInput(true);
    }

    private void Hide() { if (content != null) content.SetActive(false); }

    private void BuildRows()
    {
        if (built || listParent == null || listRowTemplate == null || config == null) return;
        foreach (ZombieData z in config.allStrains)
        {
            if (z == null) continue;
            GameObject go = Instantiate(listRowTemplate, listParent);
            go.SetActive(true);
            go.name = "LabRow_" + z.id;

            Transform iconT = go.transform.Find("Icon");
            Image icon = iconT != null ? iconT.GetComponent<Image>() : null;
            if (icon != null)
            {
                Sprite mon = Resources.Load<Sprite>("Monsters/" + z.id);
                if (mon != null) { icon.sprite = mon; icon.preserveAspect = true; icon.enabled = true; }
                else icon.enabled = false;
            }
            TMP_Text nm = go.transform.Find("Name")?.GetComponent<TMP_Text>();
            if (nm != null) nm.text = !string.IsNullOrEmpty(z.displayName) ? z.displayName : z.id;

            Outline ol = go.GetComponent<Outline>();
            Button b = go.GetComponent<Button>();
            string id = z.id;
            if (b != null) { b.onClick.RemoveAllListeners(); b.onClick.AddListener(() => Select(id)); }
            rows.Add(new Row { id = id, outline = ol });
        }
        built = true;
    }

    private void Select(string id)
    {
        selectedId = id;
        SfxManager.Play(SfxKind.ButtonClick);
        Refresh();
    }

    private void UpgradeSelected()
    {
        if (string.IsNullOrEmpty(selectedId)) return;
        bool ok = lab != null && lab.TryUpgrade(selectedId);
        SfxManager.Play(ok ? SfxKind.Buy : SfxKind.ButtonClick);
        Refresh();
    }

    private void Refresh()
    {
        if (walletLabel != null && wallet != null) walletLabel.text = "Resources:  " + wallet.Resources;

        foreach (Row r in rows)
            if (r.outline != null) r.outline.enabled = (r.id == selectedId);

        ZombieData z = config != null ? config.FindStrain(selectedId) : null;
        if (z == null) return;

        int lvl = lab != null ? lab.GetLevel(selectedId) : 0;
        int max = lab != null ? lab.MaxLevel : 0;
        float curMul = lab != null ? lab.MultiplierFor(selectedId) : 1f;
        float nextMul = config != null ? config.LabMultiplier(lvl + 1) : curMul;
        int curHp = Mathf.RoundToInt(z.maxHp * curMul), curAtk = Mathf.RoundToInt(z.attack * curMul);
        int nextHp = Mathf.RoundToInt(z.maxHp * nextMul), nextAtk = Mathf.RoundToInt(z.attack * nextMul);
        int cost = lab != null ? lab.CostFor(selectedId) : 0;
        bool maxed = lvl >= max;
        bool afford = wallet != null && wallet.Resources >= cost;

        if (detailIcon != null)
        {
            Sprite mon = Resources.Load<Sprite>("Monsters/" + selectedId);
            if (mon != null) { detailIcon.sprite = mon; detailIcon.preserveAspect = true; detailIcon.enabled = true; }
            else detailIcon.enabled = false;
        }
        if (detailName != null) detailName.text = !string.IsNullOrEmpty(z.displayName) ? z.displayName : selectedId;
        if (detailLevel != null) detailLevel.text = $"Level {lvl} / {max}";
        if (detailStats != null)
        {
            string stats = maxed
                ? $"HP   {curHp}\nATK  {curAtk}\n\n<b>Fully upgraded.</b>"
                : $"HP   {curHp}  <color=#3a8a3a>→ {nextHp}</color>\nATK  {curAtk}  <color=#3a8a3a>→ {nextAtk}</color>";
            detailStats.text = stats + "\n\n<b>Passive — " + PassiveName(z.passive) + "</b>\n" + PassiveDesc(z.passive);
        }
        if (upgradeLabel != null) upgradeLabel.text = maxed ? "MAX" : $"Upgrade  ({cost})";
        if (upgradeButton != null) upgradeButton.interactable = !maxed && afford;
    }

    private void SetFarmInput(bool e)
    {
        if (avatarMovement != null) avatarMovement.enabled = e;
        if (avatarInteraction != null) avatarInteraction.enabled = e;
    }

    public static string PassiveName(Passive p) => p switch
    {
        Passive.ThickHide => "Thick Hide",
        Passive.Bloodlust => "Bloodlust",
        Passive.Evasion => "Evasion",
        Passive.Corrosion => "Corrosion",
        Passive.Aura => "Healing Aura",
        Passive.SelfDetonate => "Self-Detonate",
        _ => "None",
    };

    public static string PassiveDesc(Passive p) => p switch
    {
        Passive.ThickHide => "Reduces every hit it takes by a flat amount.",
        Passive.Bloodlust => "Repeated hits on the same target deal escalating damage.",
        Passive.Evasion => "Has a chance to dodge an incoming hit entirely.",
        Passive.Corrosion => "Its attacks make the target take extra damage for a while.",
        Passive.Aura => "Periodically heals nearby allied monsters.",
        Passive.SelfDetonate => "Explodes for area damage to enemies when it dies.",
        _ => "No special ability.",
    };
}
