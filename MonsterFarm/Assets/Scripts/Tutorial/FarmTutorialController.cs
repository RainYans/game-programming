using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// First-launch step-by-step onboarding in the farm. It runs AFTER the How-to-Play manual has been
/// dismissed (it waits on TutorialState.ManualSeen) and walks a brand-new player through the core
/// loop by the real action: move -> plant a monster -> visit the Shop AND buy something -> ride out
/// from the War Camp. A GroundGuideTrail paints a path of arrows on the ground toward each target,
/// and the TutorialBanner shows the prompt + key hints. While onboarding is running, the War Camp is
/// gated (BlockRaid) so the player can't skip straight into the combat tutorial out of order.
public class FarmTutorialController : MonoBehaviour
{
    [Header("Refs (auto-resolved if left empty)")]
    [SerializeField] private TutorialBanner banner;
    [SerializeField] private GroundGuideTrail trail;
    [SerializeField] private Transform avatar;
    [SerializeField] private FarmActions farmActions;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MessageToast toast;
    [SerializeField] private ShopPanelUI shopPanel;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private ItemInventory itemInventory;
    [Tooltip("A soil tile to point the guide trail at during the Move/Plant steps. Optional — leave " +
             "empty to show the banner only with no ground arrows for those steps.")]
    [SerializeField] private Transform plotHint;

    [Header("Tuning")]
    [SerializeField] private float moveThreshold = 1.6f;
    [SerializeField] private bool alwaysShowInEditor = false;

    private enum Step { Move, Plant, Shop, Raid }
    private const int TotalSteps = 4;

    private Step step;
    private Vector3 startPos;
    private int cropBaseline;
    private bool running;
    private bool harvestTipShown;
    private bool awaitingBegin; // set in Start; Update polls until the manual is dismissed, then begins
    private bool hasManual;

    // Shop step has two phases: walk-to-shop, then buy-something.
    private bool shopBuyPhase;
    private int buyBaseline;
    private Button cachedBuyButton;

    private Transform shopT, warcampT;
    private RectTransform bannerRT;

    /// True while onboarding is still on the basics (not yet at the "ride out" step) — the War Camp
    /// holds the player with a hint instead of launching the combat tutorial early.
    public bool BlockRaid => running && step != Step.Raid;

    private void Start()
    {
        if (avatar == null) { var ac = FindFirstObjectByType<AvatarController>(); if (ac != null) avatar = ac.transform; }
        if (farmActions == null) farmActions = FindFirstObjectByType<FarmActions>();
        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
        if (toast == null) toast = FindFirstObjectByType<MessageToast>();
        if (shopPanel == null) shopPanel = FindFirstObjectByType<ShopPanelUI>();
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
        if (itemInventory == null) itemInventory = FindFirstObjectByType<ItemInventory>();

        bool firstTime = !TutorialState.FarmOnboardDone;
        if (Application.isEditor && alwaysShowInEditor) firstTime = true;
        if (!firstTime || banner == null)
        {
            if (banner != null) banner.HideImmediate();
            if (trail != null) trail.Hide();
            enabled = false;
            return;
        }

        banner.SkipRequested += Skip;
        bannerRT = banner.transform as RectTransform;
        hasManual = FindFirstObjectByType<ManualBookController>() != null;
        awaitingBegin = true;
    }

    /// Capture baselines and show the first step. Called from Update once the How-to-Play manual has
    /// been dismissed (polled, rather than waited on in a coroutine, so a stray deactivation can't
    /// freeze the start).
    private void Begin()
    {
        awaitingBegin = false;
        startPos = avatar != null ? avatar.position : transform.position;
        cropBaseline = farmActions != null ? farmActions.Crops.Count : 0;
        shopT = BuildingOf(BuildingType.Shop);
        warcampT = BuildingOf(BuildingType.WarCamp);

        step = Step.Move;
        running = true;
        banner.Begin(TotalSteps);
        ShowStep();
    }

