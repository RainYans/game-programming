using UnityEngine;

/// One real-time combat unit (a squad zombie or a wild enemy). Behaviour priority (highest
/// first):
///   0. Frozen — Freeze Canister disables the agent for a short time.
///   1. Flee — temporary "shoved" state from a Rotten Onion blast.
///   2. Commanded target — focus-fire an enemy the player right-clicked (player only).
///   3. Commanded move — walk to a ground point the player right-clicked (player only).
///   4. Default AI — aggro the nearest opposing agent within range; otherwise squad follows
///      the leader, enemies hold position.
///
/// Six per-strain passives are implemented here (ZombieData.passive):
///   ThickHide   — Brute: flat damage reduction taken.
///   Bloodlust   — Mauler: consecutive hits on the SAME target deal escalating damage.
///   Evasion     — Runner: % chance to dodge incoming damage.
///   Corrosion   — Spitter: on attack, debuffs target to take +X% damage for a few seconds.
///   Aura        — Shaman: periodically heals nearby same-team allies.
///   SelfDetonate— Bomber: deals AoE damage to opponents on death.
public class BattleAgent : MonoBehaviour
{
    public Team Team { get; private set; }
    public bool IsAlive => hp > 0;
    public string SourceUid { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public int Hp => hp;
    public int MaxHp => maxHp;

    private int maxHp;
    private int hp;
    private int attack;
    private float moveSpeed;
    private AttackRange range;
    private Passive passive;

    private BattleManager manager;
    private Transform leader;
    private SpriteRenderer sprite;
    private float attackTimer;
    private float flashTimer;
    private Color baseColor;

    private BattleAgent commandedTarget;
    private Vector3? commandedMove;

    private Vector3 fleeTarget;
    private float fleeTimer;

    private float frozenTimer;

    // Bloodlust — attacker state.
    private BattleAgent lastAttackTarget;
    private int consecutiveHits;

    // Corrosion — victim state.
    private float corrosionTimer;
    private float corrosionMultiplier;

    // Aura — periodic tick on Shaman.
    private float auraTimer;

    private SpriteRenderer hpFill;
    private SpriteRenderer selectionRing;

    private static Sprite squareSprite;
    private static Sprite leftSquareSprite;
    private static Sprite discSprite;

    // Tunables — placeholder; move to GameConfig during the balancing pass.
    private const float MeleeReach = 0.7f;
    private const float RangedReach = 4f;
    private const float AggroRange = 4f;
    private const float AttackInterval = 1.0f;
    private const float FollowStopDistance = 1.6f;
    private const float IsoYScale = 0.5f;
    private const float ArriveEpsilon = 0.05f;
    private const float MoveArriveDistance = 0.3f;

    // Passive tunables.
    private const int   ThickHideReduction = 2;
    private const float BloodlustPerHitBonus = 0.20f; // +20% per stack
    private const int   BloodlustMaxStacks = 5;
    private const float EvasionChance = 0.30f;
    private const float CorrosionMultiplier = 0.5f;
    private const float CorrosionDuration = 4f;
    private const float AuraTickInterval = 2f;
    private const int   AuraHealAmount = 2;
    private const float AuraRadius = 2.5f;
    private const float DetonateRadius = 1.8f;
    private const int   DetonateDamage = 8;
    private const float FlashSeconds = 0.14f;

    private static readonly Color HitColor = Color.white;
    private static readonly Color FrozenColor = new Color(0.55f, 0.80f, 1f);
    private static readonly Color DmgColor = new Color(1f, 0.65f, 0.55f);
    private static readonly Color HealColor = new Color(0.55f, 0.95f, 0.55f);
    private static readonly Color DodgeColor = new Color(0.85f, 0.85f, 0.95f);

    public void Init(BattleManager mgr, ZombieData data, Team team, Transform leaderTransform,
        string sourceUid, float damageMultiplier = 1f)
    {
        manager = mgr;
        Team = team;
        leader = leaderTransform;
        SourceUid = sourceUid;
        DisplayName = data.displayName;

        maxHp = Mathf.Max(1, data.maxHp);
        hp = maxHp;
        attack = Mathf.Max(1, data.attack);
        // Hunger makes a unit hit harder (snapshotted at deploy; 1x for Full units and enemies).
        if (damageMultiplier > 1f) attack = Mathf.Max(1, Mathf.RoundToInt(attack * damageMultiplier));
        moveSpeed = Mathf.Max(0.1f, data.moveSpeed);
        range = data.range;
        passive = data.passive;
        auraTimer = AuraTickInterval; // delay first tick

        sprite = GetComponentInChildren<SpriteRenderer>();
        baseColor = data.color;
        if (sprite != null) sprite.color = baseColor;

        BuildHpBar();
        if (Team == Team.Player) BuildSelectionRing();
    }

    // --- public API for the controller / items ---------------------------------------------

    public void SetSelected(bool s)
    {
        if (selectionRing != null) selectionRing.enabled = s && IsAlive;
    }

    public void SetCommandTarget(BattleAgent target)
    {
        if (Team != Team.Player) return;
        commandedTarget = target;
        commandedMove = null;
    }

    public void SetMoveCommand(Vector3 worldPos)
    {
        if (Team != Team.Player) return;
        commandedTarget = null;
        commandedMove = worldPos;
    }

    public void ClearCommands()
    {
        commandedTarget = null;
        commandedMove = null;
    }

    public void Repel(Vector3 center, float distance, float duration)
    {
        if (Team != Team.Enemy || !IsAlive) return;
        Vector2 away = (Vector2)(transform.position - center);
        if (away.sqrMagnitude < 0.0001f) away = Vector2.right;
        fleeTarget = transform.position + (Vector3)(away.normalized * distance);
        fleeTimer = Mathf.Max(fleeTimer, duration);
    }

    /// Freeze the agent in place (and skipping attacks) for `duration` seconds.
    public void Freeze(float duration)
    {
        if (!IsAlive) return;
        frozenTimer = Mathf.Max(frozenTimer, duration);
        if (sprite != null) sprite.color = FrozenColor;
    }

    /// Apply a Corrosion-style debuff: incoming damage multiplied by (1 + multiplier).
    public void ApplyCorrosion(float duration, float multiplier)
    {
        if (!IsAlive) return;
        corrosionTimer = Mathf.Max(corrosionTimer, duration);
        corrosionMultiplier = Mathf.Max(corrosionMultiplier, multiplier);
    }

    // --- Update -----------------------------------------------------------------------------

    private void Update()
    {
        if (!IsAlive || manager == null) return;

        TickFlashAndDebuffs();

        attackTimer -= Time.deltaTime;

        // 0) Frozen — skip everything but the timer.
        if (frozenTimer > 0f)
        {
            frozenTimer -= Time.deltaTime;
            if (frozenTimer <= 0f && sprite != null && flashTimer <= 0f) sprite.color = baseColor;
            return;
        }

        // 1) Flee.
        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.deltaTime;
            MoveToward(fleeTarget);
            return;
        }

        // Shaman aura tick (passive — runs whether engaged or not).
        if (passive == Passive.Aura) TickAura();

        // 2) Commanded target.
        if (commandedTarget != null)
        {
            if (!commandedTarget.IsAlive) commandedTarget = null;
            else { EngageTarget(commandedTarget); return; }
        }

        // 3) Commanded move.
        if (commandedMove.HasValue)
        {
            if (Vector2.Distance(transform.position, commandedMove.Value) < MoveArriveDistance)
                commandedMove = null;
            else { MoveToward(commandedMove.Value); return; }
        }

        // 4) Default AI.
        BattleAgent target = manager.NearestEnemyOf(this, AggroRange);
        if (target != null) { EngageTarget(target); return; }

        if (Team == Team.Player && leader != null)
        {
            if (Vector2.Distance(transform.position, leader.position) > FollowStopDistance)
                MoveToward(leader.position);
        }
        // Enemy idle: hold position.
    }

