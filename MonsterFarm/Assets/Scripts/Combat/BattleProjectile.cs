using UnityEngine;

/// A purely cosmetic shot fired by a ranged BattleAgent on attack — damage is already applied
/// instantly by the attacker, this just gives ranged combat a visible tell the way melee units
/// have their lunge. Flies from the shooter toward the target, then self-destroys. Generates its
/// own little disc sprite at runtime (no prefab / no imported art needed).
public class BattleProjectile : MonoBehaviour
{
    private const float Speed = 12f;
    private const float MaxLife = 0.6f;
    private static readonly Color ShotColor = new Color(0.6f, 0.95f, 0.5f, 0.95f);

    private Vector3 target;
    private float life;

    public static void Spawn(Vector3 from, Vector3 to, SpriteRenderer shooterSprite)
    {
        var go = new GameObject("Shot");
        go.transform.position = from + Vector3.up * 0.3f;
        go.transform.localScale = Vector3.one * 0.22f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeDisc();
        sr.color = ShotColor;
        sr.sortingOrder = (shooterSprite != null ? shooterSprite.sortingOrder : 0) + 3;

        var p = go.AddComponent<BattleProjectile>();
        p.target = to + Vector3.up * 0.3f;
    }

    private void Update()
    {
        life += Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, Speed * Time.deltaTime);
        if (life >= MaxLife || (transform.position - target).sqrMagnitude < 0.02f)
            Destroy(gameObject);
    }

    private static Sprite discSprite;
    private static Sprite MakeDisc()
    {
        if (discSprite != null) return discSprite;
        const int size = 16;
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        float c = size * 0.5f, r = size * 0.46f;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                px[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - (d - r + 1f)));
            }
        tex.SetPixels(px);
        tex.Apply();
        discSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return discSprite;
    }
}
