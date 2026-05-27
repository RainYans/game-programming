using System;
using System.Collections.Generic;
using UnityEngine;

/// The player's standing army of harvested zombies. Each zombie is tracked as an individual
/// ZombieUnit (so it can carry its own hunger state and, later, be deployed and permanently
/// lost) rather than a fungible count. The legacy count surface (Get / Entries / Total /
/// Add / TryRemove) is kept as an aggregate view so existing consumers — the roamer spawner,
/// counter UI, deploy screen, and save system — keep working unchanged.
public class Inventory : MonoBehaviour
{
    [Tooltip("Source of the hunger delay (Full -> Hungry). Optional; falls back to a default " +
             "if unset, but assign it so hunger timing stays tunable from one place.")]
    [SerializeField] private GameConfig config;

    private const float FallbackHungerDelaySeconds = 60f;

    private readonly List<ZombieUnit> units = new List<ZombieUnit>();

    public event Action Changed;

    public IReadOnlyList<ZombieUnit> Units => units;

    public float HungerDelaySeconds => config != null ? config.hungerDelaySeconds : FallbackHungerDelaySeconds;

    public HungerState StateOf(ZombieUnit unit) => unit.State(HungerDelaySeconds);

    // --- Per-unit API -------------------------------------------------------

    /// Add a freshly harvested unit of a strain. It starts Full (weak) and drifts to Hungry.
    public ZombieUnit AddUnit(string strainId)
    {
        var unit = new ZombieUnit(strainId, DateTime.UtcNow);
        units.Add(unit);
        Changed?.Invoke();
        return unit;
    }

    /// Look a unit up by its stable id. Roamers hold the uid (not the object) so they keep
    /// resolving the right unit after a save reload rebuilds the roster.
    public ZombieUnit FindUnit(string uid) => units.Find(u => u.uid == uid);

    public bool RemoveUnit(string uid)
    {
        int i = units.FindIndex(u => u.uid == uid);
        if (i < 0) return false;
        units.RemoveAt(i);
        Changed?.Invoke();
        return true;
    }

    /// Replace the whole roster (used by the save system on load). Fires Changed once.
    public void LoadUnits(IEnumerable<ZombieUnit> loaded)
    {
        units.Clear();
        if (loaded != null) units.AddRange(loaded);
        Changed?.Invoke();
    }

    // --- Aggregate count view (back-compat surface) -------------------------

    public int Get(string id)
    {
        int n = 0;
        foreach (ZombieUnit u in units) if (u.strainId == id) n++;
        return n;
    }

    public int Total => units.Count;

    /// id -> count, rebuilt on demand for the save system and roamer reconcile.
    public IReadOnlyDictionary<string, int> Entries
    {
        get
        {
            var dict = new Dictionary<string, int>();
            foreach (ZombieUnit u in units)
                dict[u.strainId] = (dict.TryGetValue(u.strainId, out int c) ? c : 0) + 1;
            return dict;
        }
    }

    /// Add `amount` fresh units of a strain (all start Full). Fires Changed once.
    public void Add(string id, int amount)
    {
        if (amount <= 0) return;
        DateTime now = DateTime.UtcNow;
        for (int i = 0; i < amount; i++) units.Add(new ZombieUnit(id, now));
        Changed?.Invoke();
    }

    /// Remove up to `amount` units of a strain. Returns false if fewer are owned (removes none).
    public bool TryRemove(string id, int amount)
    {
        if (amount <= 0 || Get(id) < amount) return false;
        int removed = 0;
        for (int i = units.Count - 1; i >= 0 && removed < amount; i--)
            if (units[i].strainId == id) { units.RemoveAt(i); removed++; }
        Changed?.Invoke();
        return true;
    }

    public void Clear()
    {
        if (units.Count == 0) return;
        units.Clear();
        Changed?.Invoke();
    }
}
