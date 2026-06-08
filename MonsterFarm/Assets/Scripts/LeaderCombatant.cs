using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Gives the squad LEADER (the player-controlled avatar) hit points in battle. Enemies can target
/// and damage the leader (see BattleAgent); when the leader's HP reaches zero the raid is lost
/// (BattleManager.OnLeaderDied → defeat). Drives a top-left HP bar in the battle HUD.
public class LeaderCombatant : MonoBehaviour
{
    [SerializeField] private int maxHp = 120;

    [Header("HUD (wired by the HUD build script)")]
    [SerializeField] private Image hpFill;
    [SerializeField] private TMP_Text hpText;

    private int hp;
    private SpriteRenderer sr;
    private Color baseColor = Color.white;
    private float flashTimer;
    private BattleManager manager;

    public int Hp => hp;
    public int MaxHp => maxHp;
    public bool IsAlive => hp > 0;

    private void Awake()
    {
        hp = maxHp;
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        RefreshBar();
    }

    public void Bind(BattleManager m) => manager = m;

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        hp = Mathf.Max(0, hp - Mathf.Max(0, amount));
        if (sr != null) { sr.color = Color.white; flashTimer = 0.12f; }
        DamagePopup.Spawn(transform.position, $"-{Mathf.Max(0, amount)}", new Color(1f, 0.5f, 0.4f));
        SfxManager.Play(SfxKind.Hit);
        RefreshBar();
        if (hp <= 0)
        {
            // The hero is down — grey out, stop control, lose the raid.
            if (sr != null) { sr.color = new Color(0.45f, 0.45f, 0.5f, 0.75f); baseColor = sr.color; }
            var ac = GetComponent<AvatarController>(); if (ac != null) ac.enabled = false;
            if (manager != null) manager.OnLeaderDied();
        }
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sr != null) sr.color = baseColor;
        }
    }

    private void RefreshBar()
    {
        float frac = maxHp > 0 ? Mathf.Clamp01((float)hp / maxHp) : 0f;
        if (hpFill != null)
        {
            Vector2 max = hpFill.rectTransform.anchorMax;
            max.x = frac;
            hpFill.rectTransform.anchorMax = max;
            hpFill.color = frac > 0.3f ? new Color(0.42f, 0.85f, 0.42f) : new Color(0.88f, 0.4f, 0.3f);
        }
        if (hpText != null) hpText.text = $"{hp}/{maxHp}";
    }
}
