using System.Collections.Generic;

/// Pure C# (no UnityEngine) so it is unit-testable. Resolves a deterministic
/// front-vs-front auto-battle and returns the result plus an ordered event log
/// that BattlePlayer replays as animation.

public enum Team { Player, Enemy }

public class BattleUnit
{
    public int Id;
    public string Name;
    public Team Team;
    public int MaxHp;
    public int Hp;
    public int Attack;

    public bool IsAlive => Hp > 0;
}

public enum BattleEventType { Attack, Death, BattleEnd }

public struct BattleEvent
{
    public BattleEventType Type;
    public int AttackerId;
    public int TargetId;
    public int Damage;
    public int TargetHpAfter;
}

public class BattleResult
{
    public bool PlayerWon;
    public List<BattleEvent> Log = new List<BattleEvent>();
    public int PlayerSurvivors;
    public int EnemySurvivors;
}

public static class BattleSimulator
{
    // Safety cap so a misconfigured roster (0 attack) can never hang the game.
    private const int MaxRounds = 10000;

    public static BattleResult Simulate(IList<BattleUnit> playerTeam, IList<BattleUnit> enemyTeam)
    {
        var result = new BattleResult();

        ResetForBattle(playerTeam);
        ResetForBattle(enemyTeam);

        int pIndex = 0, eIndex = 0, rounds = 0;
        while (rounds++ < MaxRounds)
        {
            BattleUnit p = NextAlive(playerTeam, ref pIndex);
            BattleUnit e = NextAlive(enemyTeam, ref eIndex);
            if (p == null || e == null) break;

            // Simultaneous exchange: both deal damage this round before deaths resolve.
            int eHpAfter = System.Math.Max(0, e.Hp - p.Attack);
            int pHpAfter = System.Math.Max(0, p.Hp - e.Attack);
            e.Hp = eHpAfter;
            p.Hp = pHpAfter;

            result.Log.Add(MakeAttack(p.Id, e.Id, p.Attack, e.Hp));
            result.Log.Add(MakeAttack(e.Id, p.Id, e.Attack, p.Hp));
            if (!e.IsAlive) result.Log.Add(MakeDeath(e.Id));
            if (!p.IsAlive) result.Log.Add(MakeDeath(p.Id));
        }

        result.PlayerSurvivors = CountAlive(playerTeam);
        result.EnemySurvivors = CountAlive(enemyTeam);
        result.PlayerWon = result.PlayerSurvivors > 0;
        result.Log.Add(new BattleEvent { Type = BattleEventType.BattleEnd });
        return result;
    }

    private static void ResetForBattle(IList<BattleUnit> team)
    {
        foreach (BattleUnit u in team) u.Hp = u.MaxHp;
    }

    private static BattleUnit NextAlive(IList<BattleUnit> team, ref int index)
    {
        while (index < team.Count && !team[index].IsAlive) index++;
        return index < team.Count ? team[index] : null;
    }

    private static int CountAlive(IList<BattleUnit> team)
    {
        int n = 0;
        foreach (BattleUnit u in team) if (u.IsAlive) n++;
        return n;
    }

    private static BattleEvent MakeAttack(int attackerId, int targetId, int damage, int targetHpAfter) =>
        new BattleEvent
        {
            Type = BattleEventType.Attack,
            AttackerId = attackerId,
            TargetId = targetId,
            Damage = damage,
            TargetHpAfter = targetHpAfter
        };

    private static BattleEvent MakeDeath(int targetId) =>
        new BattleEvent { Type = BattleEventType.Death, TargetId = targetId };
}
