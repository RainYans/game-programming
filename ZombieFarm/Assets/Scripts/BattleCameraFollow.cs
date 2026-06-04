using UnityEngine;

/// Smoothly keeps the battle camera centered on the leader, with a tiny screen-shake hook
/// used for hit / death feedback (call Shake from BattleManager / items). Standalone (no
/// Cinemachine) so the Battle scene stays self-contained.
public class BattleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 velocity;
    private float shakeTime;     // seconds remaining
    private float shakeTimeMax;  // for intensity ramp-down
    private float shakeMagnitude;

    public void SetTarget(Transform t) => target = t;

    /// Short positional shake with linear decay over `duration` seconds.
    public void Shake(float magnitude, float duration)
    {
        if (duration <= 0f || magnitude <= 0f) return;
        // Combine with any in-progress shake (use the stronger / longer one).
        if (duration > shakeTime) { shakeTime = duration; shakeTimeMax = duration; }
        if (magnitude > shakeMagnitude) shakeMagnitude = magnitude;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
        Vector3 pos = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            float k = shakeTimeMax > 0f ? Mathf.Clamp01(shakeTime / shakeTimeMax) : 0f;
            Vector2 jitter = Random.insideUnitCircle * (shakeMagnitude * k);
            pos += new Vector3(jitter.x, jitter.y, 0f);
            if (shakeTime <= 0f) shakeMagnitude = 0f;
        }
        transform.position = pos;
    }
}
