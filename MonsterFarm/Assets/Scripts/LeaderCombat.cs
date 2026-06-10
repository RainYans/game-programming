using System.Collections;
using UnityEngine;

/// Makes the squad LEADER an active fighter (action-brawler feel): a short-range arc melee swing
/// that damages + knocks back enemies in front. Auto-swings the nearest enemy in range on a
/// cooldown, and the player can left-click to swing toward the cursor on demand
/// (BattleCommandController routes the click here). Pure presentation + damage; movement stays
/// with AvatarController. No physics — damage is distance/arc based like the rest of the battle.
[RequireComponent(typeof(AvatarController))]
public class LeaderCombat : MonoBehaviour
{
    [Header("Melee swing")]
    [SerializeField] private int damage = 14;
    [SerializeField] private float range = 2.0f;
    [SerializeField] private float attackInterval = 0.5f;
    [SerializeField] private float arcDegrees = 150f;
    [SerializeField] private float knockback = 0.7f;
    [Tooltip("Auto-swing the nearest enemy in range without input. Off = the player must left-click.")]
    [SerializeField] private bool autoAttack = false;

    [Header("Swing animation (Player.png attack frames; wired by setup)")]
    [SerializeField] private Sprite[] attackDown;
    [SerializeField] private Sprite[] attackSide;   // faces RIGHT; flipped for left
    [SerializeField] private Sprite[] attackUp;
    [SerializeField] private float animFrameTime = 0.07f;
    [SerializeField] private PlayerWalkAnim walkAnim;

    private BattleManager manager;
    private SpriteRenderer sr;
    private BattleCameraFollow cam;
    private AvatarController ctrl;
    private LeaderCombatant hp;
    private float cooldown;
    private bool swinging;
    private static Sprite slashSprite;

    /// Number of swings that actually connected with an enemy. Read by the tutorial to advance the
    /// "left-click to attack" step only once the player has truly landed a hit.
    public int SwingHitCount { get; private set; }

    private void Awake()
    {
        manager = FindFirstObjectByType<BattleManager>();
        sr = GetComponentInChildren<SpriteRenderer>();
        cam = FindFirstObjectByType<BattleCameraFollow>();
        ctrl = GetComponent<AvatarController>();
        hp = GetComponent<LeaderCombatant>();
        if (walkAnim == null) walkAnim = GetComponent<PlayerWalkAnim>();
    }

