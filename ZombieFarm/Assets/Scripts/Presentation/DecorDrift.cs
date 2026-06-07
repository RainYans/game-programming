using UnityEngine;

/// PRESENTATION-ONLY. Drifts steadily in one direction and loops back after a distance.
/// For clouds / slow ambient movers. No gameplay coupling.
[DisallowMultipleComponent]
public class DecorDrift : MonoBehaviour
{
    public Vector2 velocity = new Vector2(0.4f, 0f);
    public float wrapDistance = 30f;

    private Vector3 start;

    private void Start() { start = transform.position; }

    private void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        Vector3 d = transform.position - start;
        if (Vector3.Dot(d, (Vector3)velocity) > 0f && d.magnitude > wrapDistance)
            transform.position = start;
    }
}
