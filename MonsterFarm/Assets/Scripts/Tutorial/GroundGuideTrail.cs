using System.Collections.Generic;
using UnityEngine;

/// A trail of glowing chevron arrows laid along the ground from the player toward a target — a
/// building, a farm plot, a ring. A controller calls Point(target) / Point(worldPoint) / Hide().
/// The arrows are world-space SpriteRenderers spaced evenly between the avatar and the goal, all
/// pointing at it, with a highlight that FLOWS from the player outward so the eye is led "this way".
/// The chevron sprite is generated in code, so there's no art dependency. Far more legible than a
/// single floating screen arrow — it literally paints a path on the ground.
public class GroundGuideTrail : MonoBehaviour
{
    [Header("Refs (auto-resolved if empty)")]
    [SerializeField] private Transform from;        // start of the trail; defaults to the avatar

    [Header("Layout")]
    [SerializeField] private float spacing = 1.1f;  // world units between arrows
    [SerializeField] private int maxArrows = 8;     // cap regardless of distance
    [SerializeField] private float startGap = 0.9f; // skip this much right in front of the player
    [SerializeField] private float endGap = 0.7f;   // stop this far short of the target
    [SerializeField] private float yLift = 0f;      // nudge the whole trail up/down off the ground

    [Header("Look")]
    [SerializeField] private Color color = new Color(1f, 0.86f, 0.28f, 0.95f);
    [SerializeField] private float arrowScale = 0.55f;
    [SerializeField] private int sortingOrder = 50;
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private float flowSpeed = 2.4f; // brightness wave speed along the trail

    private Transform target;       // a (possibly moving) transform to point at
    private Vector3 targetPoint;    // a fixed world point to point at
    private bool hasTarget;
    private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
    private static Sprite chevronSprite;

    private void Awake()
    {
        if (from == null) { var ac = FindFirstObjectByType<AvatarController>(); if (ac != null) from = ac.transform; }
        Hide();
    }

    /// Point the trail at a (possibly moving) transform. Pass null to hide.
    public void Point(Transform t)
    {
        target = t;
        hasTarget = t != null;
        if (!hasTarget) Hide();
    }

    /// Point the trail at a fixed world position.
    public void Point(Vector3 worldPoint)
    {
        target = null;
        targetPoint = worldPoint;
        hasTarget = true;
    }

    public void Hide()
    {
        hasTarget = false;
        target = null;
        foreach (SpriteRenderer sr in pool) if (sr != null) sr.enabled = false;
    }

    private void LateUpdate()
    {
        if (!hasTarget) return;

        Vector3 start = from != null ? from.position : transform.position;
        Vector3 goal = target != null ? target.position : targetPoint;
        start.z = 0f; goal.z = 0f;

        Vector3 delta = goal - start;
        float dist = delta.magnitude;
        Vector3 dir = dist > 0.0001f ? delta / dist : Vector3.right;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float usable = dist - startGap - endGap;
        int count = usable <= 0f ? 0 : Mathf.Min(maxArrows, Mathf.FloorToInt(usable / spacing) + 1);

        // Disable any pooled arrows beyond what we need this frame.
        for (int i = count; i < pool.Count; i++) if (pool[i] != null) pool[i].enabled = false;
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            SpriteRenderer sr = GetArrow(i);
            float along = startGap + i * spacing;
            Vector3 pos = start + dir * along;
            pos.y += yLift;
            pos.z = 0f;
            sr.transform.position = pos;
            sr.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            sr.transform.localScale = Vector3.one * arrowScale;

            // A brightness wave travels from the player outward, so the trail "points" over time.
            float phase = Time.unscaledTime * flowSpeed - i * 0.6f;
            float k = 0.5f + 0.5f * (Mathf.Sin(phase) * 0.5f + 0.5f);
            sr.color = new Color(color.r, color.g, color.b, color.a * k);
        }
    }

    private SpriteRenderer GetArrow(int i)
    {
        while (pool.Count <= i)
        {
            var go = new GameObject($"GuideArrow_{pool.Count}");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Chevron();
            sr.sortingOrder = sortingOrder;
            if (!string.IsNullOrEmpty(sortingLayer)) sr.sortingLayerName = sortingLayer;
            pool.Add(sr);
        }
        SpriteRenderer r = pool[i];
        r.enabled = true;
        return r;
    }

    // A right-pointing ">" chevron, soft-edged so it glows. Pointing +X before rotation.
    private static Sprite Chevron()
    {
        if (chevronSprite != null) return chevronSprite;
        const int s = 64;
        var tex = new Texture2D(s, s) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        var px = new Color[s * s];
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float u = x / (float)(s - 1);             // 0..1 left -> right
                float v = (y / (float)(s - 1)) * 2f - 1f; // -1..1 bottom -> top
                float edge = 1f - Mathf.Abs(v);           // stroke x-position: tip (1) at centre row
                float d = Mathf.Abs(u - edge);
                float a = Mathf.Clamp01(1f - d / 0.16f);  // thickness of the stroke
                a *= Mathf.Clamp01(1f - Mathf.Abs(v));    // fade toward top/bottom for an arrowhead feel
                px[y * s + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        chevronSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
        return chevronSprite;
    }
}
