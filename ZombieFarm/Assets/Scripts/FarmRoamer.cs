using System;
using UnityEngine;

/// A harvested zombie idly wandering the farm. Walks to target points supplied by a provider
/// (so the spawner decides the region shape — e.g. an isometric diamond), pauses, repeats.
/// Visual only for now — it represents one unit of the player's standing army (a count in
/// Inventory). When the hunger system lands, this is where per-zombie state will live.
public class FarmRoamer : MonoBehaviour
{
    private float moveSpeed = 1.2f;
    private Func<Vector3> nextTarget;

    private SpriteRenderer sprite;
    private Vector3 target;
    private float pauseTimer;

    private void Awake()
    {
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
    }

    /// Configure speed and a target-point provider, then start moving.
    /// The provider returns a fresh random world point inside the wander region each call.
    public void Init(float speed, Func<Vector3> targetProvider)
    {
        moveSpeed = speed;
        nextTarget = targetProvider;
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        PickNewTarget();
    }

    private void Update()
    {
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
        transform.position += dir * (moveSpeed * Time.deltaTime);
        if (sprite != null && Mathf.Abs(dir.x) > 0.01f) sprite.flipX = dir.x < 0f;
    }

    private void PickNewTarget()
    {
        if (nextTarget != null) target = nextTarget();
    }
}
