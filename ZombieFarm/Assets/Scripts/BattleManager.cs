using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Orchestrates a city raid played out as a sequence of rooms. Each stage maps to one room
/// (rooms[i] hosts stage i): the squad starts in room 0; clearing a room's enemies opens the
/// gate to the next room and spawns its enemies; clearing the final room wins the city.
/// Squad and casualty list carry across rooms. Reads the deployed squad + mission from
/// BattleHandoff when present; otherwise uses the inspector test roster so the scene is still
/// playable on its own. On end, writes the result back to the handoff and offers Return-to-Farm.
public class BattleManager : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Transform leader;
    [SerializeField] private TMP_Text resultLabel;
    [SerializeField] private GameObject returnButton;
    [SerializeField] private string farmSceneName = "Farm";
    [SerializeField] private BattleCameraFollow battleCamera; // optional; auto-resolved

    [Header("Rooms (one per stage)")]
    [Tooltip("One Room per mission stage. Room 0 hosts the squad and stage 0's enemies. " +
             "Rooms[i].entranceGate is opened when stage i-1 clears (leave null for room 0).")]
    [SerializeField] private List<Room> rooms = new List<Room>();

    [Header("Mission (stages + reward) when played directly")]
    [SerializeField] private MissionData mission;

    [Header("Agent look")]
    [SerializeField] private Sprite agentSprite;
    [SerializeField] private float agentScale = 0.6f;
    [SerializeField] private int sortingOrder = 5;

    [Header("Fallback test battle (no deployment)")]
    [SerializeField] private List<ZombieData> testSquad = new List<ZombieData>();
    [SerializeField] private List<MissionData.EnemySpawn> testEnemies = new List<MissionData.EnemySpawn>();
    [SerializeField] private float spawnScatter = 1.2f;

    [System.Serializable]
    public class Room
    {
        public Transform squadSpawn;   // used for the first room only
        public Transform enemySpawn;
        public BattleGate entranceGate; // gate sealing this room's entrance; null for room 0
    }

    private readonly List<BattleAgent> players = new List<BattleAgent>();
    private readonly List<BattleAgent> enemies = new List<BattleAgent>();

    /// Read-only views for the command controller (click hit-testing, squad HUD, onion blast).
    public IReadOnlyList<BattleAgent> Players => players;
    public IReadOnlyList<BattleAgent> Enemies => enemies;
    public Transform Leader => leader;
    private readonly List<string> casualties = new List<string>();
    private MissionData activeMission;
    private List<MissionData.Stage> stages;
    private int currentStage;
    private bool ended;
    private static Sprite generatedSquare;

    private void Start()
    {
        Time.timeScale = 1f;
        if (resultLabel != null) resultLabel.text = string.Empty;
        if (battleCamera == null) battleCamera = FindFirstObjectByType<BattleCameraFollow>();
        WireButton(returnButton, ReturnToFarm, startHidden: true);

        activeMission = BattleHandoff.Mission != null ? BattleHandoff.Mission : mission;
        stages = ResolveStages();

        Transform squadAnchor = rooms.Count > 0 && rooms[0] != null ? rooms[0].squadSpawn : null;
        SpawnSquad(squadAnchor);

        currentStage = 0;
        if (stages.Count == 0) { End(true); return; }
        SpawnStage(0); // first room is already "open" — enemies are immediately present
    }

    private List<MissionData.Stage> ResolveStages()
    {
        if (activeMission != null && activeMission.stages != null && activeMission.stages.Count > 0)
            return activeMission.stages;

        List<MissionData.EnemySpawn> flat =
            activeMission != null && activeMission.enemies != null && activeMission.enemies.Count > 0
                ? activeMission.enemies
                : testEnemies;
        return new List<MissionData.Stage> { new MissionData.Stage { enemies = flat } };
    }

    private void SpawnSquad(Transform at)
    {
        if (BattleHandoff.HasDeployment)
        {
            foreach (BattleHandoff.DeployedUnit u in BattleHandoff.Squad)
                SpawnAgent(u.data, Team.Player, at, u.uid);
        }
        else
        {
            foreach (ZombieData strain in testSquad)
                SpawnAgent(strain, Team.Player, at, string.Empty);
        }
    }

    private void SpawnStage(int index)
    {
        if (stages == null || index < 0 || index >= stages.Count) return;
        Transform at = index < rooms.Count && rooms[index] != null ? rooms[index].enemySpawn : null;
        foreach (MissionData.EnemySpawn spawn in stages[index].enemies)
            for (int i = 0; i < Mathf.Max(1, spawn.count); i++)
                SpawnAgent(spawn.zombie, Team.Enemy, at, string.Empty);
    }

    private void SpawnAgent(ZombieData data, Team team, Transform at, string uid)
    {
        if (data == null) return;

        var go = new GameObject($"Agent_{team}_{data.id}");
        Vector3 origin = at != null ? at.position : transform.position;
        go.transform.position = origin + (Vector3)(Random.insideUnitCircle * spawnScatter);
        go.transform.localScale = Vector3.one * agentScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = agentSprite != null ? agentSprite : GeneratedSquare();
        sr.sortingOrder = sortingOrder;

        var agent = go.AddComponent<BattleAgent>();
        agent.Init(this, data, team, leader, uid);

        (team == Team.Player ? players : enemies).Add(agent);
    }

    /// Nearest living opponent of `asker` within `range`, or null.
    public BattleAgent NearestEnemyOf(BattleAgent asker, float range)
    {
        List<BattleAgent> foes = asker.Team == Team.Player ? enemies : players;
        BattleAgent best = null;
        float bestSqr = range * range;
        Vector2 from = asker.transform.position;

        foreach (BattleAgent f in foes)
        {
            if (f == null || !f.IsAlive) continue;
            float sqr = ((Vector2)f.transform.position - from).sqrMagnitude;
            if (sqr <= bestSqr) { bestSqr = sqr; best = f; }
        }
        return best;
    }

    public void OnAgentDied(BattleAgent agent)
    {
        if (agent.Team == Team.Player && !string.IsNullOrEmpty(agent.SourceUid))
            casualties.Add(agent.SourceUid);

        players.Remove(agent);
        enemies.Remove(agent);

        // A little screen kick on every death — bigger if it's one of ours.
        if (battleCamera != null)
            battleCamera.Shake(agent.Team == Team.Player ? 0.22f : 0.12f, 0.18f);

        CheckProgress();
    }

    private void CheckProgress()
    {
        if (ended) return;
        if (AliveCount(players) == 0) { End(false); return; }
        if (AliveCount(enemies) == 0) StageCleared();
    }

    private void StageCleared()
    {
        currentStage++;
        if (currentStage >= stages.Count) { End(true); return; }

        // Open the door to the next room, then spawn its enemies (they wait until the squad
        // walks in — BattleAgent enemy idle holds position).
        if (currentStage < rooms.Count && rooms[currentStage] != null && rooms[currentStage].entranceGate != null)
            rooms[currentStage].entranceGate.Open();
        SpawnStage(currentStage);
    }

    private void End(bool won)
    {
        ended = true;
        Time.timeScale = 1f;
        int reward = won && activeMission != null ? activeMission.rewardAmount : 0;
        BattleHandoff.SetResult(won, reward, new List<string>(casualties));

        string msg = won
            ? $"City reclaimed!  +{reward} resources" + CasualtyNote()
            : "Defeat — squad wiped.";
        if (resultLabel != null) resultLabel.text = msg;
        if (returnButton != null) returnButton.SetActive(true);
        SfxManager.Play(won ? SfxKind.Win : SfxKind.Lose);
        Debug.Log($"[BattleManager] {msg}");
    }

    private string CasualtyNote() => casualties.Count > 0 ? $"  (lost {casualties.Count})" : string.Empty;

    public void ReturnToFarm()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(farmSceneName);
    }

    private static int AliveCount(List<BattleAgent> list)
    {
        int n = 0;
        foreach (BattleAgent a in list) if (a != null && a.IsAlive) n++;
        return n;
    }

    private void WireButton(GameObject buttonGo, UnityEngine.Events.UnityAction action, bool startHidden)
    {
        if (buttonGo == null) return;
        var btn = buttonGo.GetComponent<Button>();
        if (btn != null) { btn.onClick.RemoveListener(action); btn.onClick.AddListener(action); }
        if (startHidden) buttonGo.SetActive(false);
    }

    private static Sprite GeneratedSquare()
    {
        if (generatedSquare != null) return generatedSquare;
        var tex = new Texture2D(8, 8) { filterMode = FilterMode.Point };
        var px = new Color[64];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        generatedSquare = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        return generatedSquare;
    }
}
