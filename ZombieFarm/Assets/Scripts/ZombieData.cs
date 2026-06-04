using UnityEngine;

/// Combat profile for one engineered-zombie strain. `id` matches the CropData seed that
/// yields it, so harvested units map back to battle stats. Passives are simple, auto-triggered
/// flags read by the combat resolver — no per-strain code needed in the base design.
[CreateAssetMenu(fileName = "ZombieData", menuName = "ZombieFarm/Zombie Data")]
public class ZombieData : ScriptableObject
{
    public string id = "basic";
    public string displayName = "Basic Zombie";

    [Tooltip("Short role label for the deploy/inspect UI, e.g. \"Tank\" or \"Skirmisher\".")]
    public string role = "";

    [Header("Stats")]
    [Min(1)] public int maxHp = 10;
    [Min(0)] public int attack = 3;
    [Min(0f)] public float moveSpeed = 2.5f;
    public AttackRange range = AttackRange.Melee;

    [Header("Ability")]
    public Passive passive = Passive.None;

    [Header("Availability")]
    [Tooltip("True for the three starting strains (Brute / Mauler / Runner); the rest unlock via tasks.")]
    public bool unlockedAtStart = true;

    [Header("Placeholder")]
    [Tooltip("Placeholder tint for the battle/roamer sprite until real art lands.")]
    public Color color = new Color(0.5f, 0.8f, 0.4f);
}

public enum AttackRange
{
    Melee,
    Ranged,
}

/// One auto-triggered passive per strain. The combat resolver branches on this flag rather
/// than each strain carrying bespoke code (see design/zombies.md).
public enum Passive
{
    None,
    ThickHide,    // Brute: flat damage reduction
    Bloodlust,    // Mauler: consecutive hits on the same target ramp damage
    Evasion,      // Runner: high dodge / first strike on engage
    Corrosion,    // Spitter: hits can lower the target's defense
    Aura,         // Shaman: slowly heals / buffs nearby allies
    SelfDetonate, // Bomber: area damage on death
}
