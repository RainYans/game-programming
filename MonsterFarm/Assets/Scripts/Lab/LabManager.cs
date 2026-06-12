using System.Collections.Generic;
using UnityEngine;

/// Persistent per-strain upgrade levels bought at the Lab. A real component on the farm Systems
/// object (assign GameConfig in the Inspector; Wallet is auto-found). Spending goes through the
/// Wallet (which autosaves); SaveManager reads Levels to persist them and calls LoadLevels on
/// load. The upgrade scales a strain's max HP and attack when it is deployed
/// (see DeployPanel.Deploy -> BattleAgent.Init).
public class LabManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private Wallet wallet;

    private readonly Dictionary<string, int> levels = new Dictionary<string, int>();

    /// Raised when an upgrade is bought, or when levels are loaded (SaveManager listens to persist).
    public event System.Action Changed;

    public IReadOnlyDictionary<string, int> Levels => levels;

    private void Awake()
    {
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
    }

    public int GetLevel(string strainId) => levels.TryGetValue(strainId, out int v) ? v : 0;
    public int MaxLevel => config != null ? config.labMaxLevel : 0;
    public bool CanUpgrade(string strainId) => config != null && GetLevel(strainId) < config.labMaxLevel;
    public int CostFor(string strainId) => config != null ? config.LabCostFor(GetLevel(strainId)) : 0;
    public float MultiplierFor(string strainId) => config != null ? config.LabMultiplier(GetLevel(strainId)) : 1f;

    /// Spend resources to raise a strain one level. False if maxed or unaffordable.
    public bool TryUpgrade(string strainId)
    {
        if (config == null || wallet == null) return false;
        if (GetLevel(strainId) >= config.labMaxLevel) return false;
        int cost = config.LabCostFor(GetLevel(strainId));
        if (!wallet.TrySpend(cost)) return false;   // fires wallet.Changed -> autosave (old level)
        levels[strainId] = GetLevel(strainId) + 1;
        Changed?.Invoke();                          // -> SaveManager.Save persists the new level
        return true;
    }

    /// Replace all levels from a loaded save (strainId -> level).
    public void LoadLevels(Dictionary<string, int> loaded)
    {
        levels.Clear();
        if (loaded != null)
            foreach (KeyValuePair<string, int> kv in loaded) levels[kv.Key] = kv.Value;
        Changed?.Invoke();
    }
}
