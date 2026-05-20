using System;
using UnityEngine;

/// One growing crop on a cell. Growth is real wall-clock time (DateTime.UtcNow) so it
/// keeps advancing across saves; placeholder visuals tint + scale by stage.
[RequireComponent(typeof(SpriteRenderer))]
public class CropInstance : MonoBehaviour
{
    public enum Stage { Seed, Growing, Ripe }

    private CropData data;
    private DateTime plantedAtUtc;
    private SpriteRenderer sr;
    private Stage stage = Stage.Seed;

    public CropData Data => data;
    public bool IsRipe => stage == Stage.Ripe;

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Init(CropData cropData, DateTime plantedUtc)
    {
        data = cropData;
        plantedAtUtc = plantedUtc;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        ApplyStage(Stage.Seed);
    }

    private void Update()
    {
        if (data == null) return;

        float elapsed = (float)(DateTime.UtcNow - plantedAtUtc).TotalSeconds;
        float t = Mathf.Clamp01(elapsed / data.growSeconds);
        Stage next = t >= 1f ? Stage.Ripe : (t >= 0.34f ? Stage.Growing : Stage.Seed);

        if (next != stage) ApplyStage(next);
    }

    private void ApplyStage(Stage s)
    {
        stage = s;
        switch (s)
        {
            case Stage.Seed:
                sr.color = data.seedColor;
                transform.localScale = Vector3.one * 0.4f;
                break;
            case Stage.Growing:
                sr.color = data.growingColor;
                transform.localScale = Vector3.one * 0.7f;
                break;
            case Stage.Ripe:
                sr.color = data.ripeColor;
                transform.localScale = Vector3.one * 1.0f;
                break;
        }
    }
}
