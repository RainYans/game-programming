using System;
using System.Collections.Generic;
using UnityEngine;

/// Tracks which cities the player has cleared, by MissionData.id. Lives on the Systems object
/// next to the wallet/inventories; persisted by SaveManager and read by the CityMapPanel to
/// decide node state (available / locked / cleared) and unlocks. Fires Changed on every
/// mutation so the save system and any open map refresh.
public class CityProgress : MonoBehaviour
{
    private readonly HashSet<string> cleared = new HashSet<string>();

    public event Action Changed;

    public bool IsCleared(string cityId) => !string.IsNullOrEmpty(cityId) && cleared.Contains(cityId);

    public IReadOnlyCollection<string> Cleared => cleared;

    /// Record a city as cleared. No-op (no event) if it was already cleared.
    public void MarkCleared(string cityId)
    {
        if (string.IsNullOrEmpty(cityId)) return;
        if (cleared.Add(cityId)) Changed?.Invoke();
    }

    /// Replace the whole cleared set (used by the save system on load). Fires Changed once.
    public void LoadCleared(IEnumerable<string> ids)
    {
        cleared.Clear();
        if (ids != null) foreach (string id in ids) if (!string.IsNullOrEmpty(id)) cleared.Add(id);
        Changed?.Invoke();
    }
}
