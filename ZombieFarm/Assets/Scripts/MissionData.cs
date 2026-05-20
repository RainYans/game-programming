using System.Collections.Generic;
using UnityEngine;

/// One playable city mission: the wild-zombie roster to fight and the reward for winning.
[CreateAssetMenu(fileName = "MissionData", menuName = "ZombieFarm/Mission Data")]
public class MissionData : ScriptableObject
{
    public string cityName = "Fallen City 1";
    public List<EnemySpawn> enemies = new List<EnemySpawn>();
    [Min(0)] public int rewardAmount = 50;

    [System.Serializable]
    public struct EnemySpawn
    {
        public ZombieData zombie;
        [Min(1)] public int count;
    }
}
