using UnityEngine;

/// PRESENTATION-ONLY. Sets SpriteRenderer.sortingOrder from world Y so lower objects draw in
/// front (top-down depth). Base keeps everything above the ground tilemaps. No gameplay coupling.
[DisallowMultipleComponent]
[ExecuteAlways]
public class DecorYSort : MonoBehaviour
{
    public int baseOrder = 500;
    public float perUnit = 10f;
    [Tooltip("Static objects compute once on enable; moving ones (animals) need this true.")]
    public bool everyFrame = false;
    [Tooltip("Sort by the sprite's visual bottom edge (ground-contact). Auto-handles any pivot.")]
    public bool useSpriteBottom = true;
    [Tooltip("Extra Y added to the sort point when not using sprite bottom.")]
    public float yOffset = 0f;

    private SpriteRenderer sr;

    private void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        Apply();
    }

    private void LateUpdate()
    {
        if (everyFrame) Apply();
    }

    private void Apply()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        float y = useSpriteBottom ? sr.bounds.min.y : transform.position.y + yOffset;
        sr.sortingOrder = baseOrder - Mathf.RoundToInt(y * perUnit);
    }
}
