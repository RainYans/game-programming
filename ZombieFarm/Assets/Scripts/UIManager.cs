using UnityEngine;
using UnityEngine.UI;

public enum PageType { None, Shop, Battle }

/// Central page switcher. Panels are full-screen opaque GameObjects under Canvas.
/// Only one page is visible at a time. Farm input is disabled when any page is open.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPage;
    [SerializeField] private GameObject battlePage;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;
    [SerializeField] private ShopPanelUI shopPanel;
    [SerializeField] private BattlePlayer battlePlayer;
    [SerializeField] private DeployPanel deployPanel;
    [SerializeField] private CityMapPanel cityMapPanel;

    public PageType CurrentPage { get; private set; } = PageType.None;

    private void Awake()
    {
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();
        if (shopPanel == null) shopPanel = FindFirstObjectByType<ShopPanelUI>();
        if (battlePlayer == null) battlePlayer = FindFirstObjectByType<BattlePlayer>();
        if (deployPanel == null) deployPanel = FindFirstObjectByType<DeployPanel>();
        if (cityMapPanel == null) cityMapPanel = FindFirstObjectByType<CityMapPanel>();
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

    /// Dispatch a building open by type (called by AvatarInteraction when E is pressed nearby).
    public void OpenBuilding(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.Shop: OpenShop(); break;
            case BuildingType.WarCamp:
                if (cityMapPanel != null) cityMapPanel.Open();  // pick a city → deploy → battle scene
                else if (deployPanel != null) deployPanel.Open(); // fallback: straight to deploy
                else OpenBattle();                              // fallback: old in-farm battle page
                break;
            case BuildingType.Lab: OpenLab(); break;
            case BuildingType.Home: OpenHome(); break;
        }
    }

    /// Home: manual save for now (SaveManager also autosaves). A proper save/load panel comes later.
    public void OpenHome()
    {
        var save = FindFirstObjectByType<SaveManager>();
        var toast = FindFirstObjectByType<MessageToast>();
        if (save != null)
        {
            save.Save();
            if (toast != null) toast.Show("Game saved.");
        }
        else if (toast != null) toast.Show("Save system not found.");
    }

    /// Lab panel isn't built yet — show a placeholder message.
    public void OpenLab()
    {
        var toast = FindFirstObjectByType<MessageToast>();
        if (toast != null) toast.Show("Lab coming soon.");
        else Debug.Log("[UIManager] Lab coming soon.");
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

    /// Freeze/unfreeze the avatar (movement + interaction) while a full-screen page is open.
    private void SetFarmInput(bool enabled)
    {
        if (avatarMovement != null) avatarMovement.enabled = enabled;
        if (avatarInteraction != null) avatarInteraction.enabled = enabled;
    }
}
