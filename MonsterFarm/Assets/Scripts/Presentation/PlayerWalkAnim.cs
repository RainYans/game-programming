using UnityEngine;

/// PRESENTATION-ONLY. Drives the farm avatar's walk/idle sprites from AvatarController.MoveInput.
/// Reads movement, never changes it. AvatarController already flips X for left, so `side`/`idleSide`
/// hold the RIGHT-facing art and are mirrored for left automatically.
[DisallowMultipleComponent]
public class PlayerWalkAnim : MonoBehaviour
{
    [Header("Walk cycles (played while moving)")]
    public Sprite[] down;
    public Sprite[] side;
    public Sprite[] up;
    public float fps = 8f;

    [Header("Idle cycles (played while standing; optional)")]
    public Sprite[] idleDown;
    public Sprite[] idleSide;
    public Sprite[] idleUp;
    public float idleFps = 4f;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private AvatarController ctrl;

    private Vector2 facing = Vector2.down;
    private float timer;
    private int idx;

    /// While true, this component stops driving the sprite so another system (e.g. LeaderCombat's
    /// attack swing) can take over the frames. Set back to false to resume walk/idle.
    public bool Suppressed { get; set; }

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (ctrl == null) ctrl = GetComponent<AvatarController>();
        if (ctrl == null) ctrl = GetComponentInParent<AvatarController>();
    }

    private void Update()
    {
        if (sr == null || Suppressed) return;
        Vector2 mv = ctrl != null ? ctrl.MoveInput : Vector2.zero;
        bool moving = mv.sqrMagnitude > 0.01f;
        if (moving) facing = mv;

        Sprite[] set = moving ? WalkSet(facing) : IdleSet(facing);
        if (set == null || set.Length == 0) set = WalkSet(facing);   // fallback to walk
        if (set == null || set.Length == 0) return;

        float f = moving ? fps : idleFps;
        float spf = f > 0f ? 1f / f : 0.125f;
        timer += Time.deltaTime;
        while (timer >= spf) { timer -= spf; idx++; }
        sr.sprite = set[idx % set.Length];
    }

    private Sprite[] WalkSet(Vector2 f)
    {
        if (Mathf.Abs(f.x) >= Mathf.Abs(f.y)) return side;
        return f.y > 0f ? up : down;
    }

    private Sprite[] IdleSet(Vector2 f)
    {
        if (Mathf.Abs(f.x) >= Mathf.Abs(f.y)) return idleSide;
        return f.y > 0f ? idleUp : idleDown;
    }
}