    private void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
        if (manager == null) return;
        if (hp != null && !hp.IsAlive) return; // dead hero can't swing
        // Optional auto-attack (off by default — the player left-clicks to swing).
        if (autoAttack && cooldown <= 0f)
        {
            BattleAgent near = NearestEnemy();
            if (near != null) Swing((Vector2)(near.transform.position - transform.position));
        }
    }

    /// Manual swing toward a world point (left-click). Returns false if still on cooldown.
    public bool TrySwing(Vector3 aimWorld)
    {
        if (cooldown > 0f) return false;
        if (hp != null && !hp.IsAlive) return false;
        Vector2 dir = (Vector2)(aimWorld - transform.position);
        if (dir.sqrMagnitude < 0.04f)
            dir = ctrl != null ? new Vector2(ctrl.FacingCell.x, ctrl.FacingCell.y) : Vector2.right;
        Swing(dir);
        return true;
    }

    private BattleAgent NearestEnemy()
    {
        BattleAgent best = null;
        float bestSqr = range * range;
        Vector2 p = transform.position;
        var foes = manager.Enemies;
        for (int i = 0; i < foes.Count; i++)
        {
            BattleAgent e = foes[i];
            if (e == null || !e.IsAlive) continue;
            float s = ((Vector2)e.transform.position - p).sqrMagnitude;
            if (s <= bestSqr) { bestSqr = s; best = e; }
        }
        return best;
    }

    private void Swing(Vector2 dir)
    {
        cooldown = attackInterval;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();
        if (sr != null && Mathf.Abs(dir.x) > 0.05f) sr.flipX = dir.x < 0f;

        float half = arcDegrees * 0.5f;
        bool hitAny = false;
        var foes = manager.Enemies;
        for (int i = 0; i < foes.Count; i++)
        {
            BattleAgent e = foes[i];
            if (e == null || !e.IsAlive) continue;
            Vector2 to = (Vector2)e.transform.position - (Vector2)transform.position;
            if (to.sqrMagnitude > range * range) continue;
            if (Vector2.Angle(dir, to) > half) continue;
            e.TakeDamage(damage);
            e.Repel(transform.position, knockback, 0.18f);
            hitAny = true;
        }
        if (hitAny) SwingHitCount++;

        SpawnSlash(dir);
        if (hitAny) SfxManager.Play(SfxKind.Hit);
        if (cam != null) cam.Shake(hitAny ? 0.14f : 0.05f, 0.12f);
        if (gameObject.activeInHierarchy) StartCoroutine(SwingAnim(dir));
    }

    private Sprite[] AttackSet(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y)) return attackSide;
        return dir.y > 0f ? attackUp : attackDown;
    }

    /// Plays the Player.png sword-swing frames for the swing direction, taking the sprite over
    /// from PlayerWalkAnim for the duration so the hero visibly attacks.
    private IEnumerator SwingAnim(Vector2 dir)
    {
        Sprite[] set = AttackSet(dir);
        if (sr == null || set == null || set.Length == 0) yield break;
        if (swinging) yield break;
        swinging = true;
        bool side = Mathf.Abs(dir.x) >= Mathf.Abs(dir.y);
        if (side && sr != null) sr.flipX = dir.x < 0f;
        if (walkAnim != null) walkAnim.Suppressed = true;
        for (int i = 0; i < set.Length; i++)
        {
            if (sr != null && set[i] != null) sr.sprite = set[i];
            yield return new WaitForSeconds(animFrameTime);
        }
        if (walkAnim != null) walkAnim.Suppressed = false;
        swinging = false;
    }

    private void SpawnSlash(Vector2 dir)
    {
        var go = new GameObject("LeaderSlash");
        go.transform.position = transform.position + (Vector3)(dir * (range * 0.5f)) + new Vector3(0f, 0.3f, 0f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        go.transform.localScale = Vector3.one * (range * 1.15f);
        var s = go.AddComponent<SpriteRenderer>();
        s.sprite = SlashSprite();
        s.color = new Color(1f, 1f, 1f, 0.9f);
        s.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
        s.sortingOrder = 20;
        StartCoroutine(FadeSlash(s));
    }

    private IEnumerator FadeSlash(SpriteRenderer s)
    {
        float t = 0f, dur = 0.14f;
        Transform tr = s != null ? s.transform : null;
        while (t < dur && s != null)
        {
            t += Time.deltaTime;
            float k = 1f - t / dur;
            s.color = new Color(1f, 1f, 1f, 0.9f * k);
            if (tr != null) tr.localScale = Vector3.one * (range * (1.0f + 0.35f * (1f - k)));
            yield return null;
        }
        if (s != null) Destroy(s.gameObject);
    }

    private static Sprite SlashSprite()
    {
        if (slashSprite != null) return slashSprite;
        const int N = 48;
        var tex = new Texture2D(N, N) { filterMode = FilterMode.Bilinear };
        var px = new Color[N * N];
        float c = (N - 1) / 2f, R = N * 0.5f;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = x - c, dy = y - c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                float a = 0f;
                if (d >= R * 0.58f && d <= R * 0.93f && Mathf.Abs(ang) <= 74f)
                {
                    float radial = 1f - Mathf.Abs((d - R * 0.755f) / (R * 0.175f));
                    float angular = 1f - Mathf.Abs(ang) / 74f;
                    a = Mathf.Clamp01(radial) * Mathf.Clamp01(angular + 0.25f);
                }
                px[y * N + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(px); tex.Apply();
        slashSprite = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N);
        return slashSprite;
    }
}
