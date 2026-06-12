using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Esc opens a small options panel on the farm: Resume, music + SFX volume sliders, Main Menu,
/// Quit. The panel and its widgets are real, editable scene objects (wired in the Inspector). It
/// only opens when no other full-screen panel is up (checked via the avatar input being active),
/// so Esc still closes the shop/deploy/lab panels normally.
public class FarmPauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button controlsButton;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private AvatarController avatarMovement;
    [SerializeField] private AvatarInteraction avatarInteraction;
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool isOpen;

    private void Awake()
    {
        if (avatarMovement == null) avatarMovement = FindFirstObjectByType<AvatarController>();
        if (avatarInteraction == null) avatarInteraction = FindFirstObjectByType<AvatarInteraction>();

        if (resumeButton != null) { resumeButton.onClick.RemoveListener(Close); resumeButton.onClick.AddListener(Close); }
        if (menuButton != null) menuButton.onClick.AddListener(ToMenu);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
        if (masterSlider != null) { masterSlider.minValue = 0f; masterSlider.maxValue = 1f; masterSlider.onValueChanged.AddListener(MasterAudio.SetMaster); }
        if (musicSlider != null) { musicSlider.minValue = 0f; musicSlider.maxValue = 1f; musicSlider.onValueChanged.AddListener(MusicManager.SetVolume); }
        if (sfxSlider != null) { sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f; sfxSlider.onValueChanged.AddListener(SfxManager.SetVolume); }
        if (controlsButton != null) { controlsButton.onClick.RemoveAllListeners(); controlsButton.onClick.AddListener(OpenControls); }

        if (panel != null) panel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb[Key.Escape].wasPressedThisFrame)
        {
            if (controlsPanel != null && controlsPanel.activeSelf) return; // controls panel handles its own Esc
            if (isOpen) Close();
            else if (avatarMovement == null || avatarMovement.enabled) Open(); // only when no panel is up
        }
    }

    public void Open()
    {
        if (panel == null) return;
        if (masterSlider != null) masterSlider.SetValueWithoutNotify(MasterAudio.GetMaster());
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(MusicManager.GetVolume());
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(SfxManager.GetVolume());
        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
        isOpen = true;
        SetInput(false);
        SfxManager.Play(SfxKind.ButtonClick);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        isOpen = false;
        SetInput(true);
    }

    public void OpenControls()
    {
        if (controlsPanel == null) return;
        controlsPanel.SetActive(true);
        controlsPanel.transform.SetAsLastSibling();
        SfxManager.Play(SfxKind.ButtonClick);
    }

    private void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetInput(bool e)
    {
        if (avatarMovement != null) avatarMovement.enabled = e;
        if (avatarInteraction != null) avatarInteraction.enabled = e;
    }
}
