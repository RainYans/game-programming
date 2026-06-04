using System.Collections.Generic;

/// Carries data across the farm <-> battle scene boundary. Static so it survives scene loads
/// (no GameObject to destroy). The farm fills the deployment + mission, loads the Battle scene;
/// the battle writes back the result; the farm reads the result on return and applies
/// permadeath + reward, then clears it.
public static class BattleHandoff
{
    public class DeployedUnit
    {
        public string uid;       // the owning ZombieUnit.uid (for permadeath)
        public ZombieData data;  // resolved strain stats
    }

    // --- Request: farm -> battle ---
    public static List<DeployedUnit> Squad;
    public static MissionData Mission;

    public static bool HasDeployment => Squad != null && Squad.Count > 0;

    // --- Result: battle -> farm ---
    public static bool HasResult;
    public static bool Won;
    public static int Reward;
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
    }

    public static void ClearResult()
    {
        HasResult = false;
        Won = false;
        Reward = 0;
        CasualtyUids = new List<string>();
    }
}
