using System.Collections.Generic;
using UnityEngine;

/// Keeps wandering FarmRoamer visuals in sync with the harvested-zombie Inventory: one roamer
/// per owned zombie. Listens to Inventory.Changed and reconciles — harvesting adds a roamer,
/// deploying/selling later removes one. Reconcile is idempotent, so it also restores the
/// right roamers when a save is loaded. Visual only for now.
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
    [Tooltip("Optional. If assigned, roamers are tinted with each strain's ripe color; " +
             "otherwise they use a default zombie green.")]
    [SerializeField] private GameConfig config;
    [Tooltip("Optional placeholder sprite. A small square is generated at runtime if empty.")]
    [SerializeField] private Sprite roamerSprite;

    [Header("Roamer look & feel")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float roamerScale = 0.6f;
    [SerializeField] private int sortingOrder = 4;

    private readonly Dictionary<string, List<FarmRoamer>> roamers = new Dictionary<string, List<FarmRoamer>>();
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

    /// Make the on-farm roamer count for every strain match the inventory count.
    private void Reconcile()
    {
        if (inventory == null) return;

        var ids = new HashSet<string>(roamers.Keys);
        foreach (KeyValuePair<string, int> kv in inventory.Entries) ids.Add(kv.Key);

        foreach (string id in ids)
        {
            int want = inventory.Get(id);
            List<FarmRoamer> list = GetList(id);

            while (list.Count < want) list.Add(SpawnRoamer(id));
            while (list.Count > want)
            {
                int last = list.Count - 1;
                if (list[last] != null) Destroy(list[last].gameObject);
                list.RemoveAt(last);
            }
        }
    }

    private FarmRoamer SpawnRoamer(string id)
    {
        var go = new GameObject($"Roamer_{id}");
        go.transform.SetParent(transform, false);
        go.transform.position = RandomPointInArea();
        go.transform.localScale = Vector3.one * roamerScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = roamerSprite != null ? roamerSprite : GeneratedSquare();
        sr.color = ResolveColor(id);
        sr.sortingOrder = sortingOrder;

        var roamer = go.AddComponent<FarmRoamer>();
        roamer.Init(moveSpeed, RandomPointInArea);
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

    private List<FarmRoamer> GetList(string id)
    {
        if (!roamers.TryGetValue(id, out List<FarmRoamer> list))
        {
            list = new List<FarmRoamer>();
            roamers[id] = list;
        }
        return list;
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
