using UnityEngine;
using UnityEngine.UI;

public enum PageType { None, Shop, Battle }

/// Central page switcher. Panels are full-screen opaque GameObjects under Canvas.
/// Only one page is visible at a time. Farm input is disabled when any page is open.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPage;
    [SerializeField] private GameObject battlePage;
    [SerializeField] private TileInteraction farmInput;
    [SerializeField] private ShopPanelUI shopPanel;
    [SerializeField] private BattlePlayer battlePlayer;

    public PageType CurrentPage { get; private set; } = PageType.None;

    private void Awake()
    {
        if (farmInput == null) farmInput = FindFirstObjectByType<TileInteraction>();
        if (shopPanel == null) shopPanel = FindFirstObjectByType<ShopPanelUI>();
        if (battlePlayer == null) battlePlayer = FindFirstObjectByType<BattlePlayer>();
        CloseAll();
        WireButtons();
    }

    private void WireButtons()
    {
        var shopCloseBtn = transform.Find("ShopPage/Frame/CloseBtn")?.GetComponent<Button>();
        if (shopCloseBtn != null) shopCloseBtn.onClick.AddListener(CloseAll);

        var battleCloseBtn = transform.Find("BattlePage/Frame/CloseBtn")?.GetComponent<Button>();
        if (battleCloseBtn != null) battleCloseBtn.onClick.AddListener(CloseAll);

        var shopBtn = transform.Find("BottomBar/ShopBtn")?.GetComponent<Button>();
        if (shopBtn != null) shopBtn.onClick.AddListener(ToggleShop);

        var battleBtn = transform.Find("BottomBar/BattleBtn")?.GetComponent<Button>();
        if (battleBtn != null) battleBtn.onClick.AddListener(ToggleBattle);

        var deployBtn = transform.Find("BattlePage/Frame/DeployBtn")?.GetComponent<Button>();
        if (deployBtn != null)
        {
            var dc = FindFirstObjectByType<DeployController>();
            if (dc != null) deployBtn.onClick.AddListener(dc.Deploy);
        }
    }

    public void OpenShop()
    {
        CloseAll();
        SetPageActive(shopPage, true);
        shopPanel?.Open();
        SetFarmInput(false);
        CurrentPage = PageType.Shop;
    }

    public void OpenBattle()
    {
        CloseAll();
        SetPageActive(battlePage, true);
        SetFarmInput(false);
        CurrentPage = PageType.Battle;
    }

    public void CloseAll()
    {
        SetPageActive(shopPage, false);
        SetPageActive(battlePage, false);
        shopPanel?.Close();
        SetFarmInput(true);
        CurrentPage = PageType.None;
    }

    public void ToggleShop()
    {
        if (CurrentPage == PageType.Shop) CloseAll();
        else OpenShop();
    }

    public void ToggleBattle()
    {
        if (CurrentPage == PageType.Battle) CloseAll();
        else OpenBattle();
    }

    private void SetPageActive(GameObject page, bool active)
    {
        if (page != null) page.SetActive(active);
    }

    private void SetFarmInput(bool enabled)
    {
        if (farmInput != null) farmInput.enabled = enabled;
    }
}
