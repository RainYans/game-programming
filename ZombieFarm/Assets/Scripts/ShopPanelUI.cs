using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Shop UI. The card grid and category tabs are cloned at runtime from real, editable TEMPLATE
/// objects in the scene (Card grid template + Tab template) — so the look is defined by inspectable
/// GameObjects, not generated from scratch in code. Buying/stock/wallet gameplay is unchanged.
public class ShopPanelUI : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private ShopController shop;
    [SerializeField] private Wallet wallet;
    [SerializeField] private SeedInventory seedInventory;
    [SerializeField] private ItemInventory itemInventory;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform rowParent;       // ItemGrid (GridLayoutGroup) holding the cards
    [SerializeField] private GameObject cardTemplate;   // editable card template, cloned per shop entry
    [SerializeField] private Transform tabBar;          // TabBar (HorizontalLayoutGroup)
    [SerializeField] private GameObject tabTemplate;    // editable tab template, cloned per category

    [Header("Skin (icon lookup + tab tint)")]
    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private Sprite tabActive;     // selected category tab background (optional)
    [SerializeField] private Sprite tabInactive;   // unselected category tab background (optional)
    [SerializeField] private IconEntry[] icons = new IconEntry[0]; // strain/item id -> harvested look

    [System.Serializable]
    public struct IconEntry { public string id; public Sprite sprite; }

    private readonly List<Card> cards = new List<Card>();
    private string activeCategory;
    private readonly List<TabRef> tabs = new List<TabRef>();
    private struct TabRef { public string cat; public Button btn; public Image img; public TMP_Text label; }

    private struct Card
    {
        public string id;
        public int price;
        public string displayName;
        public bool isItem;
        public string category;
        public ItemStore stockSource;
        public GameObject root;
        public Button buyBtn;
        public TMP_Text nameLabel;
        public TMP_Text priceLabel;
        public TMP_Text stockLabel;
    }

    private static readonly Color Ink = new Color(0.29f, 0.19f, 0.11f);
    private static readonly Color InkSoft = new Color(0.47f, 0.37f, 0.24f);
    private static readonly Color InkGold = new Color(0.62f, 0.40f, 0.10f);
    private static readonly Color Cream = new Color(0.97f, 0.93f, 0.84f);

    private void Awake()
    {
        if (shop == null) shop = GetComponentInParent<ShopController>() ?? Object.FindFirstObjectByType<ShopController>();
        if (wallet == null) wallet = Object.FindFirstObjectByType<Wallet>();
        if (seedInventory == null) seedInventory = Object.FindFirstObjectByType<SeedInventory>();
        if (itemInventory == null) itemInventory = Object.FindFirstObjectByType<ItemInventory>();
        if (cardTemplate != null) cardTemplate.SetActive(false);
        if (tabTemplate != null) tabTemplate.SetActive(false);
        Build();
    }

    private void OnEnable()
    {
        if (wallet != null) wallet.Changed += Refresh;
        if (seedInventory != null) seedInventory.Changed += Refresh;
        if (itemInventory != null) itemInventory.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (wallet != null) wallet.Changed -= Refresh;
        if (seedInventory != null) seedInventory.Changed -= Refresh;
        if (itemInventory != null) itemInventory.Changed -= Refresh;
    }

    public void Open() => Refresh();
    public void Close() { }

    private void Build()
    {
        if (config == null || rowParent == null || cardTemplate == null) return;

        foreach (Card c in cards) if (c.root != null && c.root != cardTemplate) Destroy(c.root);
        cards.Clear();
        for (int i = rowParent.childCount - 1; i >= 0; i--)
        {
            GameObject ch = rowParent.GetChild(i).gameObject;
            if (ch != cardTemplate) Destroy(ch);
        }

        foreach (GameConfig.ShopEntry e in config.seedCatalog)
        {
            if (e.seed == null) continue;
            cards.Add(MakeCard(e.seed.id, e.seed.displayName, e.price, false, "Monsters", seedInventory));
        }
        foreach (GameConfig.ItemEntry e in config.itemCatalog)
        {
            if (string.IsNullOrEmpty(e.id)) continue;
            cards.Add(MakeCard(e.id, e.displayName, e.price, true, "Items", itemInventory));
        }

        BuildTabs();
        ShowCategory(string.IsNullOrEmpty(activeCategory) && cards.Count > 0 ? cards[0].category : activeCategory);
        Refresh();
    }

    private Card MakeCard(string id, string displayName, int price, bool isItem, string category, ItemStore stockSource)
    {
        ZombieData strain = (!isItem && config != null) ? config.FindStrain(id) : null;

        GameObject go = Instantiate(cardTemplate, rowParent);
        go.name = "Card_" + id;
        go.SetActive(true);

        SetText(go, "Name", displayName);
        string sub = isItem ? "Combat Item"
            : strain != null ? (strain.role + (strain.passive != Passive.None ? " - " + PassiveLabel(strain.passive) : ""))
            : "";
        SetText(go, "Sub", sub);
        SetText(go, "Price", $"{price} res");

        Transform iconT = go.transform.Find("IconSlot/Icon");
        Image iconImg = iconT != null ? iconT.GetComponent<Image>() : null;
        if (iconImg != null)
        {
            Sprite mon = IconFor(id);
            if (mon != null) { iconImg.sprite = mon; iconImg.preserveAspect = true; iconImg.color = Color.white; iconImg.enabled = true; }
            else { iconImg.sprite = null; iconImg.color = strain != null ? strain.color : new Color(0.55f, 0.45f, 0.32f, 1f); }
        }

        TMP_Text priceLbl = Child<TMP_Text>(go, "Price");
        TMP_Text stockLbl = Child<TMP_Text>(go, "Stock");
        Button buy = Child<Button>(go, "BuyBtn");
        if (buy != null)
        {
            buy.onClick.RemoveAllListeners();
            string captured = id;
            if (isItem) buy.onClick.AddListener(() => shop.BuyItem(captured));
            else buy.onClick.AddListener(() => shop.Buy(captured));
        }

        return new Card
        {
            id = id, price = price, displayName = displayName, isItem = isItem, category = category,
            stockSource = stockSource, root = go, buyBtn = buy,
            nameLabel = Child<TMP_Text>(go, "Name"), priceLabel = priceLbl, stockLabel = stockLbl
        };
    }

    private void BuildTabs()
    {
        if (tabBar == null || tabTemplate == null) return;
        for (int i = tabBar.childCount - 1; i >= 0; i--)
        {
            GameObject ch = tabBar.GetChild(i).gameObject;
            if (ch != tabTemplate) Destroy(ch);
        }
        tabs.Clear();

        var cats = new List<string>();
        foreach (Card c in cards) if (!cats.Contains(c.category)) cats.Add(c.category);

        foreach (string cat in cats)
        {
            GameObject go = Instantiate(tabTemplate, tabBar);
            go.name = "Tab_" + cat;
            go.SetActive(true);
            Image img = go.GetComponent<Image>();
            Button btn = go.GetComponent<Button>();
            TMP_Text label = go.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = cat;
            string captured = cat;
            if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => ShowCategory(captured)); }
            tabs.Add(new TabRef { cat = cat, btn = btn, img = img, label = label });
        }
    }

    private void ShowCategory(string cat)
    {
        if (string.IsNullOrEmpty(cat)) return;
        activeCategory = cat;
        foreach (Card c in cards)
            if (c.root != null) c.root.SetActive(c.category == cat);
        foreach (TabRef t in tabs)
        {
            bool on = t.cat == cat;
            if (t.img != null && tabActive != null && tabInactive != null)
            { t.img.sprite = on ? tabActive : tabInactive; t.img.type = Image.Type.Sliced; t.img.pixelsPerUnitMultiplier = 6f; }
            else if (t.img != null) t.img.color = on ? new Color(0.85f, 0.62f, 0.25f, 1f) : new Color(0.55f, 0.42f, 0.27f, 1f);
            if (t.label != null) t.label.color = on ? Cream : new Color(0.93f, 0.88f, 0.78f, 0.7f);
        }
    }

    private Sprite IconFor(string id)
    {
        if (icons != null)
            foreach (IconEntry e in icons)
                if (e.id == id) return e.sprite;
        return null;
    }

    private static string PassiveLabel(Passive p)
    {
        switch (p)
        {
            case Passive.ThickHide: return "Thick Hide";
            case Passive.Bloodlust: return "Bloodlust";
            case Passive.Evasion: return "Evasion";
            case Passive.Corrosion: return "Corrosion";
            case Passive.Aura: return "Aura";
            case Passive.SelfDetonate: return "Self-Detonate";
            default: return "";
        }
    }

    private static T Child<T>(GameObject card, string child) where T : Component
    {
        Transform t = card.transform.Find(child);
        return t != null ? t.GetComponent<T>() : null;
    }
    private static void SetText(GameObject card, string child, string text)
    {
        TMP_Text l = Child<TMP_Text>(card, child);
        if (l != null) l.text = text;
    }

    private void Refresh()
    {
        foreach (Card c in cards)
        {
            int owned = c.stockSource != null ? c.stockSource.Get(c.id) : 0;
            if (c.stockLabel != null) c.stockLabel.text = $"Owned: {owned}";
            if (c.buyBtn != null) c.buyBtn.interactable = wallet != null && wallet.Resources >= c.price;
        }
    }
}
