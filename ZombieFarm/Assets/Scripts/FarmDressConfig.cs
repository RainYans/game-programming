using System.Collections.Generic;
using UnityEngine;

/// Authoring data for the "Dress Farm Ground" editor tool. Edit this asset in the Inspector
/// (native drag-drop lists), then run Tools > Zombie Farm > Dress Farm Ground to scatter.
[CreateAssetMenu(fileName = "FarmDressConfig", menuName = "ZombieFarm/Farm Dress Config")]
public class FarmDressConfig : ScriptableObject
{
    [Header("Props — transparent iso decorations (trees, bushes, fences)")]
    [Tooltip("Candidates live in Assets/Art/Props/. Drag the ones you want scattered here.")]
    public List<Sprite> props = new List<Sprite>();
    [Range(0f, 0.4f)] public float propDensity = 0.08f;  // fraction of open cells that get a prop
    [Range(0.3f, 2f)] public float propScale = 1f;
    public int seed = 12345;                             // change to re-roll the layout

    [Header("Optional — vary the ground tiles (advanced)")]
    [Tooltip("Mix these tile sprites into plain ground for variety. Use ground-block sprites that " +
             "match the current tile's size/pivot, or they may misalign. Off by default.")]
    public bool varyGround = false;
    public List<Sprite> groundVariants = new List<Sprite>();
    [Range(0f, 1f)] public float groundVarietyChance = 0.25f;
}
