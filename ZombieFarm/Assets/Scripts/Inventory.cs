using System;
using System.Collections.Generic;
using UnityEngine;

/// Item id -> count, with a change event for UI to subscribe to.
public class Inventory : MonoBehaviour
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
}
