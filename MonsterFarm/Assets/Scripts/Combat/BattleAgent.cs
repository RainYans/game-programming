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
    /// True if this unit was deployed Hungry (stronger but more fragile). Inferred from the
    /// incoming-damage multiplier set at deploy, so the HUD can flag it.
    public bool IsHungry => damageTakenMultiplier > 1f;
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
    private const float LungeReach = 0.28f; // peak jab offset — applied to the SPRITE only, never the body
    private float lungeTimer;
    private Vector2 lungeDir;
    private Vector3? moveGoal; // physics-move target chosen in Update, applied in FixedUpdate (keeps unit separation)
    private Transform visualRoot; // child that holds the sprite, so the lunge jabs the art, not the body
    private const float ArriveEpsilon = 0.05f;
    private const float MoveArriveDistance = 0.3f;
    private const float FlashSeconds = 0.14f;
    // Collision (G10): a small dynamic body so units don't overlap each other and stop at / slide
    // along walls (the hero + walls are already physics colliders). Tune here if it feels off.
    private const float ColliderRadius = 0.28f;
    private const float BodyDrag = 12f;
    // Boids-style separation: the squad all chases the same leader point, so the physics solver alone
    // can't stop them piling up while moving. Each unit also steers away from nearby same-team units.
    // Tune here if the squad feels too spread out (lower) or still overlaps (raise radius).
    private const float SeparationRadius = 0.85f;
    private const float SeparationWeight = 1.3f;
    private Rigidbody2D body;

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
        string sourceUid, float damageMultiplier = 1f, float damageTakenMultiplier = 1f,
        float upgradeMultiplier = 1f)
    {
        manager = mgr;
        Team = team;
        leader = leaderTransform;
        SourceUid = sourceUid;
        DisplayName = data.displayName;
        t = mgr != null && mgr.Tuning != null ? mgr.Tuning : new GameConfig.CombatTuning();
        this.damageTakenMultiplier = damageTakenMultiplier;

        float upg = Mathf.Max(1f, upgradeMultiplier); // Lab upgrade scales base HP + attack
        maxHp = Mathf.Max(1, Mathf.RoundToInt(data.maxHp * upg));
        hp = maxHp;
        attack = Mathf.Max(1, Mathf.RoundToInt(data.attack * upg));
        // Hunger makes a unit hit harder (snapshotted at deploy; 1x for Full units and enemies).
        if (damageMultiplier > 1f) attack = Mathf.Max(1, Mathf.RoundToInt(attack * damageMultiplier));
        moveSpeed = Mathf.Max(0.1f, data.moveSpeed) * BattleMoveScale;
        range = data.range;
        passive = data.passive;
        auraTimer = t.auraTickInterval; // delay first tick

        // Put the visible sprite on a child ("Visual") so the melee lunge can jab the ART without
        // moving the physics body — moving the body into the target would shove both units apart.
        var rootSr = GetComponent<SpriteRenderer>();
        var visGo = new GameObject("Visual");
        visGo.transform.SetParent(transform, false);
        visualRoot = visGo.transform;
        sprite = visGo.AddComponent<SpriteRenderer>();
        if (rootSr != null)
        {
            sprite.sprite = rootSr.sprite;          // fallback look until monster art replaces it below
            sprite.sortingLayerID = rootSr.sortingLayerID;
            sprite.sortingOrder = rootSr.sortingOrder;
            rootSr.enabled = false;                 // the child renders now
        }

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

        SetupCollision();
        BuildHpBar();
        if (Team == Team.Player) BuildSelectionRing();
    }

    /// Small dynamic physics body so units don't stack on each other and respect walls.
    private void SetupCollision()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null) body = gameObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.drag = BodyDrag;
        var col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = ColliderRadius;
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

        // AI picks a move target here (Update); the actual physics move runs in FixedUpdate via
        // MovePosition. This keeps speed framerate-correct AND lets the solver push overlapping units
        // apart. (A plain velocity-set in Update overwrote the separation impulse → units stacked.)
        moveGoal = null;

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

        // Attack lunge — a VISUAL jab toward the target then back. Offsets the sprite child only;
        // the physics body never moves, so a melee hit no longer shoves units around on contact.
        if (lungeTimer > 0f)
        {
            lungeTimer -= Time.deltaTime;
            if (visualRoot != null)
            {
                if (lungeTimer <= 0f) visualRoot.localPosition = Vector3.zero; // snap back when the jab ends
                else
                {
                    float along = Mathf.Sin(Mathf.Clamp01(1f - lungeTimer / LungeDuration) * Mathf.PI); // 0→1→0
                    visualRoot.localPosition = (Vector3)(lungeDir * (LungeReach * along));
                }
            }
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

            // Visible attack tell: melee units lunge into the target; ranged units fire a projectile.
            if (range == AttackRange.Melee)
            {
                lungeDir = ((Vector2)(target.transform.position - transform.position)).normalized;
                lungeTimer = LungeDuration;
            }
            else
            {
                BattleProjectile.Spawn(transform.position, target.transform.position, sprite);
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
            else
            {
                BattleProjectile.Spawn(transform.position, leader.position, sprite);
            }
            attackTimer = t.attackInterval;
        }
    }

    private void MoveToward(Vector3 worldTarget)
    {
        Vector2 to = (Vector2)(worldTarget - transform.position);
        if (to.sqrMagnitude < ArriveEpsilon * ArriveEpsilon) return;
        Vector2 dir = to.normalized;
        if (body != null)
            moveGoal = worldTarget; // moved in FixedUpdate so overlapping units still separate
        else
            transform.position += (Vector3)(dir * (moveSpeed * Time.deltaTime));
        FaceDir(dir.x);
    }

    // Physics move runs here (not Update) so speed is framerate-correct. Combines "head to my goal"
    // with a push away from nearby allies, so the squad (which all chases the same leader point)
    // spreads out instead of stacking. Idle-but-crowded units also gently creep apart.
    private void FixedUpdate()
    {
        if (body == null || dummy || frozenTimer > 0f || lungeTimer > 0f) return;

        Vector2 dir = Vector2.zero;
        bool hasGoal = false;
        if (moveGoal != null)
        {
            Vector2 to = (Vector2)(moveGoal.Value - transform.position);
            if (to.sqrMagnitude >= ArriveEpsilon * ArriveEpsilon)
            {
                dir = to.normalized;
                dir.y *= IsoYScale;
                hasGoal = true;
            }
        }

        dir += SeparationPush() * SeparationWeight;
        if (dir.sqrMagnitude < 0.0001f) return;

        float spd = hasGoal ? moveSpeed : moveSpeed * 0.4f; // idle units only creep apart, don't sprint
        body.MovePosition(body.position + dir.normalized * (spd * Time.fixedDeltaTime));
    }

    // Sum of pushes away from same-team units within SeparationRadius (stronger the closer they are).
    private Vector2 SeparationPush()
    {
        if (manager == null) return Vector2.zero;
        var allies = Team == Team.Player ? manager.Players : manager.Enemies;
        if (allies == null) return Vector2.zero;
        Vector2 push = Vector2.zero;
        Vector2 p = transform.position;
        foreach (BattleAgent a in allies)
        {
            if (a == null || a == this || !a.IsAlive) continue;
            Vector2 d = p - (Vector2)a.transform.position;
            float dist = d.magnitude;
            if (dist > 0.0001f && dist < SeparationRadius)
                push += (d / dist) * (1f - dist / SeparationRadius);
        }
        return push;
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
