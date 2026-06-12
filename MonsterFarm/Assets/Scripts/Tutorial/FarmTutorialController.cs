using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// First-launch onboarding in the farm: a TutorialBanner walks a brand-new player through the core
/// loop — move, plant a monster, visit the shop, then head to the War Camp to ride out. Each step
/// advances on the real action (the avatar moved / a crop was planted / the shop opened), points a
/// TutorialArrow at the relevant building, and drops a MessageToast tip about harvesting. Only runs
/// the first time (TutorialState.FarmOnboardDone) unless the editor replay toggle is on. The actual
/// "first raid → tutorial battle" routing lives in UIManager; this just teaches and points.
public class FarmTutorialController : MonoBehaviour
{
    [Header("Refs (auto-resolved if left empty)")]
    [SerializeField] private TutorialBanner banner;
    [SerializeField] private TutorialArrow arrow;
    [SerializeField] private Transform avatar;
    [SerializeField] private FarmActions farmActions;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MessageToast toast;

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

    private void Start()
    {
        if (avatar == null) { var ac = FindFirstObjectByType<AvatarController>(); if (ac != null) avatar = ac.transform; }
        if (farmActions == null) farmActions = FindFirstObjectByType<FarmActions>();
        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
        if (toast == null) toast = FindFirstObjectByType<MessageToast>();

        bool firstTime = !TutorialState.FarmOnboardDone;
        if (Application.isEditor && alwaysShowInEditor) firstTime = true;
        if (!firstTime || banner == null) { if (banner != null) banner.HideImmediate(); enabled = false; return; }

        banner.SkipRequested += Skip;
        StartCoroutine(BeginSoon());
    }

    private IEnumerator BeginSoon()
    {
        yield return null;
        startPos = avatar != null ? avatar.position : transform.position;
        cropBaseline = farmActions != null ? farmActions.Crops.Count : 0;
        step = Step.Move;
        running = true;
        banner.Begin(TotalSteps);
        ShowStep();
    }

    private void Update()
    {
        if (!running) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) { Skip(); return; }

        switch (step)
        {
            case Step.Move:
                if (avatar != null && (avatar.position - startPos).sqrMagnitude > moveThreshold * moveThreshold) Advance(Step.Plant);
                break;
            case Step.Plant:
                if (farmActions != null && farmActions.Crops.Count > cropBaseline)
                {
                    if (!harvestTipShown && toast != null) { toast.Show("Nice! It will grow over time — stand on it and press E to harvest."); harvestTipShown = true; }
                    Advance(Step.Shop);
                }
                break;
            case Step.Shop:
                if (uiManager != null && uiManager.CurrentPage == PageType.Shop) Advance(Step.Raid);
                break;
            case Step.Raid:
                // Reaching this step means onboarding has been delivered; the War Camp routing
                // (UIManager) takes them into the tutorial battle. Mark it done so it won't replay.
                break;
        }
        UpdateArrow();
    }

    private void Advance(Step next)
    {
        step = next;
        ShowStep();
        SfxManager.Play(SfxKind.ButtonClick);
        if (step == Step.Raid) TutorialState.FarmOnboardDone = true;
    }

    private void ShowStep()
    {
        switch (step)
        {
            case Step.Move:
                banner.SetStep(0, TotalSteps, "Welcome to Monster Farm! Walk around with WASD.", "[W]", "[A]", "[S]", "[D]"); break;
            case Step.Plant:
                banner.SetStep(1, TotalSteps, "Stand on the soil and press E to plant a monster.", "[ E ]"); break;
            case Step.Shop:
                banner.SetStep(2, TotalSteps, "Walk to the Shop sign and press E to buy seeds & items.", "[ E ]"); break;
            case Step.Raid:
                banner.SetStep(3, TotalSteps, "Ready? Head to the War Camp and press E to ride out!", "[ E ]"); break;
        }
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (arrow == null) return;
        switch (step)
        {
            case Step.Shop: arrow.Point(BuildingOf(BuildingType.Shop)); break;
            case Step.Raid: arrow.Point(BuildingOf(BuildingType.WarCamp)); break;
            default: arrow.Hide(); break;
        }
    }

    private void Skip()
    {
        running = false;
        TutorialState.FarmOnboardDone = true;
        if (banner != null) banner.HideImmediate();
        if (arrow != null) arrow.Hide();
    }

    private Transform BuildingOf(BuildingType type)
    {
        foreach (Building b in FindObjectsByType<Building>(FindObjectsSortMode.None))
            if (b.type == type) return b.transform;
        return null;
    }
}
