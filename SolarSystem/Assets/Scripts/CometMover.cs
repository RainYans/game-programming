using UnityEngine;

/// <summary>
/// Bonus: attach to a Comet prefab to make it orbit the sun in a wide ellipse.
/// Add a TrailRenderer component to the same GameObject for the comet tail.
/// </summary>
public class CometMover : MonoBehaviour
{
    [Tooltip("The Sun Transform to orbit around")]
    public Transform sun;

    [Tooltip("Orbit radius on X axis (wide ellipse)")]
    public float radiusX = 28f;

    [Tooltip("Orbit radius on Z axis (narrow ellipse)")]
    public float radiusZ = 12f;

    [Tooltip("Orbit speed in degrees per second")]
    public float orbitSpeed = 20f;

    private float angle = 0f;

    void Start()
    {
        angle = Random.Range(0f, 360f);

        // Set position immediately so trail doesn't capture the initial jump
        float rad = angle * Mathf.Deg2Rad;
        Vector3 origin = sun != null ? sun.position : Vector3.zero;
        transform.position = origin + new Vector3(
            Mathf.Cos(rad) * radiusX,
            0f,
            Mathf.Sin(rad) * radiusZ
        );

        // Disable trail for first frame to avoid capturing the initial teleport
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) StartCoroutine(EnableTrailAfterFrame(trail));
    }

    System.Collections.IEnumerator EnableTrailAfterFrame(TrailRenderer trail)
    {
        trail.enabled = false;
        yield return null;
        yield return null;
        trail.enabled = true;
    }

    void Update()
    {
        angle += orbitSpeed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;
        Vector3 origin = sun != null ? sun.position : Vector3.zero;

        transform.position = origin + new Vector3(
            Mathf.Cos(rad) * radiusX,
            0f,
            Mathf.Sin(rad) * radiusZ
        );
    }
}
