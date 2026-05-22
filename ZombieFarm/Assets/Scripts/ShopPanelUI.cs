using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private ShopController shop;
    [SerializeField] private Wallet wallet;
    [SerializeField] private SeedInventory seedInventory;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform rowParent;

    private readonly List<Card> cards = new List<Card>();

    private struct Card
    {
        public string id;
        public int price;
        public string seedName;
        public GameObject root;
        public Button buyBtn;
        public TMP_Text nameLabel;
        public TMP_Text priceLabel;
        public TMP_Text stockLabel;
    }

    private void Awake()
    {
        if (shop == null) shop = GetComponentInParent<ShopController>() ?? Object.FindFirstObjectByType<ShopController>();
        if (wallet == null) wallet = Object.FindFirstObjectByType<Wallet>();
        if (seedInventory == null) seedInventory = Object.FindFirstObjectByType<SeedInventory>();
        if (rowParent == null)
        {
            var grid = GetComponentInChildren<GridLayoutGroup>(true);
            if (grid != null) rowParent = grid.transform;
        }
        Build();
    }

    private void OnEnable()
    {
        if (wallet != null) wallet.Changed += Refresh;
        if (seedInventory != null) seedInventory.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (wallet != null) wallet.Changed -= Refresh;
        if (seedInventory != null) seedInventory.Changed -= Refresh;
    }

    public void Open() => Refresh();

    public void Close() { }

    private void Build()
    {
        if (config == null || rowParent == null) return;

        // Destroy existing card GameObjects (both tracked and leftover) to avoid duplicates
        foreach (Card c in cards)
            if (c.root != null) Destroy(c.root);
        cards.Clear();
        for (int i = rowParent.childCount - 1; i >= 0; i--)
            Destroy(rowParent.GetChild(i).gameObject);

        foreach (GameConfig.ShopEntry e in config.seedCatalog)
        {
            if (e.seed == null) continue;
            var card = BuildCard(e);
            cards.Add(card);
        }
        Refresh();
    }

    private Card BuildCard(GameConfig.ShopEntry entry)
    {
        TMP_FontAsset bodyFont = TMP_Settings.defaultFontAsset;
        TMP_FontAsset titleFont = bodyFont;

        var go = new GameObject("Card_" + entry.seed.id);
        go.transform.SetParent(rowParent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(270, 155);

        // Background panel
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.10f, 0.16f, 1f);

        // Seed icon background
        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        var iconRT = iconGo.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.03f, 0.58f);
        iconRT.anchorMax = new Vector2(0.28f, 0.93f);
        iconRT.sizeDelta = Vector2.zero;
        var iconBg = iconGo.AddComponent<Image>();
        iconBg.color = new Color(0.15f, 0.18f, 0.28f, 1f);

        // Name label - larger, uses title font
        var nameGo = new GameObject("Name");
        nameGo.transform.SetParent(go.transform, false);
        var nameRT = nameGo.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.32f, 0.60f);
        nameRT.anchorMax = new Vector2(0.98f, 0.93f);
        nameRT.sizeDelta = Vector2.zero;
        var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text = entry.seed.displayName;
        nameTmp.fontSize = 22;
        nameTmp.color = new Color(1f, 0.95f, 0.75f);
        nameTmp.alignment = TextAlignmentOptions.Left;
        nameTmp.font = titleFont;

        // Price label - warm gold
        var priceGo = new GameObject("Price");
        priceGo.transform.SetParent(go.transform, false);
        var priceRT = priceGo.AddComponent<RectTransform>();
        priceRT.anchorMin = new Vector2(0.32f, 0.35f);
        priceRT.anchorMax = new Vector2(0.98f, 0.55f);
        priceRT.sizeDelta = Vector2.zero;
        var priceTmp = priceGo.AddComponent<TextMeshProUGUI>();
        priceTmp.text = $"{entry.price} res";
        priceTmp.fontSize = 15;
        priceTmp.color = new Color(0.95f, 0.82f, 0.35f);
        priceTmp.alignment = TextAlignmentOptions.Left;
        priceTmp.font = bodyFont;

        // Stock label - in the icon area
        var stockGo = new GameObject("Stock");
        stockGo.transform.SetParent(go.transform, false);
        var stockRT = stockGo.AddComponent<RectTransform>();
        stockRT.anchorMin = new Vector2(0.03f, 0.35f);
        stockRT.anchorMax = new Vector2(0.28f, 0.55f);
        stockRT.sizeDelta = Vector2.zero;
        var stockTmp = stockGo.AddComponent<TextMeshProUGUI>();
        stockTmp.text = "x0";
        stockTmp.fontSize = 16;
        stockTmp.color = new Color(0.55f, 0.85f, 0.55f);
        stockTmp.alignment = TextAlignmentOptions.Center;
        stockTmp.font = bodyFont;
        stockTmp.fontStyle = FontStyles.Bold;

        // Buy button - gold/warm color
        var btnGo = new GameObject("BuyBtn");
        btnGo.transform.SetParent(go.transform, false);
        var btnRT = btnGo.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.08f, 0.04f);
        btnRT.anchorMax = new Vector2(0.92f, 0.28f);
        btnRT.sizeDelta = Vector2.zero;
        var btnImg = btnGo.AddComponent<Image>();
        var btn = btnGo.AddComponent<Button>();
        btnImg.color = new Color(0.22f, 0.38f, 0.18f);
        var cb = btn.colors;
        cb.normalColor = new Color(0.22f, 0.38f, 0.18f);
        cb.highlightedColor = new Color(0.32f, 0.55f, 0.25f);
        cb.pressedColor = new Color(0.12f, 0.22f, 0.10f);
        cb.disabledColor = new Color(0.15f, 0.15f, 0.15f);
        cb.selectedColor = cb.normalColor;
        btn.colors = cb;

        var btnLabel = new GameObject("Label");
        btnLabel.transform.SetParent(btnGo.transform, false);
        var blRT = btnLabel.AddComponent<RectTransform>();
        blRT.anchorMin = Vector2.zero;
        blRT.anchorMax = Vector2.one;
        blRT.sizeDelta = Vector2.zero;
        var blTmp = btnLabel.AddComponent<TextMeshProUGUI>();
        blTmp.text = "BUY";
        blTmp.fontSize = 17;
        blTmp.color = Color.white;
        blTmp.alignment = TextAlignmentOptions.Center;
        blTmp.font = titleFont;
        blTmp.fontStyle = FontStyles.Bold;

        string id = entry.seed.id;
        btn.onClick.AddListener(() => shop.Buy(id));

        return new Card
        {
            id = id,
            price = entry.price,
            seedName = entry.seed.displayName,
            root = go,
            buyBtn = btn,
            nameLabel = nameTmp,
            priceLabel = priceTmp,
            stockLabel = stockTmp
        };
    }

    private void Refresh()
    {
        foreach (Card c in cards)
        {
            int owned = seedInventory != null ? seedInventory.Get(c.id) : 0;
            if (c.stockLabel != null)
                c.stockLabel.text = $"Owned: {owned}";
            if (c.buyBtn != null)
                c.buyBtn.interactable = wallet != null && wallet.Resources >= c.price;
        }
    }
}
