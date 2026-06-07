using UnityEngine;

/// PRESENTATION-ONLY. Vertical sine bob on local position. For floating/flying critters,
/// hovering props. No gameplay coupling.
[DisallowMultipleComponent]
public class DecorBob : MonoBehaviour
{
    public float amplitude = 0.12f;
    public float speed = 3f;
    public bool randomPhase = true;

    private Vector3 baseLocal;
    private float phase;

    private void Start()
    {
        baseLocal = transform.localPosition;
        if (randomPhase) phase = Random.value * 6.28318f;
    }

    private void Update()
    {
        Vector3 p = baseLocal;
        p.y += Mathf.Sin(Time.time * speed + phase) * amplitude;
        transform.localPosition = p;
    }
}
