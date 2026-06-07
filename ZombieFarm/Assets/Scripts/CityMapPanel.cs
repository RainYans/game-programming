using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Level-select panel ("Choose a Farm to Raid"). Shows a row of real, inspectable LevelCard
/// objects — one per farm/level. Clicking a card's RAID button opens the DeployPanel for that
/// mission. The cards are pre-built GameObjects in the scene (NOT generated at runtime) so they
/// can be inspected and edited in the Unity editor.
///
/// NOTE: the class is still named CityMapPanel so existing scene references (UIManager, HUD
/// battle button, WarCamp) keep working without rewiring. The GameObject is named
/// "LevelSelectPanel" in the hierarchy.
public class CityMapPanel : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private DeployPanel deployPanel;
    [SerializeField] private CityProgress cityProgress;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;

    [Header("UI refs")]
    [SerializeField] private GameObject content;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backdropButton;
    [SerializeField] private LevelCard[] cards = new LevelCard[0];

    private bool isOpen;

    private void Awake()
    {
        if (deployPanel == null) deployPanel = FindFirstObjectByType<DeployPanel>();
        if (cityProgress == null) cityProgress = FindFirstObjectByType<CityProgress>();
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();
        if (cards == null || cards.Length == 0) cards = GetComponentsInChildren<LevelCard>(true);

        WireOnce(closeButton, Close);
        WireOnce(backdropButton, Close);
        foreach (LevelCard card in cards)
        {
            if (card == null) continue;
            card.AutoBind();
            LevelCard c = card;
            if (c.raidButton != null) { c.raidButton.onClick.RemoveAllListeners(); c.raidButton.onClick.AddListener(() => OnRaid(c)); }
        }
        Hide();
    }

    private void OnDisable() { if (isOpen) SetFarmInput(true); isOpen = false; }

    private void Update()
    {
        if (!isOpen) return;
        if (Keyboard.current != null && Keyboard.current[Key.Escape].wasPressedThisFrame) Close();
    }

    public void Open()
    {
        if (content == null) return;
        content.transform.SetAsLastSibling();
        content.SetActive(true);
        RefreshCards();
        isOpen = true;
        SetFarmInput(false);
        SfxManager.Play(SfxKind.ButtonClick);
    }

    public void Close() { isOpen = false; Hide(); SetFarmInput(true); }
    private void Hide() { if (content != null) content.SetActive(false); }

    private void RefreshCards()
    {
        foreach (LevelCard card in cards)
        {
            if (card == null) continue;
            card.AutoBind();
            bool hasMission = card.mission != null;
            bool cleared = hasMission && cityProgress != null && cityProgress.IsCleared(card.mission.id);
            bool available = hasMission && (cleared || IsAvailable(card));

            if (card.titleText != null) card.titleText.text = hasMission ? card.mission.cityName : "Coming soon";
            if (card.hintText != null) card.hintText.text = hasMission ? card.mission.mapHint : "This farm isn't built yet.";
            if (card.rewardText != null) card.rewardText.text = hasMission ? ("Reward  " + card.mission.rewardAmount) : "";
            if (card.statusText != null) card.statusText.text = !hasMission ? "SOON" : cleared ? "CLEARED" : available ? "READY" : "LOCKED";
            if (card.lockOverlay != null) card.lockOverlay.SetActive(hasMission && !available);
            if (card.raidButton != null) card.raidButton.interactable = available;
        }
    }

    private bool IsAvailable(LevelCard card)
    {
        if (card.availableAtStart) return true;
        if (card.unlockAfter == null || card.unlockAfter.Length == 0) return false;
        if (cityProgress == null) return false;
        foreach (MissionData m in card.unlockAfter) if (m != null && !cityProgress.IsCleared(m.id)) return false;
        return true;
    }

    private void OnRaid(LevelCard card)
    {
        if (card == null || card.mission == null) { Toast("Coming soon — this farm isn't built yet."); return; }
        bool cleared = cityProgress != null && cityProgress.IsCleared(card.mission.id);
        if (!(card.availableAtStart || cleared || IsAvailable(card))) { Toast("Locked — clear an earlier farm first."); return; }
        SfxManager.Play(SfxKind.ButtonClick);
        Close();
        if (deployPanel != null) deployPanel.Open(card.mission);
    }

    private void SetFarmInput(bool enabled)
    {
        if (avatarMovement != null) avatarMovement.enabled = enabled;
        if (avatarInteraction != null) avatarInteraction.enabled = enabled;
    }

    private void Toast(string msg)
    {
        MessageToast t = FindFirstObjectByType<MessageToast>();
        if (t != null) t.Show(msg); else Debug.Log("[LevelSelect] " + msg);
    }

    private static void WireOnce(Button b, UnityEngine.Events.UnityAction a)
    {
        if (b == null) return;
        b.onClick.RemoveListener(a);
        b.onClick.AddListener(a);
    }
}
