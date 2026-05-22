using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeployController : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Wallet wallet;
    [SerializeField] private MissionData mission;
    [SerializeField] private List<ZombieData> zombieRoster = new List<ZombieData>();
    [SerializeField] private int maxDeploy = 3;
    [SerializeField] private TMP_Text resultLabel;

    public event System.Action<BattleResult, List<BattleUnit>, List<BattleUnit>, string> BattleResolved;

    private void Awake()
    {
        if (inventory == null) inventory = Object.FindFirstObjectByType<Inventory>();
        if (wallet == null) wallet = Object.FindFirstObjectByType<Wallet>();
    }

    public void Deploy()
    {
        if (resultLabel != null) resultLabel.text = string.Empty;

        List<BattleUnit> player = BuildPlayerTeam();
        if (player.Count == 0)
        {
            if (resultLabel != null) resultLabel.text = "No zombies to deploy!";
            return;
        }
        List<BattleUnit> enemy = BuildEnemyTeam();

        BattleResult result = BattleSimulator.Simulate(player, enemy);

        int reward = result.PlayerWon ? mission.rewardAmount : 0;
        if (reward > 0) wallet.Add(reward);

        string message = result.PlayerWon
            ? $"WIN vs {mission.cityName}! +{reward} resources (total {wallet.Resources})"
            : $"LOST vs {mission.cityName}...";

        BattleResolved?.Invoke(result, player, enemy, message);
    }

    private List<BattleUnit> BuildPlayerTeam()
    {
        var team = new List<BattleUnit>();
        int id = 0;
        foreach (ZombieData z in zombieRoster)
        {
            if (z == null || team.Count >= maxDeploy) continue;
            int take = Mathf.Min(inventory.Get(z.id), maxDeploy - team.Count);
            for (int i = 0; i < take; i++)
                team.Add(NewUnit(ref id, z, Team.Player));
            if (take > 0) inventory.TryRemove(z.id, take);
        }
        return team;
    }

    private List<BattleUnit> BuildEnemyTeam()
    {
        var team = new List<BattleUnit>();
        int id = 1000;
        foreach (MissionData.EnemySpawn spawn in mission.enemies)
        {
            if (spawn.zombie == null) continue;
            for (int i = 0; i < spawn.count; i++)
                team.Add(NewUnit(ref id, spawn.zombie, Team.Enemy));
        }
        return team;
    }

    private BattleUnit NewUnit(ref int id, ZombieData z, Team team) => new BattleUnit
    {
        Id = id++,
        Name = z.displayName,
        Team = team,
        MaxHp = z.maxHp,
        Hp = z.maxHp,
        Attack = z.attack
    };
}
