using System.Collections;
using UnityEngine;

/// Farm-side: when the scene loads after a raid, applies the BattleHandoff result —
/// permadeath (remove casualty uids from the roster) and the currency reward — then clears it.
/// Deferred one frame so it runs AFTER SaveManager has loaded the roster from disk (both run in
/// Start; order isn't guaranteed), otherwise the casualties would be re-added by the load.
public class BattleResultApplier : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemInventory itemInventory;
    [SerializeField] private Wallet wallet;
    [SerializeField] private CityProgress cityProgress;
    [SerializeField] private MessageToast toast;

    private void Awake()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (itemInventory == null) itemInventory = FindFirstObjectByType<ItemInventory>();
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (cityProgress == null) cityProgress = FindFirstObjectByType<CityProgress>();
        if (toast == null) toast = FindFirstObjectByType<MessageToast>();
    }

    private IEnumerator Start()
    {
        if (!BattleHandoff.HasResult) yield break;
        yield return null; // let SaveManager.Load repopulate the roster first

        int lost = 0;
        if (inventory != null && BattleHandoff.CasualtyUids != null)
            foreach (string uid in BattleHandoff.CasualtyUids)
                if (inventory.RemoveUnit(uid)) lost++;

        int reward = BattleHandoff.Won ? Mathf.Max(0, BattleHandoff.Reward) : 0;
        if (reward > 0 && wallet != null) wallet.Add(reward);

        // Onions thrown during the raid are spent. (Quitting mid-raid clears the result, so —
        // like surviving units — unused onions simply stay in stock.)
        if (itemInventory != null && BattleHandoff.OnionsUsed > 0)
            itemInventory.TryRemove(GameConfig.RottenOnionId, BattleHandoff.OnionsUsed);

        // Reclaiming a city marks it cleared on the map (unlocks adjacent nodes; shows a ✓).
        if (BattleHandoff.Won && cityProgress != null && BattleHandoff.Mission != null)
            cityProgress.MarkCleared(BattleHandoff.Mission.id);

        if (toast != null)
            toast.Show(BuildSummary(BattleHandoff.Won, reward, lost), 3.5f);

        BattleHandoff.ClearResult();
        BattleHandoff.ClearDeployment();
    }

    private static string BuildSummary(bool won, int reward, int lost)
    {
        if (won)
            return lost > 0
                ? $"Raid won! +{reward} resources, {lost} zombie(s) lost."
                : $"Raid won! +{reward} resources, no losses.";
        return lost > 0 ? $"Raid failed. {lost} zombie(s) lost." : "Raid failed.";
    }
}
