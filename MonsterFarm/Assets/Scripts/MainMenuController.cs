using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Title-screen menu. Continue loads the existing save (Farm), New Game wipes the save + tutorial
/// flags and plays the story intro, Quit exits. Continue is disabled when no save exists, and New
/// Game asks to confirm before overwriting one. Fades the screen in on enter and out on transition.
public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button quitButton;

    [Header("Overwrite confirm (shown when New Game is pressed with a save present)")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Button confirmYes;
    [SerializeField] private Button confirmNo;

    [Header("Transition")]
    [SerializeField] private CanvasGroup fade; // full-screen black overlay
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private string farmScene = "Farm";
    [SerializeField] private string introScene = "Intro";

    private bool busy;

    private void Start()
    {
        Time.timeScale = 1f;
        bool hasSave = SaveManager.HasSave();
        if (continueButton != null) continueButton.interactable = hasSave;

        Wire(continueButton, Continue);
        Wire(newGameButton, NewGamePressed);
        Wire(quitButton, Quit);
        Wire(confirmYes, StartNewGame);
        Wire(confirmNo, () => { if (confirmPanel != null) confirmPanel.SetActive(false); });
        if (confirmPanel != null) confirmPanel.SetActive(false);

        if (fade != null) { fade.alpha = 1f; fade.blocksRaycasts = false; StartCoroutine(FadeTo(0f)); }
    }

    private void Continue()
    {
        if (busy || !SaveManager.HasSave()) return;
        StartCoroutine(LoadAfterFade(farmScene));
    }

    private void NewGamePressed()
    {
        if (busy) return;
        if (SaveManager.HasSave() && confirmPanel != null) confirmPanel.SetActive(true);
        else StartNewGame();
    }

    private void StartNewGame()
    {
        if (busy) return;
        SaveManager.DeleteSave();      // fresh slot — Farm will seed starting defaults
        TutorialState.ResetAll();      // replay onboarding + combat tutorial
        StartCoroutine(LoadAfterFade(introScene));
    }

    private void Quit()
    {
        if (busy) return;
        SfxManager.Play(SfxKind.ButtonClick);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator LoadAfterFade(string scene)
    {
        busy = true;
        SfxManager.Play(SfxKind.ButtonClick);
        yield return FadeTo(1f);
        SceneManager.LoadScene(scene);
    }

    private IEnumerator FadeTo(float target)
    {
        if (fade == null) yield break;
        fade.blocksRaycasts = target > 0.5f;
        float from = fade.alpha, t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(from, target, t / fadeTime);
            yield return null;
        }
        fade.alpha = target;
    }

    private static void Wire(Button b, UnityEngine.Events.UnityAction a)
    {
        if (b == null) return;
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(a);
    }
}