    private void TickFlashAndDebuffs()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sprite != null)
                sprite.color = frozenTimer > 0f ? FrozenColor : baseColor;
        }
        if (corrosionTimer > 0f) corrosionTimer -= Time.deltaTime;
    }

    private void EngageTarget(BattleAgent target)
    {
        float reach = range == AttackRange.Ranged ? RangedReach : MeleeReach;
        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist > reach)
            MoveToward(target.transform.position);
        else if (attackTimer <= 0f)
        {
            FaceToward(target.transform.position);
            int outgoing = attack;

            // Mauler — Bloodlust ramps damage on consecutive same-target hits.
            if (passive == Passive.Bloodlust)
            {
                if (lastAttackTarget == target) consecutiveHits = Mathf.Min(consecutiveHits + 1, BloodlustMaxStacks);
                else { lastAttackTarget = target; consecutiveHits = 1; }
                float bonus = 1f + (consecutiveHits - 1) * BloodlustPerHitBonus;
                outgoing = Mathf.RoundToInt(attack * bonus);
            }

            target.TakeDamage(outgoing, this);

            // Spitter — Corrosion debuff on hit.
            if (passive == Passive.Corrosion && target.IsAlive)
                target.ApplyCorrosion(CorrosionDuration, CorrosionMultiplier);

            attackTimer = AttackInterval;
        }
    }

    private void MoveToward(Vector3 worldTarget)
    {
        Vector2 to = (Vector2)(worldTarget - transform.position);
        if (to.sqrMagnitude < ArriveEpsilon * ArriveEpsilon) return;
        Vector2 dir = to.normalized;
        dir.y *= IsoYScale;
        transform.position += (Vector3)(dir * (moveSpeed * Time.deltaTime));
        FaceDir(dir.x);
    }

    private void FaceToward(Vector3 worldTarget) => FaceDir(worldTarget.x - transform.position.x);

    private void FaceDir(float dx)
    {
        if (sprite != null && Mathf.Abs(dx) > 0.01f) sprite.flipX = dx < 0f;
    }

    public void TakeDamage(int amount, BattleAgent attacker = null)
    {
        if (!IsAlive) return;
        int finalAmount = Mathf.Max(0, amount);

        // Spitter Corrosion (victim side).
        if (corrosionTimer > 0f)
            finalAmount = Mathf.RoundToInt(finalAmount * (1f + corrosionMultiplier));

        // Brute ThickHide.
        if (passive == Passive.ThickHide)
            finalAmount = Mathf.Max(0, finalAmount - ThickHideReduction);

        // Runner Evasion.
        if (passive == Passive.Evasion && Random.value < EvasionChance)
        {
            DamagePopup.Spawn(transform.position, "Dodge!", DodgeColor);
            return;
        }

        hp -= finalAmount;
        if (finalAmount > 0)
        {
            DamagePopup.Spawn(transform.position, $"-{finalAmount}", DmgColor);
            SfxManager.Play(SfxKind.Hit);
        }

        if (sprite != null) { sprite.color = HitColor; flashTimer = FlashSeconds; }
        RefreshHpBar();

        if (hp <= 0) Die(attacker);
    }

    /// Heal up to maxHp; pops a green "+N".
    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0 || hp >= maxHp) return;
        int actual = Mathf.Min(amount, maxHp - hp);
        hp += actual;
        RefreshHpBar();
        DamagePopup.Spawn(transform.position, $"+{actual}", HealColor);
    }

    private void TickAura()
    {
        auraTimer -= Time.deltaTime;
        if (auraTimer > 0f) return;
        auraTimer = AuraTickInterval;

        var allies = Team == Team.Player ? manager.Players : manager.Enemies;
        foreach (BattleAgent a in allies)
        {
            if (a == null || a == this || !a.IsAlive) continue;
            if (Vector2.Distance(a.transform.position, transform.position) <= AuraRadius)
                a.Heal(AuraHealAmount);
        }
    }

    private void Die(BattleAgent killer)
    {
        hp = 0;
        RefreshHpBar();
        if (selectionRing != null) selectionRing.enabled = false;

        // Bomber — SelfDetonate AoE on death.
        if (passive == Passive.SelfDetonate)
        {
            var foes = Team == Team.Player ? manager.Enemies : manager.Players;
            foreach (BattleAgent f in foes)
            {
                if (f == null || !f.IsAlive) continue;
                if (Vector2.Distance(f.transform.position, transform.position) <= DetonateRadius)
                    f.TakeDamage(DetonateDamage, this);
            }
            DamagePopup.Spawn(transform.position, "BOOM!", new Color(1f, 0.7f, 0.3f));
        }

        SfxManager.Play(SfxKind.Death);
        if (manager != null) manager.OnAgentDied(this);
        Destroy(gameObject);
    }

    // --- visuals (built in Init) -----------------------------------------------------------

    private void BuildHpBar()
    {
        float agentScale = transform.localScale.x;
        float inv = agentScale > 0.001f ? 1f / agentScale : 1f;

        var rootGo = new GameObject("HpRoot");
        rootGo.transform.SetParent(transform, false);
        rootGo.transform.localPosition = new Vector3(0f, 0.95f * inv, 0f);
        rootGo.transform.localScale = new Vector3(inv, inv, 1f);

        var bgGo = new GameObject("HpBg");
        bgGo.transform.SetParent(rootGo.transform, false);
        bgGo.transform.localScale = new Vector3(0.8f, 0.10f, 1f);
        var bg = bgGo.AddComponent<SpriteRenderer>();
        bg.sprite = EnsureSquare();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.9f);
        bg.sortingOrder = (sprite != null ? sprite.sortingOrder : 0) + 2;

        var fillGo = new GameObject("HpFill");
        fillGo.transform.SetParent(rootGo.transform, false);
        fillGo.transform.localPosition = new Vector3(-0.4f, 0f, 0f);
        fillGo.transform.localScale = new Vector3(0.8f, 0.08f, 1f);
        hpFill = fillGo.AddComponent<SpriteRenderer>();
        hpFill.sprite = EnsureSquareLeftPivot();
        hpFill.color = Team == Team.Player
            ? new Color(0.42f, 0.85f, 0.42f)
            : new Color(0.85f, 0.35f, 0.35f);
        hpFill.sortingOrder = bg.sortingOrder + 1;
    }

    private void RefreshHpBar()
    {
        if (hpFill == null) return;
        float frac = Mathf.Clamp01((float)hp / maxHp);
        Vector3 s = hpFill.transform.localScale;
        hpFill.transform.localScale = new Vector3(0.8f * frac, s.y, s.z);
    }

    private void BuildSelectionRing()
    {
        float agentScale = transform.localScale.x;
        float inv = agentScale > 0.001f ? 1f / agentScale : 1f;

        var ringGo = new GameObject("SelectionRing");
        ringGo.transform.SetParent(transform, false);
        ringGo.transform.localPosition = new Vector3(0f, -0.05f * inv, 0f);
        ringGo.transform.localScale = new Vector3(0.9f * inv, 0.5f * inv, 1f);
        selectionRing = ringGo.AddComponent<SpriteRenderer>();
        selectionRing.sprite = EnsureDisc();
        selectionRing.color = new Color(1f, 0.92f, 0.30f, 0.55f);
        selectionRing.sortingOrder = (sprite != null ? sprite.sortingOrder : 0) - 1;
        selectionRing.enabled = false;
    }

    // --- runtime sprite generation ---------------------------------------------------------

    private static Sprite EnsureSquare()
    {
        if (squareSprite != null) return squareSprite;
        var tex = MakeWhite(8);
        squareSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        return squareSprite;
    }

    private static Sprite EnsureSquareLeftPivot()
    {
        if (leftSquareSprite != null) return leftSquareSprite;
        var tex = MakeWhite(8);
        leftSquareSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0f, 0.5f), 8f);
        return leftSquareSprite;
    }

    private static Sprite EnsureDisc()
    {
        if (discSprite != null) return discSprite;
        const int size = 32;
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        float cx = size * 0.5f, cy = size * 0.5f, r = size * 0.48f;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float a = Mathf.Clamp01(1f - (d - r + 1f));
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        discSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return discSprite;
    }

    private static Texture2D MakeWhite(int size)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Point };
        var px = new Color[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }
}
