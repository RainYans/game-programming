using System.Collections.Generic;
using UnityEngine;

/// Gentle title-screen atmosphere: drifts the dust-mote child objects slowly upward with a sway,
/// wrapping them back to the bottom when they leave the top. The motes are REAL, editable child
/// GameObjects placed in the scene (so they show in the Hierarchy and the Inspector) — this script
/// only animates whatever motes are parented under it; it never creates objects at runtime. Add or
/// remove "Mote" children in the editor to taste.
public class MenuAmbience : MonoBehaviour
{
    [SerializeField] private float riseMin = 14f, riseMax = 34f; // px/sec upward
    [SerializeField] private float swayAmpMin = 8f, swayAmpMax = 22f;

    private struct Mote { public RectTransform rt; public float rise, swayAmp, swayHz, phase; }
    private readonly List<Mote> motes = new List<Mote>();
    private RectTransform area;

    private void Awake()
    {
        area = transform as RectTransform;
        if (area == null) return;
        foreach (RectTransform child in area)
        {
            if (!child.gameObject.activeSelf) continue;
            motes.Add(new Mote
            {
                rt = child,
                rise = Random.Range(riseMin, riseMax),
                swayAmp = Random.Range(swayAmpMin, swayAmpMax),
                swayHz = Random.Range(0.2f, 0.6f),
                phase = Random.Range(0f, 6.28f)
            });
        }
    }

    private void Update()
    {
        if (area == null || motes.Count == 0) return;
        Rect r = area.rect;
        float top = r.height * 0.5f, bottom = -r.height * 0.5f;
        for (int i = 0; i < motes.Count; i++)
        {
            Mote m = motes[i];
            Vector2 p = m.rt.anchoredPosition;
            p.y += m.rise * Time.unscaledDeltaTime;
            p.x += Mathf.Sin(Time.unscaledTime * m.swayHz + m.phase) * m.swayAmp * Time.unscaledDeltaTime;
            if (p.y > top + 24f) { p.y = bottom - 24f; p.x = Random.Range(-r.width * 0.5f, r.width * 0.5f); }
            m.rt.anchoredPosition = p;
        }
    }
}
