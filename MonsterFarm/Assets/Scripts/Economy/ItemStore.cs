using System;
using System.Collections.Generic;
using UnityEngine;

/// Generic id -> count store with a change event. Subclassed into Inventory (harvested
/// zombies) and SeedInventory (seed stock) so the two are distinct component types — code
/// that does FindFirstObjectByType<Inventory>() never accidentally grabs the seed store.
public abstract class ItemStore : MonoBehaviour
{
    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

    public event Action Changed;

    public int Get(string id) => counts.TryGetValue(id, out int n) ? n : 0;

    public int Total
    {
        get
        {
            int sum = 0;
            foreach (int v in counts.Values) sum += v;
            return sum;
        }
    }

    /// Read-only view of every id -> count entry, for the save system to serialize.
    public IReadOnlyDictionary<string, int> Entries => counts;

    public void Add(string id, int amount)
    {
        if (amount <= 0) return;
        counts[id] = Get(id) + amount;
        Changed?.Invoke();
    }

    public bool TryRemove(string id, int amount)
    {
        if (amount <= 0 || Get(id) < amount) return false;
        counts[id] = Get(id) - amount;
        Changed?.Invoke();
        return true;
    }

    public void Clear()
    {
        if (counts.Count == 0) return;
        counts.Clear();
        Changed?.Invoke();
    }
}
