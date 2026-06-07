using UnityEngine;

/// PRESENTATION-ONLY. Gentle wind sway — small sine rotation around the base. For trees,
/// bushes, tall plants. No gameplay coupling.
[DisallowMultipleComponent]
public class DecorSway : MonoBehaviour
{
    public float angle = 2.5f;
    public float speed = 1.2f;
    public bool randomPhase = true;
    [Tooltip("Pivot offset (local units) the sway rotates around; negative Y = pivot at base.")]
    public Vector3 pivotOffset = new Vector3(0f, -0.5f, 0f);

    private float phase;
    private Vector3 basePos;
    private Quaternion baseRot;

    private void Start()
    {
        basePos = transform.localPosition;
        baseRot = transform.localRotation;
        if (randomPhase) phase = Random.value * 6.28318f;
    }

    private void Update()
    {
        float a = Mathf.Sin(Time.time * speed + phase) * angle;
        Quaternion rot = Quaternion.Euler(0f, 0f, a);
        // rotate around a pivot below the sprite so the base stays planted
        Vector3 piv = pivotOffset;
        transform.localRotation = baseRot * rot;
        transform.localPosition = basePos + (baseRot * (rot * (-piv) + piv));
    }
}
