using System.Collections.Generic;
using UnityEngine;

/// One playable city: a sequence of combat stages to clear, and the reward for reclaiming it.
/// A city is cleared stage-by-stage with a prep phase between stages (see combat.md).
[CreateAssetMenu(fileName = "MissionData", menuName = "ZombieFarm/Mission Data")]
public class MissionData : ScriptableObject
{
    [Tooltip("Stable id for save/progression (which cities are cleared). Distinct from the " +
             "display name. e.g. \"city1\".")]
    public string id = "city1";

    public string cityName = "Fallen City 1";

    [Tooltip("One-line difficulty / enemy hint shown on the city-map node, e.g. " +
             "\"Wild grunts & runners — gentle\".")]
    [TextArea] public string mapHint = "";

    [Tooltip("The stages fought in order. If empty, the flat 'enemies' list below is used as a " +
             "single stage (back-compat).")]
    public List<Stage> stages = new List<Stage>();

    [Tooltip("Legacy single-encounter enemy list; used as one stage when 'stages' is empty.")]
    public List<EnemySpawn> enemies = new List<EnemySpawn>();

    [Min(0)] public int rewardAmount = 50;

    [System.Serializable]
    public class Stage
    {
        public List<EnemySpawn> enemies = new List<EnemySpawn>();
    }

    [System.Serializable]
    public struct EnemySpawn
    {
        public ZombieData zombie;
        [Min(1)] public int count;
    }
}
