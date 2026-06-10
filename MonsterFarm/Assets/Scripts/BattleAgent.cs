using System.Collections.Generic;
using UnityEngine;

/// Which side an agent fights on. (Was defined in the old BattleSimulator; kept here now that
/// the prototype sim is gone — BattleAgent/BattleManager still use it.)
public enum Team { Player, Enemy }

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
    /// Static portrait sprite for the squad HUD card (strain icon, or first anim frame).
    public Sprite Portrait { get; private set; }

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
    private bool dummy; // training target: stands still, never attacks

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

    // Animated real monster art (Resources/MonsterAnim/<id>), cycled while alive.
    private Sprite[] animFrames;
    private float animTimer;
    private int animIdx;
    private const float AnimFps = 6f;

    // Feel constants (fixed). The balance-relevant tunables now live in GameConfig.CombatTuning.
    private const float FollowStopDistance = 1.6f;
    private const float IsoYScale = 1f; // top-down: no Y squash (was 0.5f for isometric)
    private const float BattleMoveScale = 1.1f; // global unit speed (units must keep up with the leader)
    private const float LungeDuration = 0.13f;
    private const float LungeSpeed = 7f;
    private float lungeTimer;
    private Vector2 lungeDir;
    private const float ArriveEpsilon = 0.05f;
    private const float MoveArriveDistance = 0.3f;
    private const float FlashSeconds = 0.14f;

    // Balance tunables, resolved from GameConfig via BattleManager.Tuning at Init.
    private GameConfig.CombatTuning t;
    // Incoming-damage multiplier from being deployed Hungry (1 = Full / enemy).
    private float damageTakenMultiplier = 1f;

    private static readonly Color HitColor = Color.white;
    private static readonly Color FrozenColor = new Color(0.55f, 0.80f, 1f);
    private static readonly Color DmgColor = new Color(1f, 0.65f, 0.55f);
    private static readonly Color HealColor = new Color(0.55f, 0.95f, 0.55f);
    private static readonly Color DodgeColor = new Color(0.85f, 0.85f, 0.95f);

    public void Init(BattleManager mgr, ZombieData data, Team team, Transform leaderTransform,
        string sourceUid, float damageMultiplier = 1f, float damageTakenMultiplier = 1f)
    {
        manager = mgr;
        Team = team;
        leader = leaderTransform;
        SourceUid = sourceUid;
        DisplayName = data.displayName;
        t = mgr != null && mgr.Tuning != null ? mgr.Tuning : new GameConfig.CombatTuning();
        this.damageTakenMultiplier = damageTakenMultiplier;

        maxHp = Mathf.Max(1, data.maxHp);
        hp = maxHp;
        attack = Mathf.Max(1, data.attack);
        // Hunger makes a unit hit harder (snapshotted at deploy; 1x for Full units and enemies).
        if (damageMultiplier > 1f) attack = Mathf.Max(1, Mathf.RoundToInt(attack * damageMultiplier));
        moveSpeed = Mathf.Max(0.1f, data.moveSpeed) * BattleMoveScale;
        range = data.range;
        passive = data.passive;
        auraTimer = t.auraTickInterval; // delay first tick

        sprite = GetComponentInChildren<SpriteRenderer>();
        // Real animated monster art by strain id; fall back to a static sprite, then the
        // placeholder colour tint if no art exists.
        animFrames = Resources.LoadAll<Sprite>("MonsterAnim/" + data.id);
        if (animFrames != null && animFrames.Length > 0)
        {
            System.Array.Sort(animFrames, (a, b) => string.CompareOrdinal(a.name, b.name));
            baseColor = Color.white;
            if (sprite != null) sprite.sprite = animFrames[0];
        }
        else
        {
            Sprite still = Resources.Load<Sprite>("Monsters/" + data.id);
            if (still != null) { baseColor = Color.white; if (sprite != null) sprite.sprite = still; }
            else baseColor = data.color;
        }
        if (sprite != null) sprite.color = baseColor;

        // Portrait for the squad HUD card: prefer the static strain icon, else the first anim frame.
        Sprite icon = Resources.Load<Sprite>("Monsters/" + data.id);
        Portrait = icon != null ? icon
                 : (animFrames != null && animFrames.Length > 0 ? animFrames[0] : null);

        BuildHpBar();
        if (Team == Team.Player) BuildSelectionRing();
    }

    // --- public API for the controller / items ---------------------------------------------

    public void SetSelected(bool s)
    {
        if (selectionRing != null) selectionRing.enabled = s && IsAlive;
    }

    /// Training dummy: the agent stands still and never attacks, but still takes damage and reacts
    /// to Freeze / Rotten Onion. Used by the combat tutorial as a safe practice target.
    public void SetDummy(bool d) => dummy = d;

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
        Animate();

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

        // Attack lunge — jab toward the target then back (a clear melee "swing", no drift).
        if (lungeTimer > 0f)
        {
            lungeTimer -= Time.deltaTime;
            float sign = lungeTimer > LungeDuration * 0.5f ? 1f : -1f;
            transform.position += (Vector3)(lungeDir * sign * (LungeSpeed * Time.deltaTime));
            return;
        }

        // Training dummy: never engages, moves, or attacks — just stands and takes hits. (Frozen,
        // flee/knockback, and lunge above still run so item effects read clearly.)
        if (dummy) return;

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
        BattleAgent target = manager.NearestEnemyOf(this, t.aggroRange);

        // Enemies will also attack the player's leader when it is the nearest target.
        if (Team == Team.Enemy)
        {
            LeaderCombatant lead = manager.LeaderUnit;
            if (lead != null && lead.IsAlive && leader != null)
            {
                float dLead = Vector2.Distance(transform.position, leader.position);
                float dAgent = target != null ? Vector2.Distance(transform.position, target.transform.position) : float.MaxValue;
                if (dLead <= t.aggroRange && dLead <= dAgent) { EngageLeader(lead); return; }
            }
        }

        if (target != null) { EngageTarget(target); return; }

        if (Team == Team.Player && leader != null)
        {
            if (Vector2.Distance(transform.position, leader.position) > FollowStopDistance)
                MoveToward(leader.position);
        }
        // Enemy idle: hold position.
    }

    private void Animate()
    {
        if (sprite == null || animFrames == null || animFrames.Length < 2) return;
        animTimer += Time.deltaTime;
        float spf = 1f / AnimFps;
        while (animTimer >= spf) { animTimer -= spf; animIdx++; }
        sprite.sprite = animFrames[animIdx % animFrames.Length];
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
        float reach = range == AttackRange.Ranged ? t.rangedReach : t.meleeReach;
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
                if (lastAttackTarget == target) consecutiveHits = Mathf.Min(consecutiveHits + 1, t.bloodlustMaxStacks);
                else { lastAttackTarget = target; consecutiveHits = 1; }
                float bonus = 1f + (consecutiveHits - 1) * t.bloodlustPerHitBonus;
                outgoing = Mathf.RoundToInt(attack * bonus);
            }

            target.TakeDamage(outgoing, this);

            // Spitter — Corrosion debuff on hit.
            if (passive == Passive.Corrosion && target.IsAlive)
                target.ApplyCorrosion(t.corrosionDuration, t.corrosionExtraDamage);

            // Visible attack: melee units lunge into the target so combat reads clearly.
            if (range == AttackRange.Melee)
            {
                lungeDir = ((Vector2)(target.transform.position - transform.position)).normalized;
                lungeTimer = LungeDuration;
            }

            attackTimer = t.attackInterval;
        }
    }

    /// Enemy attacking the player's leader (mirrors EngageTarget but the target has no BattleAgent).
    private void EngageLeader(LeaderCombatant lead)
    {
        float reach = range == AttackRange.Ranged ? t.rangedReach : t.meleeReach;
        float dist = Vector2.Distance(transform.position, leader.position);
        if (dist > reach) MoveToward(leader.position);
        else if (attackTimer <= 0f)
        {
            FaceToward(leader.position);
            lead.TakeDamage(attack);
            if (range == AttackRange.Melee)
            {
                lungeDir = ((Vector2)(leader.position - transform.position)).normalized;
                lungeTimer = LungeDuration;
            }
            attackTimer = t.attackInterval;
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

        // Hunger vulnerability — a unit deployed Hungry takes more (the cost of the attack bonus).
        if (damageTakenMultiplier > 1f)
            finalAmount = Mathf.RoundToInt(finalAmount * damageTakenMultiplier);

        // Brute ThickHide.
        if (passive == Passive.ThickHide)
            finalAmount = Mathf.Max(0, finalAmount - t.thickHideReduction);

        // Runner Evasion.
        if (passive == Passive.Evasion && Random.value < t.evasionChance)
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
        auraTimer = t.auraTickInterval;

        var allies = Team == Team.Player ? manager.Players : manager.Enemies;
        foreach (BattleAgent a in allies)
        {
            if (a == null || a == this || !a.IsAlive) continue;
            if (Vector2.Distance(a.transform.position, transform.position) <= t.auraRadius)
                a.Heal(t.auraHealAmount);
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
            // Snapshot the foe list: a detonate kill runs OnAgentDied, which removes from the
            // manager's live list — iterating that list directly would throw "Collection modified".
            var foes = new List<BattleAgent>(Team == Team.Player ? manager.Enemies : manager.Players);
            foreach (BattleAgent f in foes)
            {
                if (f == null || !f.IsAlive) continue;
                if (Vector2.Distance(f.transform.position, transform.position) <= t.detonateRadius)
                    f.TakeDamage(t.detonateDamage, this);
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
