using UnityEngine;

/// The buildings the player can enter from the farm.
/// WarCamp is the staging point for going out to reclaim cities (opens the campaign/city map).
public enum BuildingType { Shop, Lab, WarCamp, Home }

/// Passive marker on a scene building. The avatar opens it by walking up and pressing E
/// (handled by AvatarInteraction). Needs a trigger Collider2D for proximity detection.
[RequireComponent(typeof(Collider2D))]
public class Building : MonoBehaviour
{
    public BuildingType type;
}
