using UnityEngine;

/// PRESENTATION-ONLY. Loops a set of sprite frames on a SpriteRenderer. Pure decoration
/// (water, grass, wings, smoke, windmill, idle animals) — touches no gameplay/state/save.
[DisallowMultipleComponent]
public class DecorAnim : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 6f;
    public bool randomizePhase = true;

    [SerializeField] private SpriteRenderer sr;
    private float timer;
    private int idx;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0)
        {
            if (randomizePhase)
            {
                idx = Random.Range(0, frames.Length);
                timer = Random.value * (1f / Mathf.Max(0.1f, fps));
            }
            if (sr != null) sr.sprite = frames[idx];
        }
    }

    private void Update()
    {
        if (sr == null || frames == null || frames.Length < 2 || fps <= 0f) return;
        timer += Time.deltaTime;
        float spf = 1f / fps;
        while (timer >= spf)
        {
            timer -= spf;
            idx = (idx + 1) % frames.Length;
            sr.sprite = frames[idx];
        }
    }
}
