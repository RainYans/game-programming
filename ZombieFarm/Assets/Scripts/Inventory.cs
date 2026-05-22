/// Harvested zombies the player owns, keyed by zombie/crop id. Behaviour lives in ItemStore;
/// this is its own type so it stays distinct from the seed stock (SeedInventory).
public class Inventory : ItemStore { }
