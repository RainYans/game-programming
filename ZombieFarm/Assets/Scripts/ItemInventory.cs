/// The player's stock of combat items (currently the Rotten Onion) keyed by item id. A
/// distinct ItemStore subclass — like Inventory and SeedInventory — so a
/// FindFirstObjectByType<ItemInventory>() never accidentally grabs the seed or zombie store.
/// Lives on the Systems object next to those two; persisted by SaveManager and carried into a
/// raid via BattleHandoff.
public class ItemInventory : ItemStore { }
