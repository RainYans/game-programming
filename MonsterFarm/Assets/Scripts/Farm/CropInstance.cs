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
    private Sprite monsterArt;   // real harvested-monster sprite (Resources/Monsters/<id>), null = placeholder

    public CropData Data => data;
    public bool IsRipe => stage == Stage.Ripe;
    public DateTime PlantedAtUtc => plantedAtUtc;

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Init(CropData cropData, DateTime plantedUtc)
    {
        data = cropData;
        plantedAtUtc = plantedUtc;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        monsterArt = cropData != null ? Resources.Load<Sprite>("Monsters/" + cropData.id) : null;
        if (monsterArt != null) sr.sprite = monsterArt;
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
        bool art = monsterArt != null;   // with real art: keep sprite, fade+grow by stage
        switch (s)
        {
            case Stage.Seed:
                sr.color = art ? new Color(1f, 1f, 1f, 0.55f) : data.seedColor;
                transform.localScale = Vector3.one * (art ? 0.5f : 0.4f);
                break;
            case Stage.Growing:
                sr.color = art ? new Color(1f, 1f, 1f, 0.8f) : data.growingColor;
                transform.localScale = Vector3.one * (art ? 0.72f : 0.7f);
                break;
            case Stage.Ripe:
                sr.color = art ? Color.white : data.ripeColor;
                transform.localScale = Vector3.one * (art ? 0.92f : 1.0f);
                break;
        }
    }
}
