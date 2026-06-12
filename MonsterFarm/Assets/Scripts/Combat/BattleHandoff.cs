using System.Collections.Generic;

/// Carries data across the farm <-> battle scene boundary. Static so it survives scene loads
/// (no GameObject to destroy). The farm fills the deployment + mission, loads the Battle scene;
/// the battle writes back the result; the farm reads the result on return and applies
/// permadeath + reward, then clears it.
public static class BattleHandoff
{
    public class DeployedUnit
    {
        public string uid;                       // the owning ZombieUnit.uid (for permadeath)
        public ZombieData data;                  // resolved strain stats
        public float damageMultiplier = 1f;      // 1 = Full; >1 if deployed Hungry (deals more)
        public float damageTakenMultiplier = 1f; // 1 = Full; >1 if deployed Hungry (takes more)
        public float upgradeMultiplier = 1f;     // Lab upgrade: scales maxHp + attack (1 = none)
    }

    // --- Request: farm -> battle ---
    public static List<DeployedUnit> Squad;
    public static MissionData Mission;
    public static GameConfig Config;        // for combat tuning in the Battle scene (may be null in tests)
    public static int OnionsCarried;        // Rotten Onions the squad brought in (from ItemInventory)
    public static int FreezesCarried;       // Freeze Canisters the squad brought in

    public static bool HasDeployment => Squad != null && Squad.Count > 0;

    // --- Result: battle -> farm ---
    public static bool HasResult;
    public static bool Won;
    public static int Reward;
    public static int OnionsUsed;           // Rotten Onions thrown this raid (removed on return)
    public static int FreezesUsed;          // Freeze Canisters thrown this raid (removed on return)
    public static List<string> CasualtyUids = new List<string>();

    public static void SetDeployment(List<DeployedUnit> squad, MissionData mission)
    {
        Squad = squad;
        Mission = mission;
    }

    public static void SetResult(bool won, int reward, List<string> casualties)
    {
        HasResult = true;
        Won = won;
        Reward = reward;
        CasualtyUids = casualties ?? new List<string>();
    }

    public static void ClearDeployment()
    {
        Squad = null;
        Mission = null;
        Config = null;
        OnionsCarried = 0;
        FreezesCarried = 0;
    }

    public static void ClearResult()
    {
        HasResult = false;
        Won = false;
        Reward = 0;
        OnionsUsed = 0;
        FreezesUsed = 0;
        CasualtyUids = new List<string>();
    }
}
