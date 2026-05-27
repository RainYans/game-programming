using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// Orchestrates one real-time skirmish: spawns the player squad and the wild enemies, answers
/// "nearest enemy" queries for the agents, and resolves win (enemies cleared) / lose (squad
/// wiped). Slice 1 uses a hardcoded test roster set in the inspector; later slices feed it the
/// deployed squad + the current stage's enemies.
public class BattleManager : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Transform leader;
    [SerializeField] private Transform squadSpawn;
    [SerializeField] private Transform enemySpawn;
    [SerializeField] private TMP_Text resultLabel;

    [Header("Agent look")]
    [SerializeField] private Sprite agentSprite;
    [SerializeField] private float agentScale = 0.6f;
    [SerializeField] private int sortingOrder = 5;

    [Header("Slice 1 — hardcoded test battle")]
    [SerializeField] private List<ZombieData> testSquad = new List<ZombieData>();
    [SerializeField] private List<MissionData.EnemySpawn> testEnemies = new List<MissionData.EnemySpawn>();
    [SerializeField] private float spawnScatter = 1.5f;

    private readonly List<BattleAgent> players = new List<BattleAgent>();
    private readonly List<BattleAgent> enemies = new List<BattleAgent>();
    private bool ended;
    private static Sprite generatedSquare;

    private void Start()
    {
        if (resultLabel != null) resultLabel.text = string.Empty;
        SpawnAll();
    }

    private void SpawnAll()
    {
        foreach (ZombieData strain in testSquad)
            SpawnAgent(strain, Team.Player, squadSpawn);

        foreach (MissionData.EnemySpawn spawn in testEnemies)
            for (int i = 0; i < Mathf.Max(1, spawn.count); i++)
                SpawnAgent(spawn.zombie, Team.Enemy, enemySpawn);
    }

    private void SpawnAgent(ZombieData data, Team team, Transform at)
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
        agent.Init(this, data, team, leader, string.Empty);

        (team == Team.Player ? players : enemies).Add(agent);
    }

    /// Nearest living opponent of `asker` within `range`, or null. Players target enemies and
    /// vice-versa.
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
        players.Remove(agent);
        enemies.Remove(agent);
        CheckEnd();
    }

    private void CheckEnd()
    {
        if (ended) return;
        if (AliveCount(enemies) == 0) End("Victory! Stage cleared.");
        else if (AliveCount(players) == 0) End("Defeat — squad wiped.");
    }

    private void End(string message)
    {
        ended = true;
        if (resultLabel != null) resultLabel.text = message;
        Debug.Log($"[BattleManager] {message}");
    }

    private static int AliveCount(List<BattleAgent> list)
    {
        int n = 0;
        foreach (BattleAgent a in list) if (a != null && a.IsAlive) n++;
        return n;
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
