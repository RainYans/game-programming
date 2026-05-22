/// Seeds the player can plant, keyed by crop id. The shop adds to it; planting consumes
/// from it. Separate type from Inventory so the two stores never get confused at wiring time.
public class SeedInventory : ItemStore { }
