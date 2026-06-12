using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// Drives the 5-step combat tutorial inside the dedicated tutorial battle scene. Watches real
/// gameplay signals (the leader moved / landed a swing / commanded the squad / cleared the area /
/// reached the exit) and advances a TutorialBanner step-by-step, pointing a TutorialArrow at the
/// relevant target. Adds NO combat logic — it only reads existing public state. Marks the combat
/// tutorial complete (TutorialState) once the player reaches the final area (a win), so the next
/// raid routes to a real city. Skippable via the banner's Skip button or Esc.
public class TutorialController : MonoBehaviour
{
    [Header("Refs (auto-resolved if left empty)")]
    [SerializeField] private TutorialBanner banner;
    [SerializeField] private TutorialArrow arrow;
    [SerializeField] private BattleManager manager;
    [SerializeField] private LeaderCombat leaderCombat;
    [SerializeField] private BattleCommandController commandController;

    [Header("Tuning")]
    [SerializeField] private float moveThreshold = 1.6f;
    [Tooltip("Replay the tutorial in the editor even if it was already completed once.")]
    [SerializeField] private bool alwaysShowInEditor = false;

    private enum Step { Move, Attack, Command, Clear, Gate }
    private const int TotalSteps = 5;

    private Step step;
    private Vector3 startPos;
    private int swingBaseline, commandBaseline;
    private bool running;
    private bool bannerActive;

    private void Start()
    {
        if (manager == null) manager = FindFirstObjectByType<BattleManager>();
        if (leaderCombat == null) leaderCombat = FindFirstObjectByType<LeaderCombat>();
        if (commandController == null) commandController = FindFirstObjectByType<BattleCommandController>();

        bool firstTime = !TutorialState.BattleTutorialDone;
        if (Application.isEditor && alwaysShowInEditor) firstTime = true;

        if (banner == null) { enabled = false; return; }
        banner.SkipRequested += Skip;

        // Always run the watcher (so a win marks the tutorial done even if the banner is skipped);
        // only show the banner UI the first time through.
        bannerActive = firstTime;
        if (!firstTime) banner.HideImmediate();
        StartCoroutine(BeginSoon());
    }

    private IEnumerator BeginSoon()
    {
        yield return null; // let BattleManager.Start spawn the squad + bind the leader
        Transform leader = manager != null ? manager.Leader : null;
        startPos = leader != null ? leader.position : transform.position;
        swingBaseline = leaderCombat != null ? leaderCombat.SwingHitCount : 0;
        commandBaseline = commandController != null ? commandController.CommandCount : 0;
        step = Step.Move;
        running = true;
        if (bannerActive) { banner.Begin(TotalSteps); ShowStep(); }
    }

    private void Update()
    {
        if (!running) return;

        // Hero down → battle lost; drop the banner and let the result panel take over.
        if (manager != null && manager.LeaderUnit != null && !manager.LeaderUnit.IsAlive)
        {
            running = false;
            if (banner != null) banner.HideImmediate();
            if (arrow != null) arrow.Hide();
            return;
        }

        // Reaching the final area is the win — mark done regardless of whether the banner is showing.
        if (FinalReached()) { Finish(); return; }

        if (!bannerActive) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) { Skip(); return; }

        // If a fast player clears the area before the command step, skip straight to "go to the gate".
        if (step < Step.Clear && AreaCleared()) { step = Step.Gate; ShowStep(); UpdateArrow(); return; }

        switch (step)
        {
            case Step.Move:
                Transform l = manager != null ? manager.Leader : null;
                if (l != null && (l.position - startPos).sqrMagnitude > moveThreshold * moveThreshold) Advance(Step.Attack);
                break;
            case Step.Attack:
                if (leaderCombat != null && leaderCombat.SwingHitCount > swingBaseline) Advance(Step.Command);
                break;
            case Step.Command:
                if (commandController != null && commandController.CommandCount > commandBaseline) Advance(Step.Clear);
                break;
            case Step.Clear:
                if (AreaCleared()) Advance(Step.Gate);
                break;
        }
        UpdateArrow();
    }

    private void Advance(Step next)
    {
        step = next;
        ShowStep();
        SfxManager.Play(SfxKind.ButtonClick);
    }

    private void ShowStep()
    {
        switch (step)
        {
            case Step.Move:
                banner.SetStep(0, TotalSteps, "Move your Commander with WASD.", "[W]", "[A]", "[S]", "[D]"); break;
            case Step.Attack:
                banner.SetStep(1, TotalSteps, "Get close and LEFT-CLICK to swing at the nearest enemy.", "[ Left-Click ]"); break;
            case Step.Command:
                banner.SetStep(2, TotalSteps, "RIGHT-CLICK to send your whole squad to attack.", "[ Right-Click ]"); break;
            case Step.Clear:
                banner.SetStep(3, TotalSteps, "Wipe out every enemy in this area!"); break;
            case Step.Gate:
                banner.SetStep(4, TotalSteps, "Well done — walk through the open gate to finish."); break;
        }
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (arrow == null) return;
        switch (step)
        {
            case Step.Attack:
            case Step.Command:
            case Step.Clear:
                arrow.Point(NearestEnemy());
                break;
            case Step.Gate:
                arrow.Point(FinalAreaTransform());
                break;
            default:
                arrow.Hide();
                break;
        }
    }

    private void Finish()
    {
        running = false;
        TutorialState.BattleTutorialDone = true;
        if (arrow != null) arrow.Hide();
        if (bannerActive && banner != null) banner.Finish("Tutorial complete! Clear the village to claim victory.");
    }

    private void Skip()
    {
        bannerActive = false;
        TutorialState.BattleTutorialDone = true; // they opted out — don't funnel them back here
        if (banner != null) banner.HideImmediate();
        if (arrow != null) arrow.Hide();
    }

    // --- signals from existing public state ------------------------------------------------

    private bool AreaCleared()
    {
        var areas = manager != null ? manager.Areas : null;
        return areas != null && areas.Count > 0 && areas[0] != null && areas[0].Cleared;
    }

    private BattleArea FinalArea()
    {
        var areas = manager != null ? manager.Areas : null;
        if (areas == null) return null;
        foreach (BattleArea a in areas) if (a != null && a.isFinal) return a;
        return areas.Count > 0 ? areas[areas.Count - 1] : null;
    }

    private bool FinalReached()
    {
        BattleArea f = FinalArea();
        return f != null && f.Activated;
    }

    private Transform FinalAreaTransform()
    {
        BattleArea f = FinalArea();
        return f != null ? f.transform : null;
    }

    private Transform NearestEnemy()
    {
        if (manager == null) return null;
        Transform leader = manager.Leader;
        Vector2 from = leader != null ? (Vector2)leader.position : Vector2.zero;
        Transform best = null;
        float bestSqr = float.MaxValue;
        foreach (BattleAgent e in manager.Enemies)
        {
            if (e == null || !e.IsAlive) continue;
            float s = ((Vector2)e.transform.position - from).sqrMagnitude;
            if (s < bestSqr) { bestSqr = s; best = e.transform; }
        }
        return best;
    }
}
