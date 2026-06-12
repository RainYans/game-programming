using System;
using TMPro;
using UnityEngine;

/// One harvested zombie idly wandering the farm — a visual stand-in for a single owned
/// ZombieUnit. Walks to target points supplied by a provider (so the spawner decides the
/// region shape, e.g. an isometric diamond), pauses, repeats. Shows a floating label whose
/// text is the strain name (identity) and whose color flags hunger (Full vs Hungry, the latter
/// being the stronger combat state). Hunger drifts over time, so the label is polled, not
/// event-driven. Holds the unit's uid (not the object) so it survives a save reload.
public class FarmRoamer : MonoBehaviour
{
    private float moveSpeed = 1.2f;
    private Func<Vector3> nextTarget;

    private SpriteRenderer sprite;
    private Rigidbody2D body;
    private Vector3 target;
    private float pauseTimer;

    private string uid;
    private Inventory inventory;
    private Sprite[] animFrames;
    private float animFps = 5f;
    private int animIdx;
    private float animTimer;
    private TextMeshPro label;
    private string displayName = "";
    private float hungerPollTimer;
    private HungerState lastState = HungerState.Full;

    private static readonly Color FullColor = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color HungryColor = new Color(1f, 0.5f, 0.2f);
    private const float HungerPollInterval = 0.5f;

    private void Awake()
    {
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        SetupCollision();
    }

    /// Small dynamic body so roamers don't clip through buildings/each other/the avatar.
    private void SetupCollision()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null) body = gameObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.drag = 10f;
        var col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
    }

    /// Configure movement + identity. `inventory` is used to re-resolve this unit's hunger by
    /// uid each poll (robust across save reloads that rebuild the roster).
    public void Init(float speed, Func<Vector3> targetProvider, string unitUid,
        string strainDisplayName, Inventory inv)
    {
        moveSpeed = speed;
        nextTarget = targetProvider;
        uid = unitUid;
        displayName = strainDisplayName ?? "";
        inventory = inv;
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        EnsureLabel();
        PickNewTarget();
        RefreshHunger(force: true);
    }

    /// Assign the looping walk/idle frames (the harvested monster's animation).
    public void SetFrames(Sprite[] frames, float fps)
    {
        animFrames = frames;
        animFps = fps > 0f ? fps : 5f;
        if (sprite != null && frames != null && frames.Length > 0) sprite.sprite = frames[0];
    }

    private void Update()
    {
        Wander();
        Animate();
        PollHunger();
    }

    private void Animate()
    {
        if (sprite == null || animFrames == null || animFrames.Length < 2) return;
        animTimer += Time.deltaTime;
        float spf = 1f / animFps;
        while (animTimer >= spf) { animTimer -= spf; animIdx++; }
        sprite.sprite = animFrames[animIdx % animFrames.Length];
    }

    private void Wander()
    {
        // Velocity-driven movement: reset each frame, then set it while actually walking. (Was
        // MovePosition with Time.deltaTime in Update, which made the wander speed framerate-dependent
        // — units barely crawled at high FPS.)
        if (body != null) body.velocity = Vector2.zero;

        if (nextTarget == null) return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        Vector3 to = target - transform.position;
        if (to.sqrMagnitude < 0.02f)
        {
            pauseTimer = UnityEngine.Random.Range(0.5f, 2.0f);
            PickNewTarget();
            return;
        }

        Vector3 dir = to.normalized;
        if (body != null)
            body.velocity = (Vector2)(dir * moveSpeed);
        else
            transform.position += dir * (moveSpeed * Time.deltaTime);
        if (sprite != null && Mathf.Abs(dir.x) > 0.01f) sprite.flipX = dir.x < 0f;
    }

    private void PollHunger()
    {
        hungerPollTimer -= Time.deltaTime;
        if (hungerPollTimer > 0f) return;
        hungerPollTimer = HungerPollInterval;
        RefreshHunger(force: false);
    }

    private void RefreshHunger(bool force)
    {
        if (inventory == null || label == null) return;
        ZombieUnit unit = inventory.FindUnit(uid);
        HungerState state = unit != null ? inventory.StateOf(unit) : HungerState.Full;
        if (!force && state == lastState) return;
        lastState = state;

        bool hungry = state == HungerState.Hungry;
        label.text = hungry ? $"{displayName}!" : displayName;
        label.color = hungry ? HungryColor : FullColor;
    }

    private void EnsureLabel()
    {
        if (label != null) return;

        var go = new GameObject("Label", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(transform, false);
        // Counter the roamer's downscale so the text stays a sane size, and float above the head.
        float inv = transform.localScale.x > 0.001f ? 1f / transform.localScale.x : 1f;
        rt.localScale = Vector3.one * inv;
        rt.localPosition = new Vector3(0f, 0.9f, 0f);
        rt.sizeDelta = new Vector2(4f, 1f);

        label = go.AddComponent<TextMeshPro>();
        if (TMP_Settings.defaultFontAsset != null) label.font = TMP_Settings.defaultFontAsset;
        label.text = displayName;
        label.fontSize = 2.2f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = FullColor;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerID = sprite != null ? sprite.sortingLayerID : mr.sortingLayerID;
            mr.sortingOrder = (sprite != null ? sprite.sortingOrder : 0) + 1;
        }
    }

    private void PickNewTarget()
    {
        if (nextTarget != null) target = nextTarget();
    }
}
