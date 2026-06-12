using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Operability layer on top of the real-time battle: mouse selection, right-click commands,
/// and field-control items (Rotten Onion + Freeze Canister). All UI lives in the scene as a
/// hand-built hierarchy (built by BattleSceneSetup) so you can restyle it in the editor — only
/// the per-squad rows are cloned at runtime from `squadRowTemplate`. Same pattern as the farm
/// panels.
public class BattleCommandController : MonoBehaviour
{
    private enum Item { None, Onion, Freeze }

    [Header("Refs")]
    [SerializeField] private Camera battleCamera;
    [SerializeField] private BattleManager manager;
    [SerializeField] private Canvas canvas;
    [SerializeField] private LeaderCombat leaderCombat; // action-brawler: left-click swings the hero

    [Header("UI (wired by the setup menu, editable in the scene)")]
    [SerializeField] private RectTransform dragBox;
    [SerializeField] private TMP_Text onionLabel;
    [SerializeField] private TMP_Text freezeLabel;
    [SerializeField] private GameObject targetingHint;
    [SerializeField] private TMP_Text targetingHintLabel; // child of targetingHint, retext-ed per item
    [SerializeField] private RectTransform squadHudParent;
    [SerializeField] private RectTransform squadRowTemplate;

    [Header("Rotten Onion (key 1)")]
    [SerializeField] private int onionsAvailable = 3;
    [SerializeField] private float onionRadius = 2.5f;
    [SerializeField] private float onionRepelDistance = 3f;
    [SerializeField] private float onionRepelDuration = 2f;
    private static readonly Color OnionColor = new Color(0.95f, 0.85f, 0.20f, 0.55f);
    private static readonly Color OnionBlastColor = new Color(0.95f, 0.85f, 0.20f, 0.70f);

    [Header("Freeze Canister (key 2)")]
    [SerializeField] private int freezesAvailable = 2;
    [SerializeField] private float freezeRadius = 2.5f;
    [SerializeField] private float freezeDuration = 1.5f;
    private static readonly Color FreezeColor = new Color(0.45f, 0.80f, 1f, 0.55f);
    private static readonly Color FreezeBlastColor = new Color(0.45f, 0.80f, 1f, 0.70f);

    [Header("Selection")]
    [SerializeField] private float clickAgentRadius = 0.8f;
    [SerializeField] private float dragStartThresholdPixels = 14f;

    private readonly HashSet<BattleAgent> selected = new HashSet<BattleAgent>();
    private bool leftPressing;
    private Vector2 leftPressScreen;
    private bool dragging;
    private Item currentItem = Item.None;

    /// True while the player is aiming an item (Onion / Freeze). The pause menu uses this to
    /// know not to grab Esc when the controller already uses it to cancel targeting.
    public bool IsTargeting => currentItem != Item.None;

    /// Hooked to the HUD item buttons (same as pressing 1 / 2).
    public void ToggleOnion() => ToggleItem(Item.Onion);
    public void ToggleFreeze() => ToggleItem(Item.Freeze);
    public int OnionsLeft => onionsAvailable;
    public int FreezesLeft => freezesAvailable;

    /// How many times the player has issued a whole-squad right-click order. Read by the tutorial
    /// to advance the "right-click to command your squad" step.
    public int CommandCount { get; private set; }

    /// How many thrown items actually landed on at least one enemy. The tutorial requires a HIT
    /// (not just a throw) to clear the item step.
    public int ItemsHitCount { get; private set; }

    private readonly List<HudEntry> squadHud = new List<HudEntry>();
    private bool squadHudBuilt;

    private SpriteRenderer targeter;
    private static Sprite discSprite;

    private struct HudEntry
    {
        public BattleAgent agent;
        public TMP_Text label;
        public RectTransform fill;
        public Image fillImage;
    }

