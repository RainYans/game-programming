using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Tiny top-right minimap: a dot per BattleArea (laid out by each area's world position),
/// links between connected areas, and a live marker for the squad leader. Dots are colour-coded
/// (cleared / fighting / open / locked). Self-builds its children under this RectTransform.
public class BattleMinimap : MonoBehaviour
{
    [SerializeField] private BattleManager manager;
    [SerializeField] private RectTransform area;   // the map drawing area (defaults to this RT)
    [SerializeField] private float padding = 18f;

    private static readonly Color Cleared = new Color(0.40f, 0.72f, 0.42f);
    private static readonly Color Fighting = new Color(0.92f, 0.74f, 0.28f);
    private static readonly Color Open = new Color(0.85f, 0.80f, 0.60f);
    private static readonly Color Locked = new Color(0.40f, 0.38f, 0.34f);
    private static readonly Color LinkCol = new Color(0.55f, 0.45f, 0.30f, 0.7f);
    private static readonly Color PlayerCol = new Color(1f, 0.95f, 0.30f);

    private readonly List<(BattleArea a, Image dot)> dots = new List<(BattleArea, Image)>();
    private Image playerDot;
    private Bounds worldBounds;
    private bool built;

    private void Start()
    {
        if (manager == null) manager = FindFirstObjectByType<BattleManager>();
        if (area == null) area = transform as RectTransform;
        Build();
    }

    private void Build()
    {
        if (manager == null || manager.Areas == null || manager.Areas.Count == 0) return;

        // world bounds of the areas
        bool any = false;
        foreach (BattleArea a in manager.Areas)
        {
            if (a == null) continue;
            Vector3 p = a.transform.position;
            if (!any) { worldBounds = new Bounds(p, Vector3.zero); any = true; }
            else worldBounds.Encapsulate(p);
        }
        if (!any) return;
        worldBounds.Expand(4f);

        // links first (behind dots)
        var byId = new Dictionary<int, BattleArea>();
        foreach (BattleArea a in manager.Areas) if (a != null) byId[a.areaId] = a;
        foreach (BattleArea a in manager.Areas)
        {
            if (a == null) continue;
            foreach (int to in a.linkTo)
                if (byId.TryGetValue(to, out BattleArea b) && b != null && to > a.areaId)
                    DrawLink(MapPos(a.transform.position), MapPos(b.transform.position));
        }

        // dots
        foreach (BattleArea a in manager.Areas)
        {
            if (a == null) continue;
            var dot = MakeDot("Dot_" + a.areaId, 14f, Locked);
            dot.rectTransform.anchoredPosition = MapPos(a.transform.position);
            dots.Add((a, dot));
        }

        playerDot = MakeDot("Player", 10f, PlayerCol);
        built = true;
    }

    private void Update()
    {
        if (!built) return;
        foreach (var (a, dot) in dots)
        {
            if (a == null || dot == null) continue;
            dot.color = a.Cleared ? Cleared : a.Activated ? Fighting : Open;
        }
        if (playerDot != null && manager != null && manager.Leader != null)
            playerDot.rectTransform.anchoredPosition = MapPos(manager.Leader.position);
    }

    private Vector2 MapPos(Vector3 world)
    {
        Rect r = area.rect;
        float w = r.width - padding * 2f, h = r.height - padding * 2f;
        float u = Mathf.InverseLerp(worldBounds.min.x, worldBounds.max.x, world.x);
        float v = Mathf.InverseLerp(worldBounds.min.y, worldBounds.max.y, world.y);
        return new Vector2(-r.width / 2f + padding + u * w, -r.height / 2f + padding + v * h);
    }

    private Image MakeDot(string n, float size, Color c)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(area, false);
        var img = go.AddComponent<Image>();
        img.sprite = Disc();
        img.color = c;
        img.rectTransform.sizeDelta = new Vector2(size, size);
        return img;
    }

    private void DrawLink(Vector2 a, Vector2 b)
    {
        var go = new GameObject("Link", typeof(RectTransform));
        go.transform.SetParent(area, false);
        var img = go.AddComponent<Image>();
        img.color = LinkCol;
        var rt = img.rectTransform;
        rt.anchoredPosition = (a + b) * 0.5f;
        rt.sizeDelta = new Vector2(Vector2.Distance(a, b), 4f);
        rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
    }

    private static Sprite discSprite;
    private static Sprite Disc()
    {
        if (discSprite != null) return discSprite;
        const int s = 24;
        var tex = new Texture2D(s, s) { filterMode = FilterMode.Bilinear };
        float c = s * 0.5f, r = s * 0.46f;
        var px = new Color[s * s];
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                px[y * s + x] = new Color(1, 1, 1, Mathf.Clamp01(1f - (d - r + 1f)));
            }
        tex.SetPixels(px); tex.Apply();
        discSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
        return discSprite;
    }
}
