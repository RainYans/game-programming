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

    [Tooltip("How big the 'press E' detection circle is around this building. Bigger = you can " +
             "open it from farther away. Each building has its own value, so a big barn can have a " +
             "wider reach than a small signpost. Shown as a yellow circle in the Scene view when " +
             "the building is selected — drag this value and watch the circle grow/shrink.")]
    public float interactionRadius = 1.6f;

    [Tooltip("Optional: moves the CENTER of the detection circle relative to the building's pivot, " +
             "without moving the art. Usually leave at 0 — the avatar walks at the base, so a circle " +
             "centred on the pivot is natural. Only nudge this if the circle sits off the building.")]
    public Vector2 interactionOffset = Vector2.zero;

    /// World centre of the detection circle = pivot + interactionOffset.
    public Vector3 InteractionPoint => transform.position + (Vector3)interactionOffset;

    private void OnDrawGizmosSelected()
    {
        DrawRangeCircle(new Color(1f, 0.85f, 0.2f, 0.95f));
    }

    // A flat circle in the XY plane (the actual 2D detection range), not a 3D sphere — reads
    // cleanly from the top-down Scene camera so you can size it by eye against the building art.
    private void DrawRangeCircle(Color c)
    {
        Gizmos.color = c;
        Vector3 center = InteractionPoint;
        float r = Mathf.Max(0f, interactionRadius);
        const int segments = 48;
        Vector3 prev = center + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
