using UnityEngine;

/// Smoothly keeps the battle camera centered on the leader. Standalone (no Cinemachine) to keep
/// the Battle scene self-contained; the farm scene keeps its Cinemachine rig.
public class BattleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 velocity;

    public void SetTarget(Transform t) => target = t;

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}