    private void Awake()
    {
        if (battleCamera == null) battleCamera = Camera.main;
        if (manager == null) manager = FindFirstObjectByType<BattleManager>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (leaderCombat == null) leaderCombat = FindFirstObjectByType<LeaderCombat>();

        if (dragBox == null || onionLabel == null || squadHudParent == null || squadRowTemplate == null)
            Debug.LogWarning("[BattleCommandController] UI not wired. Re-run Setup Battle Scene.");

        if (squadRowTemplate != null) squadRowTemplate.gameObject.SetActive(false);
        if (dragBox != null) dragBox.gameObject.SetActive(false);
        if (targetingHint != null) targetingHint.SetActive(false);

        // A real deployment overrides the inspector default with the count the player actually
        // bought + carried in; a standalone test scene (no deployment) keeps the serialized value.
        if (BattleHandoff.HasDeployment)
        {
            onionsAvailable = Mathf.Max(0, BattleHandoff.OnionsCarried);
            freezesAvailable = Mathf.Max(0, BattleHandoff.FreezesCarried);
            BattleHandoff.OnionsUsed = 0;
            BattleHandoff.FreezesUsed = 0;
        }

        BuildTargeter();
        UpdateOnionLabel();
        UpdateFreezeLabel();
    }

    private void Update()
    {
        if (manager == null) return;
        if (battleCamera == null) battleCamera = Camera.main;
        if (!squadHudBuilt && manager.Players.Count > 0) BuildSquadHudEntries();

        UpdateInput();
        UpdateTargeter();
        RefreshSquadHud();
    }

    // --- input ------------------------------------------------------------------------------

    private void UpdateInput()
    {
        Mouse mouse = Mouse.current;
        Keyboard kb = Keyboard.current;
        if (mouse == null) return;

        if (kb != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) ToggleItem(Item.Onion);
            if (kb.digit2Key.wasPressedThisFrame) ToggleItem(Item.Freeze);
            if (kb.escapeKey.wasPressedThisFrame && currentItem != Item.None) SetItem(Item.None);
        }

        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // Left-click: throw the active item, otherwise swing the leader's melee toward the cursor.
        if (mouse.leftButton.wasPressedThisFrame && !overUi)
        {
            if (currentItem != Item.None) ThrowAt(MouseWorld());
            else if (leaderCombat != null) leaderCombat.TrySwing(MouseWorld());
        }

