using UnityEngine;

/// One real-time combat unit (a squad zombie or a wild enemy). Stats come from a ZombieData.
/// Behaviour is intentionally light — the design's depth is in prep + positioning, not unit
/// micro: acquire the nearest enemy in aggro range, close to attack range, auto-attack on a
/// cooldown; with no enemy near, player units loosely follow the leader and enemies advance.
///
/// Slice 1 of combat (real-time core). Movement is transform-based (no physics yet); melee/
/// ranged differ only by reach for now (projectiles + passives come later).
public class BattleAgent : MonoBehaviour
{
    public Team Team { get; private set; }
    public bool IsAlive => hp > 0;
    /// For player units, the owning ZombieUnit.uid — used later for permadeath. Empty for enemies.
    public string SourceUid { get; private set; }

    private int maxHp;
    private int hp;
    private int attack;
    private float moveSpeed;
    private AttackRange range;

    private BattleManager manager;
    private Transform leader;
    private SpriteRenderer sprite;
    private float attackTimer;
    private float flashTimer;
    private Color baseColor;

    // Tunables (placeholder; move to GameConfig during the balancing pass).
    private const float MeleeReach = 0.7f;
    private const float RangedReach = 4f;
    private const float AggroRange = 7f;
    private const float AttackInterval = 1.0f;
    private const float FollowStopDistance = 1.6f;
    private const float IsoYScale = 0.5f;
    private const float ArriveEpsilon = 0.05f;

    public void Init(BattleManager mgr, ZombieData data, Team team, Transform leaderTransform, string sourceUid)
    {
        manager = mgr;
        Team = team;
        leader = leaderTransform;
        SourceUid = sourceUid;

        maxHp = Mathf.Max(1, data.maxHp);
        hp = maxHp;
        attack = Mathf.Max(1, data.attack);
        moveSpeed = Mathf.Max(0.1f, data.moveSpeed);
        range = data.range;

        sprite = GetComponentInChildren<SpriteRenderer>();
        baseColor = data.color;
        if (sprite != null) sprite.color = baseColor;
    }

    private void Update()
    {
        if (!IsAlive || manager == null) return;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sprite != null) sprite.color = baseColor;
        }

        attackTimer -= Time.deltaTime;

        BattleAgent target = manager.NearestEnemyOf(this, AggroRange);
        float reach = range == AttackRange.Ranged ? RangedReach : MeleeReach;

        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist > reach)
                MoveToward(target.transform.position);
            else if (attackTimer <= 0f)
            {
                FaceToward(target.transform.position);
                target.TakeDamage(attack);
                attackTimer = AttackInterval;
            }
        }
        else
        {
            // Idle behaviour: squad tags along behind the leader; enemies push toward it.
            if (Team == Team.Player && leader != null)
            {
                if (Vector2.Distance(transform.position, leader.position) > FollowStopDistance)
                    MoveToward(leader.position);
            }
            else if (Team == Team.Enemy && leader != null)
            {
                MoveToward(leader.position);
            }
        }
    }

    private void MoveToward(Vector3 worldTarget)
    {
        Vector2 to = (Vector2)(worldTarget - transform.position);
        if (to.sqrMagnitude < ArriveEpsilon * ArriveEpsilon) return;
        Vector2 dir = to.normalized;
        dir.y *= IsoYScale; // keep motion feeling iso-consistent with the leader
        transform.position += (Vector3)(dir * (moveSpeed * Time.deltaTime));
        FaceDir(dir.x);
    }

    private void FaceToward(Vector3 worldTarget) => FaceDir(worldTarget.x - transform.position.x);

    private void FaceDir(float dx)
    {
        if (sprite != null && Mathf.Abs(dx) > 0.01f) sprite.flipX = dx < 0f;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        hp -= Mathf.Max(0, amount);
        if (sprite != null) { sprite.color = Color.white; flashTimer = 0.08f; }
        if (hp <= 0) Die();
    }

    private void Die()
    {
        hp = 0;
        manager.OnAgentDied(this);
        Destroy(gameObject);
    }
}
