using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Staged combat tutorial played in the minimal training-ground scene. Teaches one mechanic at a
/// time, spawning exactly what each step needs and clearing it before the next: move into a ring →
/// dash with Shift → attack a stationary dummy until it falls → command two allies onto a dummy →
/// throw an item and HIT the dummies. Every step has a soft fallback so it can never hard-lock (a
/// reach timeout, a command timeout, and an "items ran out" escape). Mandatory (no Skip). On
/// completion it shows a short "you're ready to raid" panel, then returns to the farm. Positions are
/// offsets from THIS object (place it at the arena centre) so the whole layout is editable.
public class CombatTutorialController : MonoBehaviour
{
    [Header("Refs (auto-resolved where possible)")]
    [SerializeField] private TutorialBanner banner;
    [SerializeField] private TutorialArrow arrow;
    [SerializeField] private Transform targetMarker;       // a ring sprite shown for move/dash goals
    [SerializeField] private BattleManager manager;
    [SerializeField] private LeaderDash leaderDash;
    [SerializeField] private BattleCommandController commandController;
    [SerializeField] private CanvasGroup sceneFade;        // optional black overlay for the exit

    [Header("End-of-training panel (city-map screenshot + 'go raid' text)")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Button endContinueButton;

    [Header("Units (assign strains)")]
    [SerializeField] private ZombieData dummyEnemy;        // stationary practice target
    [SerializeField] private ZombieData allyA;             // goblin ally #1
    [SerializeField] private ZombieData allyB;             // goblin ally #2

    [Header("Layout (offsets from this object = arena centre)")]
    [SerializeField] private Vector2 moveRingOffset = new Vector2(4f, 1.5f);
    [SerializeField] private Vector2 dashRingOffset = new Vector2(-4.5f, -1f);
    [SerializeField] private Vector2 attackDummyOffset = new Vector2(4f, 2f);
    [SerializeField] private Vector2 ally1Offset = new Vector2(-3.5f, -2f);
    [SerializeField] private Vector2 ally2Offset = new Vector2(-2f, -2.5f);
    [SerializeField] private Vector2 cmdEnemyOffset = new Vector2(4f, 1f);
    [SerializeField] private Vector2[] itemDummyOffsets =
        { new Vector2(-2.5f, 3f), new Vector2(0f, 3.4f), new Vector2(2.5f, 3f) };

    [Header("Tuning")]
    [SerializeField] private float reachRadius = 1.2f;
    [SerializeField] private float stepPause = 1.1f;
    [SerializeField] private float fadeTime = 0.6f;
    [SerializeField] private float reachTimeout = 15f;     // auto-pass the move step if stuck
    [SerializeField] private float commandTimeout = 8f;    // auto-pass the command step once issued
    [SerializeField] private string farmScene = "Farm";
    [SerializeField] private bool alwaysShowInEditor = false;

    private const int TotalSteps = 5;
    private Transform leader;
    private Vector3 markerPos;
    private bool endContinue;

    private void Start()
    {
        Time.timeScale = 1f;
        if (manager == null) manager = FindFirstObjectByType<BattleManager>();
        if (leaderDash == null) leaderDash = FindFirstObjectByType<LeaderDash>();
        if (commandController == null) commandController = FindFirstObjectByType<BattleCommandController>();

        if (endContinueButton != null) { endContinueButton.onClick.RemoveAllListeners(); endContinueButton.onClick.AddListener(() => endContinue = true); }
        if (endPanel != null) endPanel.SetActive(false);

        bool firstTime = !TutorialState.BattleTutorialDone || (Application.isEditor && alwaysShowInEditor);
        if (!firstTime || banner == null)
        {
            if (banner != null) banner.HideImmediate();
            if (arrow != null) arrow.Hide();
            if (targetMarker != null) targetMarker.gameObject.SetActive(false);
            enabled = false;
            return;
        }
        banner.SetSkippable(false); // mandatory tutorial
        StartCoroutine(Run());
    }

    private void Update()
    {
        // gentle pulse on the active target ring
        if (targetMarker != null && targetMarker.gameObject.activeSelf)
        {
            float s = 1f + 0.12f * Mathf.Sin(Time.unscaledTime * 4f);
            targetMarker.localScale = new Vector3(s, s, 1f);
        }
    }

    private IEnumerator Run()
    {
        yield return null; // let BattleManager bind the leader
        leader = manager != null ? manager.Leader : null;
        Vector3 c = transform.position;
        banner.Begin(TotalSteps);

        // 1) MOVE into the ring (auto-pass if the player gets stuck on geometry)
        Step(0, "Move into the glowing ring with WASD.", "[W]", "[A]", "[S]", "[D]");
        ShowMarker(c + (Vector3)moveRingOffset);
        yield return WaitOrTimeout(Reached, reachTimeout);
        yield return Cheer();

        // 2) DASH with Shift
        Step(1, "Tap Shift to dash toward the ring!", "[ Shift ]");
        ShowMarker(c + (Vector3)dashRingOffset);
        int dashBase = leaderDash != null ? leaderDash.DashCount : 0;
        yield return new WaitUntil(() => leaderDash != null && leaderDash.DashCount > dashBase);
        yield return Cheer();
        HideMarker();

        // 3) ATTACK a stationary dummy until it falls
        Step(2, "Get close and LEFT-CLICK to attack the dummy until it falls.", "[ Left-Click ]");
        BattleAgent d = manager.SpawnUnit(dummyEnemy, Team.Enemy, c + (Vector3)attackDummyOffset, true);
        if (arrow != null && d != null) arrow.Point(d.transform);
        yield return new WaitUntil(() => d == null || !d.IsAlive);
        yield return Cheer();

        // 4) COMMAND two allies onto a dummy (auto-pass a short while after the order is given so a
        //    weak squad can't hang the tutorial)
        Step(3, "RIGHT-CLICK the enemy to send your whole squad at it.", "[ Right-Click ]");
        manager.SpawnUnit(allyA, Team.Player, c + (Vector3)ally1Offset);
        manager.SpawnUnit(allyB, Team.Player, c + (Vector3)ally2Offset);
        BattleAgent cd = manager.SpawnUnit(dummyEnemy, Team.Enemy, c + (Vector3)cmdEnemyOffset, true);
        if (arrow != null && cd != null) arrow.Point(cd.transform);
        int cmdBase = commandController != null ? commandController.CommandCount : 0;
        float cmdAt = -1f;
        yield return new WaitUntil(() =>
        {
            if (commandController == null) return true;
            if (cmdAt < 0f && commandController.CommandCount > cmdBase) cmdAt = Time.unscaledTime;
            bool commanded = commandController.CommandCount > cmdBase;
            bool dead = cd == null || !cd.IsAlive;
            return commanded && (dead || Time.unscaledTime - cmdAt > commandTimeout);
        });
        yield return Cheer();

        // 5) ITEMS — clear allies, throw and HIT the dummies (escape if the player burns every item
        //    without landing one, so a string of misses can't soft-lock the tutorial)
        manager.ClearTeam(Team.Player);
        Step(4, "Press 1 or 2, then click to throw — land it on the enemies!", "[ 1 ]", "[ 2 ]");
        BattleAgent first = null;
        foreach (Vector2 off in itemDummyOffsets)
        {
            BattleAgent e = manager.SpawnUnit(dummyEnemy, Team.Enemy, c + (Vector3)off, true);
            if (first == null) first = e;
        }
        if (arrow != null && first != null) arrow.Point(first.transform);
        int hitBase = commandController != null ? commandController.ItemsHitCount : 0;
        yield return new WaitUntil(() =>
            commandController != null &&
            (commandController.ItemsHitCount > hitBase ||
             commandController.OnionsLeft + commandController.FreezesLeft <= 0));
        yield return Cheer();

        // Done — mark complete, celebrate once, then show the "go raid" panel before returning.
        if (arrow != null) arrow.Hide();
        HideMarker();
        TutorialState.BattleTutorialDone = true;
        banner.HideImmediate();
        SfxManager.Play(SfxKind.Win);
        yield return ShowEndPanel();
        yield return FadeAndLoad();
    }

    private void Step(int index, string message, params string[] keys)
    {
        banner.SetStep(index, TotalSteps, message, keys);
    }

    /// Wait until `done` is true, or `timeout` real-seconds elapse (a soft fallback so no step hangs).
    private IEnumerator WaitOrTimeout(System.Func<bool> done, float timeout)
    {
        float t0 = Time.unscaledTime;
        yield return new WaitUntil(() => done() || Time.unscaledTime - t0 > timeout);
    }

    private IEnumerator Cheer()
    {
        SfxManager.Play(SfxKind.ButtonClick); // soft per-step beat; the big Win is saved for the end
        yield return new WaitForSecondsRealtime(stepPause);
    }

    private IEnumerator ShowEndPanel()
    {
        if (endPanel == null) { yield return new WaitForSecondsRealtime(1.5f); yield break; }
        endContinue = false;
        endPanel.SetActive(true);
        float t0 = Time.unscaledTime;
        yield return new WaitUntil(() => endContinue || Time.unscaledTime - t0 > 12f);
        endPanel.SetActive(false);
    }

    private void ShowMarker(Vector3 pos)
    {
        markerPos = pos;
        if (targetMarker != null) { targetMarker.position = pos; targetMarker.gameObject.SetActive(true); }
        if (arrow != null && targetMarker != null) arrow.Point(targetMarker);
    }

    private void HideMarker()
    {
        if (targetMarker != null) targetMarker.gameObject.SetActive(false);
    }

    private bool Reached()
    {
        return leader != null && Vector2.Distance(leader.position, markerPos) <= reachRadius;
    }

    private IEnumerator FadeAndLoad()
    {
        if (sceneFade != null)
        {
            sceneFade.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                sceneFade.alpha = Mathf.Clamp01(t / fadeTime);
                yield return null;
            }
        }
        SceneManager.LoadScene(farmScene);
    }
}