        // Right-click: cancel item aiming, else send the WHOLE squad at once (no select step) —
        // focus the enemy under the cursor, or move there if it's empty ground.
        if (mouse.rightButton.wasPressedThisFrame && !overUi)
        {
            if (currentItem != Item.None) SetItem(Item.None);
            else CommandAll(MouseWorld());
        }
    }

    private Vector3 MouseWorld()
    {
        if (battleCamera == null) return Vector3.zero;
        Vector2 sp = Mouse.current.position.ReadValue();
        Vector3 wp = battleCamera.ScreenToWorldPoint(new Vector3(sp.x, sp.y, -battleCamera.transform.position.z));
        wp.z = 0f;
        return wp;
    }

    // --- selection --------------------------------------------------------------------------

    private void ApplyClickSelect()
    {
        Vector3 worldPos = MouseWorld();
        BattleAgent best = NearestAgentIn(manager.Players, worldPos, clickAgentRadius);
        ClearSelection();
        if (best != null) { selected.Add(best); best.SetSelected(true); }
    }

    private void ApplyDragSelect(Vector2 startScreen, Vector2 endScreen)
    {
        if (battleCamera == null) return;
        Vector3 a = battleCamera.ScreenToWorldPoint(new Vector3(startScreen.x, startScreen.y, -battleCamera.transform.position.z));
        Vector3 b = battleCamera.ScreenToWorldPoint(new Vector3(endScreen.x, endScreen.y, -battleCamera.transform.position.z));
        float xmin = Mathf.Min(a.x, b.x), xmax = Mathf.Max(a.x, b.x);
        float ymin = Mathf.Min(a.y, b.y), ymax = Mathf.Max(a.y, b.y);

        ClearSelection();
        foreach (BattleAgent p in manager.Players)
        {
            if (p == null || !p.IsAlive) continue;
            Vector3 pos = p.transform.position;
            if (pos.x >= xmin && pos.x <= xmax && pos.y >= ymin && pos.y <= ymax)
            { selected.Add(p); p.SetSelected(true); }
        }
    }

    private void ClearSelection()
    {
        foreach (BattleAgent p in selected) if (p != null) p.SetSelected(false);
        selected.Clear();
    }

    private void ApplyCommand(Vector3 worldPos)
    {
        if (selected.Count == 0) return;
        BattleAgent enemyAtClick = NearestAgentIn(manager.Enemies, worldPos, clickAgentRadius);
        foreach (BattleAgent p in selected)
        {
            if (p == null || !p.IsAlive) continue;
            if (enemyAtClick != null) p.SetCommandTarget(enemyAtClick);
            else p.SetMoveCommand(worldPos);
        }
    }

    /// Send the entire living squad to focus the enemy under the cursor, or move to the point.
    /// No prior selection needed — the action-brawler scheme commands everyone together.
    private void CommandAll(Vector3 worldPos)
    {
        if (manager == null) return;
        BattleAgent enemyAtClick = NearestAgentIn(manager.Enemies, worldPos, clickAgentRadius * 1.6f);
        bool issued = false;
        foreach (BattleAgent p in manager.Players)
        {
            if (p == null || !p.IsAlive) continue;
            if (enemyAtClick != null) p.SetCommandTarget(enemyAtClick);
            else p.SetMoveCommand(worldPos);
            issued = true;
        }
        if (issued) CommandCount++;
    }

    private static BattleAgent NearestAgentIn(IReadOnlyList<BattleAgent> list, Vector3 worldPos, float radius)
    {
        BattleAgent best = null;
        float bestSqr = radius * radius;
        foreach (BattleAgent a in list)
        {
            if (a == null || !a.IsAlive) continue;
            float d = ((Vector2)(a.transform.position - worldPos)).sqrMagnitude;
            if (d <= bestSqr) { bestSqr = d; best = a; }
        }
        return best;
    }

    // --- items (Rotten Onion / Freeze Canister) -------------------------------------------

    private int CountFor(Item item) => item == Item.Onion ? onionsAvailable
                                     : item == Item.Freeze ? freezesAvailable
                                     : 0;

    private float RadiusFor(Item item) => item == Item.Onion ? onionRadius
                                        : item == Item.Freeze ? freezeRadius
                                        : 0f;

    private void ToggleItem(Item item)
    {
        if (currentItem == item) { SetItem(Item.None); return; }
        if (CountFor(item) <= 0) return;
        SetItem(item);
    }

    private void SetItem(Item item)
    {
        currentItem = item;
        bool active = item != Item.None;

        if (targeter != null)
        {
            targeter.enabled = active;
            if (active)
            {
                targeter.color = item == Item.Onion ? OnionColor : FreezeColor;
                targeter.transform.localScale = Vector3.one * (RadiusFor(item) * 2f);
            }
        }

        if (targetingHint != null) targetingHint.SetActive(active);
        if (active && targetingHintLabel != null)
            targetingHintLabel.text = item == Item.Onion
                ? "Throwing Rotten Onion — left-click to throw, Esc / right-click to cancel"
                : "Throwing Freeze Canister — left-click to throw, Esc / right-click to cancel";
    }

    private void UpdateTargeter()
    {
        if (currentItem == Item.None || targeter == null) return;
        targeter.transform.position = MouseWorld();
    }

    private void ThrowAt(Vector3 worldPos)
    {
        SfxManager.Play(SfxKind.ItemThrow);
        switch (currentItem)
        {
            case Item.Onion:
                if (onionsAvailable <= 0) return;
                onionsAvailable--;
                BattleHandoff.OnionsUsed++; // consumed on return (BattleResultApplier removes it)
                int onionHits = 0;
                foreach (BattleAgent e in manager.Enemies)
                {
                    if (e == null || !e.IsAlive) continue;
                    if (((Vector2)(e.transform.position - worldPos)).sqrMagnitude <= onionRadius * onionRadius)
                        { e.Repel(worldPos, onionRepelDistance, onionRepelDuration); onionHits++; }
                }
                if (onionHits > 0) ItemsHitCount++;
                StartCoroutine(BlastEffect(worldPos, onionRadius, OnionBlastColor));
                UpdateOnionLabel();
                if (onionsAvailable <= 0) SetItem(Item.None);
                break;

            case Item.Freeze:
                if (freezesAvailable <= 0) return;
                freezesAvailable--;
                BattleHandoff.FreezesUsed++; // consumed on return (BattleResultApplier removes it)
                int freezeHits = 0;
                foreach (BattleAgent e in manager.Enemies)
                {
                    if (e == null || !e.IsAlive) continue;
                    if (((Vector2)(e.transform.position - worldPos)).sqrMagnitude <= freezeRadius * freezeRadius)
                        { e.Freeze(freezeDuration); freezeHits++; }
                }
                if (freezeHits > 0) ItemsHitCount++;
                StartCoroutine(BlastEffect(worldPos, freezeRadius, FreezeBlastColor));
                UpdateFreezeLabel();
                if (freezesAvailable <= 0) SetItem(Item.None);
                break;
        }
    }

    private IEnumerator BlastEffect(Vector3 pos, float radius, Color color)
    {
        var go = new GameObject("ItemBlast");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * (radius * 2f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = EnsureDisc();
        sr.color = color;
        sr.sortingOrder = 10;
        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            sr.color = new Color(color.r, color.g, color.b, color.a * (1f - t / dur));
            yield return null;
        }
        Destroy(go);
    }

    private void BuildTargeter()
    {
        var go = new GameObject("ItemTargeter");
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one * (onionRadius * 2f);
        targeter = go.AddComponent<SpriteRenderer>();
        targeter.sprite = EnsureDisc();
        targeter.color = OnionColor;
        targeter.sortingOrder = 9;
        targeter.enabled = false;
    }

    // --- squad HUD --------------------------------------------------------------------------

    private void BuildSquadHudEntries()
    {
        if (squadHudParent == null || squadRowTemplate == null) return;
        squadHudBuilt = true;

        foreach (BattleAgent p in manager.Players)
        {
            if (p == null) continue;

            var rowGo = Instantiate(squadRowTemplate.gameObject, squadHudParent);
            rowGo.SetActive(true);
            rowGo.name = $"Hud_{p.DisplayName}";

            TMP_Text label = rowGo.transform.Find("Name")?.GetComponent<TMP_Text>();
            Transform fillTf = rowGo.transform.Find("HpBg/HpFill");
            RectTransform fill = fillTf as RectTransform;
            Image fillImage = fill != null ? fill.GetComponent<Image>() : null;

            if (label != null) label.text = p.DisplayName + (p.IsHungry ? "  (Hungry)" : "");

            // Portrait (optional child on the card template).
            Image portrait = rowGo.transform.Find("Portrait")?.GetComponent<Image>();
            if (portrait != null)
            {
                if (p.Portrait != null) { portrait.sprite = p.Portrait; portrait.enabled = true; }
                else portrait.enabled = false;
            }

            squadHud.Add(new HudEntry { agent = p, label = label, fill = fill, fillImage = fillImage });
        }
    }

    private void RefreshSquadHud()
    {
        foreach (HudEntry h in squadHud)
        {
            bool alive = h.agent != null && h.agent.IsAlive;
            if (h.label != null)
                h.label.color = !alive ? new Color(0.55f, 0.55f, 0.55f)
                              : (h.agent.IsHungry ? new Color(1f, 0.55f, 0.25f) : Color.white);

            float frac = (h.agent != null && h.agent.MaxHp > 0)
                ? Mathf.Clamp01((float)h.agent.Hp / h.agent.MaxHp) : 0f;
            if (h.fill != null)
            {
                Vector2 max = h.fill.anchorMax;
                max.x = frac;
                h.fill.anchorMax = max;
            }
            if (h.fillImage != null)
                h.fillImage.color = alive
                    ? new Color(0.42f, 0.85f, 0.42f)
                    : new Color(0.45f, 0.18f, 0.18f);
        }
    }

    private void UpdateDragBoxVisual(Vector2 currentScreen)
    {
        if (dragBox == null) return;
        Vector2 a = ScreenToCanvas(leftPressScreen);
        Vector2 b = ScreenToCanvas(currentScreen);
        Vector2 min = Vector2.Min(a, b);
        Vector2 max = Vector2.Max(a, b);
        dragBox.anchoredPosition = min;
        dragBox.sizeDelta = max - min;
    }

    /// Convert a screen-space point into the canvas's bottom-left-origin local space (accounts for
    /// the CanvasScaler) so the drag rectangle tracks the cursor exactly at any resolution.
    private Vector2 ScreenToCanvas(Vector2 screen)
    {
        var rt = canvas != null ? canvas.transform as RectTransform : null;
        if (rt == null) return screen;
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screen, uiCam, out Vector2 local);
        return local + rt.rect.size * 0.5f;
    }

    private void UpdateOnionLabel()
    {
        if (onionLabel != null) onionLabel.text = $"×{onionsAvailable}";
    }

    private void UpdateFreezeLabel()
    {
        if (freezeLabel != null) freezeLabel.text = $"×{freezesAvailable}";
    }

    private static Sprite EnsureDisc()
    {
        if (discSprite != null) return discSprite;
        const int size = 64;
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
}
