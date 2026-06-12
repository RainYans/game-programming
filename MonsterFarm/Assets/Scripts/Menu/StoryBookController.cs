using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// New-game story intro played as a turning picture-book. Each page carries an illustration (drop
/// an AI-generated image into its slot in the Inspector — a placeholder shows until then) plus a
/// line of narration. Click / Next / Space turns the page with a lift-and-settle animation and a
/// page-flip sound; Skip (or finishing the last page) fades out and loads the farm. Pure
/// presentation + scene flow; pages are authored as editable data on this component.
public class StoryBookController : MonoBehaviour
{
    [System.Serializable]
    public class Page
    {
        public Sprite illustration;          // left page art — assign your AI image here
        [TextArea(2, 5)] public string text; // right page narration
    }

    [Header("Story pages (edit in the Inspector; drop AI art into each illustration slot)")]
    [SerializeField] private List<Page> pages = new List<Page>();

    [Header("UI refs")]
    [SerializeField] private Image illustration;
    [SerializeField] private TMP_Text narration;
    [SerializeField] private TMP_Text pageLabel;
    [SerializeField] private RectTransform contentRoot;   // illustration+text holder (animated on turn)
    [SerializeField] private CanvasGroup contentGroup;
    [SerializeField] private Image turnSweep;             // optional page-colored sweep overlay
    [SerializeField] private TMP_Text nextLabel;          // "Next ▶" — becomes "Begin" on the last page
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    [Header("Transition")]
    [SerializeField] private CanvasGroup sceneFade;       // full-screen black overlay
    [SerializeField] private float fadeTime = 0.6f;
    [SerializeField] private string farmScene = "Farm";

    private int index;
    private bool turning, finished;

    private void Start()
    {
        Time.timeScale = 1f;
        if (nextButton != null) { nextButton.onClick.RemoveAllListeners(); nextButton.onClick.AddListener(Advance); }
        if (skipButton != null) { skipButton.onClick.RemoveAllListeners(); skipButton.onClick.AddListener(Skip); }
        if (turnSweep != null) turnSweep.gameObject.SetActive(false);

        SetPage(0);
        if (sceneFade != null) { sceneFade.alpha = 1f; sceneFade.blocksRaycasts = false; StartCoroutine(FadeScene(0f)); }
    }

    private void Update()
    {
        if (turning || finished) return;
        Mouse m = Mouse.current; Keyboard k = Keyboard.current;
        bool overButton = UnityEngine.EventSystems.EventSystem.current != null &&
                          UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        bool click = m != null && m.leftButton.wasPressedThisFrame && !overButton;
        bool key = k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
        if (click || key) Advance();
    }

    private void Advance()
    {
        if (turning || finished) return;
        if (index >= pages.Count - 1) { StartCoroutine(Finish()); return; }
        StartCoroutine(Turn(index + 1));
    }

    private void Skip()
    {
        if (finished) return;
        StartCoroutine(Finish());
    }

    private IEnumerator Turn(int next)
    {
        turning = true;
        SfxManager.Play(SfxKind.ButtonClick);

        // lift the current page toward the spine
        yield return Animate(1f, 0.15f, 1f, 0.9f, 0.14f);
        if (turnSweep != null) StartCoroutine(Sweep());
        SetPage(next);
        // settle the new page in
        yield return Animate(0.15f, 1f, 0.9f, 1f, 0.18f);

        turning = false;
    }

    private IEnumerator Sweep()
    {
        turnSweep.gameObject.SetActive(true);
        var rt = turnSweep.rectTransform;
        float t = 0f, dur = 0.26f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = t / dur;
            rt.localScale = new Vector3(Mathf.Lerp(1f, 0f, k), 1f, 1f);
            turnSweep.color = new Color(1f, 1f, 1f, 0.5f * (1f - k));
            yield return null;
        }
        rt.localScale = Vector3.one;
        turnSweep.gameObject.SetActive(false);
    }

    private IEnumerator Animate(float a0, float a1, float s0, float s1, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = t / dur;
            if (contentGroup != null) contentGroup.alpha = Mathf.Lerp(a0, a1, k);
            if (contentRoot != null) contentRoot.localScale = Vector3.one * Mathf.Lerp(s0, s1, k);
            yield return null;
        }
        if (contentGroup != null) contentGroup.alpha = a1;
        if (contentRoot != null) contentRoot.localScale = Vector3.one * s1;
    }

    private void SetPage(int i)
    {
        index = Mathf.Clamp(i, 0, Mathf.Max(0, pages.Count - 1));
        if (pages.Count == 0) return;
        Page p = pages[index];
        if (illustration != null && p.illustration != null) { illustration.sprite = p.illustration; illustration.color = Color.white; }
        if (narration != null) narration.text = p.text;
        if (pageLabel != null) pageLabel.text = $"{index + 1} / {pages.Count}";
        if (nextLabel != null) nextLabel.text = index >= pages.Count - 1 ? "Begin" : "Next  >";
    }

    private IEnumerator Finish()
    {
        finished = true;
        yield return FadeScene(1f);
        SceneManager.LoadScene(farmScene);
    }

    private IEnumerator FadeScene(float target)
    {
        if (sceneFade == null) yield break;
        sceneFade.blocksRaycasts = target > 0.5f;
        float from = sceneFade.alpha, t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            sceneFade.alpha = Mathf.Lerp(from, target, t / fadeTime);
            yield return null;
        }
        sceneFade.alpha = target;
    }
}
