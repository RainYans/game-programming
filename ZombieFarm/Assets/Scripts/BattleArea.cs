using System.Collections.Generic;
using UnityEngine;

/// One combat region in the battle map. The squad's LEADER walking into this area's trigger
/// activates it: BattleManager spawns the area's enemies; when they are all dead the area is
/// cleared and its exit gates open (which may unlock several branches). Supports branching +
/// backtracking — areas activate independently on entry, not in a fixed linear order.
[RequireComponent(typeof(Collider2D))]
public class BattleArea : MonoBehaviour
{
    public int areaId;
    public string label = "Area";
    [Tooltip("Where this area's enemies spawn.")]
    public Transform enemySpawn;
    [Tooltip("Enemy groups for this area.")]
    public List<Group> enemies = new List<Group>();
    [Tooltip("Gates opened when this area is cleared (paths to adjacent areas).")]
    public List<BattleGate> exitGates = new List<BattleGate>();
    [Tooltip("Clearing this area wins the battle.")]
    public bool isFinal;
    [Tooltip("Position on the minimap (arbitrary units; auto-scaled).")]
    public Vector2 mapPos;
    [Tooltip("Area ids this connects to, for drawing minimap links.")]
    public List<int> linkTo = new List<int>();

    public bool Cleared { get; private set; }
    public bool Activated { get; private set; }

    private BattleManager mgr;
    private readonly List<BattleAgent> spawned = new List<BattleAgent>();

    [System.Serializable]
    public struct Group { public ZombieData zombie; public int count; }

    public void Bind(BattleManager m) { mgr = m; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Activated || Cleared) return;
        if (other.GetComponentInParent<AvatarController>() == null) return; // only the leader
        Activate();
    }

    public void Activate()
    {
        if (Activated || Cleared) return;
        Activated = true;
        if (mgr == null) mgr = FindFirstObjectByType<BattleManager>();
        if (mgr != null) mgr.SpawnAreaEnemies(this, spawned);
        if (spawned.Count == 0) MarkCleared(); // nothing to fight
    }

    private void Update()
    {
        if (!Activated || Cleared) return;
        for (int i = spawned.Count - 1; i >= 0; i--)
            if (spawned[i] == null || !spawned[i].IsAlive) spawned.RemoveAt(i);
        if (spawned.Count == 0) MarkCleared();
    }

    private void MarkCleared()
    {
        Cleared = true;
        foreach (BattleGate g in exitGates) if (g != null) g.Open();
        if (mgr != null) mgr.OnAreaCleared(this);
    }
}
