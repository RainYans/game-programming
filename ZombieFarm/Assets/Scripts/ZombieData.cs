using UnityEngine;

/// Combat stats for one engineered-zombie strain. `id` matches the CropData seed that
/// yields it, so harvested inventory items map to battle stats.
[CreateAssetMenu(fileName = "ZombieData", menuName = "ZombieFarm/Zombie Data")]
public class ZombieData : ScriptableObject
{
    public string id = "basic";
    public string displayName = "Basic Zombie";

    [Min(1)] public int maxHp = 10;
    [Min(0)] public int attack = 3;

    [Tooltip("Placeholder tint for the battle sprite")]
    public Color color = new Color(0.5f, 0.8f, 0.4f);
}
