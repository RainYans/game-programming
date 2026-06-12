using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Monster bestiary / codex — a read-only two-column screen: LEFT a selectable list of monsters
/// (icon + name), RIGHT the selected monster's portrait, role, stats, passive, and backstory.
/// All UI lives as real, editable scene objects; one left-row is cloned per strain at runtime.
public class BestiaryPanel : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private GameConfig config;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;

    [Header("UI — frame")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button backdropButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openButton;   // HUD button that opens the codex

    [Header("UI — left list")]
    [SerializeField] private Transform listParent;
    [SerializeField] private GameObject listRowTemplate;

    [Header("UI — right detail")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailRole;
    [SerializeField] private TMP_Text detailStory;

    private struct Row { public string id; public Outline outline; }
    private readonly List<Row> rows = new List<Row>();
    private string selectedId;
    private bool built, isOpen;

    private void Awake()
    {
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();
        if (backdropButton != null) { backdropButton.onClick.RemoveListener(Close); backdropButton.onClick.AddListener(Close); }
        if (closeButton != null) { closeButton.onClick.RemoveListener(Close); closeButton.onClick.AddListener(Close); }
        if (openButton != null) { openButton.onClick.RemoveListener(Open); openButton.onClick.AddListener(Open); }
        if (listRowTemplate != null) listRowTemplate.SetActive(false);
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
        BuildRows();
        if (string.IsNullOrEmpty(selectedId) && rows.Count > 0) selectedId = rows[0].id;
        content.transform.SetAsLastSibling();
        content.SetActive(true);
        isOpen = true;
        SetFarmInput(false);
        SfxManager.Play(SfxKind.ButtonClick);
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
            go.name = "DexRow_" + z.id;
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

    private void Refresh()
    {
        foreach (Row r in rows)
            if (r.outline != null) r.outline.enabled = (r.id == selectedId);

        ZombieData z = config != null ? config.FindStrain(selectedId) : null;
        if (z == null) return;

        if (detailIcon != null)
        {
            Sprite mon = Resources.Load<Sprite>("Monsters/" + selectedId);
            if (mon != null) { detailIcon.sprite = mon; detailIcon.preserveAspect = true; detailIcon.enabled = true; }
            else detailIcon.enabled = false;
        }
        if (detailName != null) detailName.text = !string.IsNullOrEmpty(z.displayName) ? z.displayName : selectedId;
        string rng = z.range == AttackRange.Ranged ? "Ranged" : "Melee";
        if (detailRole != null) detailRole.text = $"{z.role}    ·    {rng}";

        if (detailStory != null)
        {
            string stats = $"HP {z.maxHp}     ATK {z.attack}     SPD {z.moveSpeed:0.0}";
            string passive = $"<b>Passive — {LabPanel.PassiveName(z.passive)}</b>\n{LabPanel.PassiveDesc(z.passive)}";
            string story = string.IsNullOrEmpty(z.backstory) ? "" : "\n\n<i>" + z.backstory + "</i>";
            detailStory.text = stats + "\n\n" + passive + story;
        }
    }

    private void SetFarmInput(bool e)
    {
        if (avatarMovement != null) avatarMovement.enabled = e;
        if (avatarInteraction != null) avatarInteraction.enabled = e;
    }
}
