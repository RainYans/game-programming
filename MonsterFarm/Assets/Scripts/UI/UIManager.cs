using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PageType { None, Shop }

/// Central page switcher. Panels are full-screen opaque GameObjects under Canvas.
/// Only one page is visible at a time. Farm input is disabled when any page is open.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPage;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;
    [SerializeField] private ShopPanelUI shopPanel;
    [SerializeField] private DeployPanel deployPanel;
    [SerializeField] private CityMapPanel cityMapPanel;

    public PageType CurrentPage { get; private set; } = PageType.None;

    private void Awake()
    {
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();
        if (shopPanel == null) shopPanel = FindFirstObjectByType<ShopPanelUI>();
        if (deployPanel == null) deployPanel = FindFirstObjectByType<DeployPanel>();
        if (cityMapPanel == null) cityMapPanel = FindFirstObjectByType<CityMapPanel>();
        CloseAll();
        WireButtons();
    }

    private void WireButtons()
    {
        var shopCloseBtn = transform.Find("ShopPage/Frame/CloseBtn")?.GetComponent<Button>();
        if (shopCloseBtn != null) shopCloseBtn.onClick.AddListener(CloseAll);

        var shopBtn = transform.Find("BottomBar/ShopBtn")?.GetComponent<Button>();
        if (shopBtn != null) shopBtn.onClick.AddListener(ToggleShop);

        // The HUD "battle" button now opens the city map (the in-farm battle page is retired).
        var battleBtn = transform.Find("BottomBar/BattleBtn")?.GetComponent<Button>();
        if (battleBtn != null && cityMapPanel != null) battleBtn.onClick.AddListener(cityMapPanel.Open);
    }

    /// Dispatch a building open by type (called by AvatarInteraction when E is pressed nearby).
    public void OpenBuilding(BuildingType type)
    {
        SfxManager.Play(SfxKind.ButtonClick);
        switch (type)
        {
            case BuildingType.Shop: OpenShop(); break;
            case BuildingType.WarCamp:
                if (!TutorialState.BattleTutorialDone)
                {
                    // The first-ever expedition routes into the dedicated combat tutorial scene
                    // (fixed starter squad, no deploy step). Clearing it marks the tutorial done so
                    // every later raid goes through the normal city map → deploy → battle flow.
                    BattleHandoff.ClearDeployment();
                    BattleHandoff.ClearResult();
                    TutorialState.FarmOnboardDone = true; // reaching the raid finishes farm onboarding
                    SceneManager.LoadScene(TutorialState.TutorialSceneName);
                }
                else if (cityMapPanel != null) cityMapPanel.Open();    // pick a city → deploy → battle
                else if (deployPanel != null) deployPanel.Open();      // fallback: straight to deploy
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

    /// Lab: open the strain-upgrade panel (it manages its own farm-input pause).
    public void OpenLab()
    {
        CloseAll();
        var lab = FindFirstObjectByType<LabPanel>();
        if (lab != null) { lab.Open(); return; }
        var toast = FindFirstObjectByType<MessageToast>();
        if (toast != null) toast.Show("Lab coming soon.");
    }

    public void OpenShop()
    {
        CloseAll();
        SetPageActive(shopPage, true);
        shopPanel?.Open();
        SetFarmInput(false);
        CurrentPage = PageType.Shop;
    }

    public void CloseAll()
    {
        SetPageActive(shopPage, false);
        shopPanel?.Close();
        SetFarmInput(true);
        CurrentPage = PageType.None;
    }

    public void ToggleShop()
    {
        if (CurrentPage == PageType.Shop) CloseAll();
        else OpenShop();
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
