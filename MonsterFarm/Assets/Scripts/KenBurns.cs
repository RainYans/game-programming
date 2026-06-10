using UnityEngine;

/// Subtle, continuous "Ken Burns" drift — a slow zoom + pan on a RectTransform — to give a still
/// illustration a sense of life. Drop it on the story-book illustration (clip it with a RectMask2D
/// on the parent so the motion never spills past the page). Uses unscaled time. Composes with any
/// scaling a parent does (e.g. the page-turn animation), since it only touches this object.
public class KenBurns : MonoBehaviour
{
    [SerializeField] private float zoom = 0.06f;   // extra scale at the peak of the breath
    [SerializeField] private float panX = 12f;     // horizontal drift (local units)
    [SerializeField] private float panY = 9f;      // vertical drift
    [SerializeField] private float speed = 0.22f;

    private RectTransform rt;
    private Vector2 basePos;
    private Vector3 baseScale;
    private float seed;

    private void Awake()
    {
        rt = transform as RectTransform;
        basePos = rt != null ? rt.anchoredPosition : Vector2.zero;
        baseScale = transform.localScale;
        seed = Mathf.Repeat(transform.position.x * 0.13f, 6.28f); // a little phase variety
    }

    private void Update()
    {
        if (rt == null) return;
        float t = Time.unscaledTime * speed + seed;
        float s = 1f + zoom * 0.5f * (1f + Mathf.Sin(t));
        transform.localScale = baseScale * s;
        rt.anchoredPosition = basePos + new Vector2(Mathf.Sin(t * 0.8f) * panX, Mathf.Cos(t * 0.6f) * panY);
    }
}
