using UnityEngine;

/// Spends wallet resources to add seeds to the SeedInventory, priced from GameConfig.
public class ShopController : MonoBehaviour
{
    [SerializeField] private Wallet wallet;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private ItemInventory itemInventory;
    [SerializeField] private GameConfig config;

    private void Awake()
    {
        if (wallet == null) wallet = FindFirstObjectByType<Wallet>();
        if (seedInventory == null) seedInventory = FindFirstObjectByType<SeedInventory>();
        if (itemInventory == null) itemInventory = FindFirstObjectByType<ItemInventory>();
    }

    /// Buy one seed of the given crop id. Returns false if the id isn't sold or funds are short.
    public bool Buy(string cropId)
    {
        if (config == null) return false;
        foreach (GameConfig.ShopEntry e in config.seedCatalog)
        {
            if (e.seed == null || e.seed.id != cropId) continue;
            if (!wallet.TrySpend(e.price))
            {
                ShowMessage($"Not enough resources! Need {e.price}.");
                return false;
            }
            seedInventory.Add(e.seed.id, 1);
            SfxManager.Play(SfxKind.Buy);
            ShowMessage($"Bought {e.seed.displayName} for {e.price} resources.");
            return true;
        }
        return false;
    }

    /// Buy one combat item of the given id. Returns false if the id isn't sold or funds are short.
    public bool BuyItem(string itemId)
    {
        if (config == null || itemInventory == null) return false;
        foreach (GameConfig.ItemEntry e in config.itemCatalog)
        {
            if (e.id != itemId) continue;
            if (!wallet.TrySpend(e.price))
            {
                ShowMessage($"Not enough resources! Need {e.price}.");
                return false;
            }
            itemInventory.Add(e.id, 1);
            SfxManager.Play(SfxKind.Buy);
            ShowMessage($"Bought {e.displayName} for {e.price} resources.");
            return true;
        }
        return false;
    }

    private void ShowMessage(string msg)
    {
        var toast = FindFirstObjectByType<MessageToast>();
        if (toast != null) toast.Show(msg);
    }
}
