using System.Collections.Generic;
using UnityEngine;

/// Keeps wandering FarmRoamer visuals in sync with the harvested-zombie roster: exactly one
/// roamer per owned ZombieUnit, keyed by the unit's uid. Listens to Inventory.Changed and
/// reconciles — harvesting adds a roamer, deploying/selling removes one. Reconcile is
/// idempotent, so it also restores the right roamers when a save is loaded (uids are preserved).
///
/// The wander region is a Collider2D (use a PolygonCollider2D so you can freely drag its
/// vertices into your isometric farm's diamond shape via Unity's "Edit Collider" tool).
/// Roamers pick random points inside that collider.
public class FarmRoamerSpawner : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [Tooltip("Defines the wander region. A PolygonCollider2D works best — edit its shape with " +
             "the 'Edit Collider' button. If left empty, a Collider2D on this object is used.")]
    [SerializeField] private Collider2D wanderArea;
    [Tooltip("Tints each roamer with its strain's ripe color and labels it with the strain name.")]
    [SerializeField] private GameConfig config;
    [Tooltip("Optional placeholder sprite. A small square is generated at runtime if empty.")]
    [SerializeField] private Sprite roamerSprite;

    [Header("Roamer look & feel")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float roamerScale = 0.6f;
    [SerializeField] private int sortingOrder = 4;

    private readonly Dictionary<string, FarmRoamer> roamersByUid = new Dictionary<string, FarmRoamer>();
    private static Sprite generatedSquare;

    private void Awake()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (wanderArea == null) wanderArea = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.Changed += Reconcile;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.Changed -= Reconcile;
    }

    private void Start() => Reconcile();

    /// Spawn a roamer for every owned unit that lacks one, and remove roamers whose unit is gone.
    private void Reconcile()
    {
        if (inventory == null) return;

        var live = new HashSet<string>();
        foreach (ZombieUnit u in inventory.Units)
        {
            live.Add(u.uid);
            if (!roamersByUid.ContainsKey(u.uid)) roamersByUid[u.uid] = SpawnRoamer(u);
        }

        var stale = new List<string>();
        foreach (KeyValuePair<string, FarmRoamer> kv in roamersByUid)
            if (!live.Contains(kv.Key)) stale.Add(kv.Key);

        foreach (string deadUid in stale)
        {
            if (roamersByUid[deadUid] != null) Destroy(roamersByUid[deadUid].gameObject);
            roamersByUid.Remove(deadUid);
        }
    }

    private FarmRoamer SpawnRoamer(ZombieUnit unit)
    {
        var go = new GameObject($"Roamer_{unit.strainId}_{unit.uid.Substring(0, 4)}");
        go.transform.SetParent(transform, false);
        go.transform.position = RandomPointInArea();
        go.transform.localScale = Vector3.one * roamerScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = roamerSprite != null ? roamerSprite : GeneratedSquare();
        sr.color = ResolveColor(unit.strainId);
        sr.sortingOrder = sortingOrder;

        var roamer = go.AddComponent<FarmRoamer>();
        roamer.Init(moveSpeed, RandomPointInArea, unit.uid, ResolveName(unit.strainId), inventory);
        return roamer;
    }

    /// A random world point inside the wander collider (rejection-sampled within its bounds).
    private Vector3 RandomPointInArea()
    {
        if (wanderArea == null) return transform.position;

        Bounds b = wanderArea.bounds;
        for (int i = 0; i < 30; i++)
        {
            var p = new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
            if (wanderArea.OverlapPoint(p)) return new Vector3(p.x, p.y, 0f);
        }
        return b.center; // fallback if sampling kept missing (e.g. a very thin shape)
    }

    private Color ResolveColor(string id)
    {
        if (config != null)
        {
            CropData seed = config.FindSeed(id);
            if (seed != null) return seed.ripeColor;
        }
        return new Color(0.45f, 0.75f, 0.35f); // default zombie green
    }

    private string ResolveName(string id)
    {
        if (config != null)
        {
            CropData seed = config.FindSeed(id);
            if (seed != null && !string.IsNullOrEmpty(seed.displayName)) return seed.displayName;
        }
        return id;
    }

    private static Sprite GeneratedSquare()
    {
        if (generatedSquare != null) return generatedSquare;
        var tex = new Texture2D(8, 8) { filterMode = FilterMode.Point };
        var pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        generatedSquare = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        return generatedSquare;
    }
}
