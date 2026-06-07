using UnityEngine;
using UnityEngine.InputSystem;

/// Lets the avatar plant/harvest the farm cell it is standing on by pressing the interact
/// key (E), and highlights that cell while it is on farmable soil. Reuses the input-agnostic
/// FarmActions.Interact — the same entry point the old mouse click used.
///
/// Lives on the Avatar. When avatar interaction is active, the mouse-based TileInteraction
/// should be disabled so the two don't fight over the highlight tilemap.
public class AvatarInteraction : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private FarmActions farmActions;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AvatarController avatar;
    [SerializeField] private SpriteRenderer avatarSprite;

    [Tooltip("Fine-tune which cell is highlighted. The sample starts at the avatar's FEET " +
             "(sprite bottom-center); nudge Y up (+) if the highlight sits a tile too low.")]
    [SerializeField] private Vector2 interactOffset = new Vector2(0f, 0.1f);

    [Tooltip("How close the avatar's center must be to a building's center for E to open it.")]
    [SerializeField] private float buildingReach = 1.2f;

    [SerializeField] private Key interactKey = Key.E;

    private Vector3Int currentCell;
    private bool hasHighlight;

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (farmActions == null) farmActions = FindFirstObjectByType<FarmActions>();
        if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
        if (avatar == null) avatar = GetComponent<AvatarController>();
        if (avatarSprite == null) avatarSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (gridManager == null) return;

        // Sample the cell under the avatar. transform.position is a stable reference fixed to the
        // character, so a single interactOffset always points at the same body part (the feet);
        // tune interactOffset.Y in the Inspector to line the highlight up with where it stands.
        Vector3 sample = transform.position + (Vector3)interactOffset;
        sample.z = 0f;
        Vector3Int cell = gridManager.WorldToCell(sample);
        bool onFarm = gridManager.IsFarmCell(cell);

        UpdateHighlight(cell, onFarm);

        Keyboard kb = Keyboard.current;
        if (kb == null || !kb[interactKey].wasPressedThisFrame) return;

        // E is context-sensitive: open a nearby building first, else plant/harvest this cell.
        if (TryOpenNearbyBuilding()) return;
        if (onFarm && farmActions != null) farmActions.Interact(cell);
    }

    /// Open the NEAREST building whose center is within buildingReach. Returns true if opened.
    /// Center-distance based (not collider overlap) so reach is predictable and the closest
    /// building always wins — no accidentally opening a different building nearby.
    private bool TryOpenNearbyBuilding()
    {
        if (uiManager == null) return false;

        Building[] buildings = FindObjectsByType<Building>(FindObjectsSortMode.None);
        Building nearest = null;
        float bestSqr = buildingReach * buildingReach;
        Vector2 here = transform.position;

        foreach (Building b in buildings)
        {
            float sqr = ((Vector2)b.transform.position - here).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                nearest = b;
            }
        }

        if (nearest == null) return false;
        uiManager.OpenBuilding(nearest.type);
        return true;
    }

    private void UpdateHighlight(Vector3Int cell, bool onFarm)
    {
        // Clear the old highlight if we moved off the cell or off farmable soil.
        if (hasHighlight && (cell != currentCell || !onFarm))
        {
            gridManager.ClearHighlight(currentCell);
            hasHighlight = false;
        }
        // Highlight the current cell when standing on farmable soil.
        if (onFarm && !hasHighlight)
        {
            gridManager.SetHighlight(cell);
            currentCell = cell;
            hasHighlight = true;
        }
    }

    private void OnDisable()
    {
        if (gridManager != null && hasHighlight) gridManager.ClearHighlight(currentCell);
        hasHighlight = false;
    }
}