    private void Update()
    {
        if (awaitingBegin)
        {
            // Wait for the manual to be dismissed (if one exists) before coaching the player.
            if (!hasManual || TutorialState.ManualSeen) Begin();
            return;
        }
        if (!running) return;

        switch (step)
        {
            case Step.Move:
                if (avatar != null && (avatar.position - startPos).sqrMagnitude > moveThreshold * moveThreshold)
                    Advance(Step.Plant);
                break;

            case Step.Plant:
                if (farmActions != null && farmActions.Crops.Count > cropBaseline)
                {
                    if (!harvestTipShown && toast != null)
                    { toast.Show("Nice! It grows over time — stand on it and press E to harvest."); harvestTipShown = true; }
                    Advance(Step.Shop);
                }
                break;

            case Step.Shop:
                bool shopOpen = uiManager != null && uiManager.CurrentPage == PageType.Shop;
                if (shopOpen)
                {
                    if (!shopBuyPhase) EnterBuyPhase();
                    PulseBuyButton();
                    if (BoughtTotal() > buyBaseline) { RestoreBuyButton(); Advance(Step.Raid); }
                }
                else if (shopBuyPhase)
                {
                    // Player closed the shop before buying — revert to the "go to the Shop" prompt.
                    shopBuyPhase = false;
                    RestoreBuyButton();
                    ShowStep();
                }
                break;

            case Step.Raid:
                // Reaching the War Camp routes into the combat tutorial (UIManager), which marks the
                // farm onboarding done. Nothing to poll here; just keep the trail pointing.
                break;
        }

        UpdateTrail();
    }

    private void Advance(Step next)
    {
        step = next;
        ShowStep();
        SfxManager.Play(SfxKind.ButtonClick);
    }

    private void ShowStep()
    {
        shopBuyPhase = false;
        switch (step)
        {
            case Step.Move:
                banner.SetStep(0, TotalSteps, "Welcome to Monster Farm! Move with WASD to explore.", "[W]", "[A]", "[S]", "[D]"); break;
            case Step.Plant:
                banner.SetStep(1, TotalSteps, "Step onto the soil, press E, then choose a monster to plant.", "[ E ]"); break;
            case Step.Shop:
                banner.SetStep(2, TotalSteps, "Head to the Shop and press E to buy seeds & items.", "[ E ]"); break;
            case Step.Raid:
                banner.SetStep(3, TotalSteps, "Ready? Go to the War Camp and press E to ride out!", "[ E ]"); break;
        }
        // The War Camp / "ride out" gate sits at the BOTTOM of the farm, so put the banner up top
        // for that step; for the others keep it at the bottom (where it won't cover the seed-pick
        // popup that opens during planting).
        SetBannerTop(step == Step.Raid);
        UpdateTrail();
    }

    /// Park the prompt banner at the top or bottom of the screen so it never covers what the current
    /// step needs the player to see (bottom buildings for the raid step, the seed popup for planting).
    private void SetBannerTop(bool top)
    {
        if (bannerRT == null) return;
        Vector2 anchor = top ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        bannerRT.anchorMin = anchor;
        bannerRT.anchorMax = anchor;
        bannerRT.pivot = anchor;
        bannerRT.anchoredPosition = top ? new Vector2(0f, -110f) : new Vector2(0f, 110f);
    }

    private void EnterBuyPhase()
    {
        shopBuyPhase = true;
        buyBaseline = BoughtTotal();
        banner.SetStep(2, TotalSteps, "Now click Buy to purchase a seed or item.", "[ Buy ]");
        if (trail != null) trail.Hide();
    }

    private int BoughtTotal()
    {
        int s = seedInventory != null ? seedInventory.Total : 0;
        int i = itemInventory != null ? itemInventory.Total : 0;
        return s + i;
    }

    private void PulseBuyButton()
    {
        if (cachedBuyButton == null && shopPanel != null) cachedBuyButton = shopPanel.FirstVisibleBuyButton();
        if (cachedBuyButton != null)
        {
            float s = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 6f);
            cachedBuyButton.transform.localScale = new Vector3(s, s, 1f);
        }
    }

    private void RestoreBuyButton()
    {
        if (cachedBuyButton != null) cachedBuyButton.transform.localScale = Vector3.one;
        cachedBuyButton = null;
    }

    private void UpdateTrail()
    {
        if (trail == null) return;
        switch (step)
        {
            case Step.Move:
            case Step.Plant:
                if (plotHint != null) trail.Point(plotHint); else trail.Hide();
                break;
            case Step.Shop:
                if (shopBuyPhase) trail.Hide();
                else if (shopT != null) trail.Point(shopT); else trail.Hide();
                break;
            case Step.Raid:
                if (warcampT != null) trail.Point(warcampT); else trail.Hide();
                break;
        }
    }

    private void Skip()
    {
        running = false;
        TutorialState.FarmOnboardDone = true;
        RestoreBuyButton();
        if (banner != null) banner.HideImmediate();
        if (trail != null) trail.Hide();
    }

    private Transform BuildingOf(BuildingType type)
    {
        foreach (Building b in FindObjectsByType<Building>(FindObjectsSortMode.None))
            if (b.type == type) return b.transform;
        return null;
    }
}
