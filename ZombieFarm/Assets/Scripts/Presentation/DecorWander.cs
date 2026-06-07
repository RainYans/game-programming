using UnityEngine;

/// PRESENTATION-ONLY. Idly wanders inside a circle around the spawn point, pausing between
/// hops, flipping to face travel. For decorative animals/critters. No gameplay coupling.
[DisallowMultipleComponent]
public class DecorWander : MonoBehaviour
{
    public float speed = 0.5f;
    public float radius = 2.5f;
    public Vector2 pauseRange = new Vector2(0.8f, 3f);
    public bool flipX = true;

    private SpriteRenderer sr;
    private Vector3 home, target;
    private float pause;

    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        home = transform.position;
        Pick();
    }

    private void Update()
    {
        if (pause > 0f) { pause -= Time.deltaTime; return; }

        Vector3 to = target - transform.position; to.z = 0f;
        if (to.sqrMagnitude < 0.0025f)
        {
            pause = Random.Range(pauseRange.x, pauseRange.y);
            Pick();
            return;
        }
        Vector3 dir = to.normalized;
        transform.position += dir * (speed * Time.deltaTime);
        if (flipX && sr != null && Mathf.Abs(dir.x) > 0.01f) sr.flipX = dir.x < 0f;
    }

    private void Pick()
    {
        Vector2 r = Random.insideUnitCircle * radius;
        target = home + new Vector3(r.x, r.y, 0f);
    }
}
