using UnityEngine;
using UnityEngine.InputSystem;

/// WASD movement for the farm avatar, driven through a Rigidbody2D so the avatar collides
/// with solid objects (buildings, map edges) instead of passing through them.
///
/// Reads the keyboard directly via the New Input System device API (matching the rest of the
/// project). The body is Dynamic with zero gravity and frozen rotation; we set its velocity
/// each physics step, which lets the physics engine resolve collisions and stop the avatar at
/// walls. Needs a Collider2D on the avatar (added by the Setup Avatar menu).
[RequireComponent(typeof(Rigidbody2D))]
public class AvatarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Tooltip("Vertical movement is scaled by this to match the isometric tile ratio (≈0.5 for " +
             "a 2:1 iso grid). Set 1 for straight XY.")]
    [SerializeField, Range(0.25f, 1f)] private float isoYScale = 0.5f;

    private Rigidbody2D body;
    private SpriteRenderer sprite;
    private Vector2 input;

    public Vector2 MoveInput => input;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null) body = gameObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) { input = Vector2.zero; return; }

        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        Vector2 dir = new Vector2(x, y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        input = dir;

        if (sprite != null && Mathf.Abs(x) > 0.01f) sprite.flipX = x < 0f;
    }

    private void FixedUpdate()
    {
        if (body != null)
            body.velocity = new Vector2(input.x, input.y * isoYScale) * moveSpeed;
    }

    // When frozen (e.g. a UI panel opens and this component is disabled), stop dead so the
    // Dynamic body doesn't keep drifting on its last velocity.
    private void OnDisable()
    {
        if (body != null) body.velocity = Vector2.zero;
    }
}
