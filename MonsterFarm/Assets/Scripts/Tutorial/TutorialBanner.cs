using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Reusable on-screen coaching banner (parchment + pixel font), shared by the farm onboarding and
/// the combat tutorial. A controller drives it: Begin(total) → SetStep(...) per step → Finish(msg).
/// The banner owns only presentation: fade in/out, the "Step n / N" line, a per-step dot strip, a
/// key-hint pill, the Skip button, and a small attention pulse. No game logic lives here.
public class TutorialBanner : MonoBehaviour
{
    [Header("Wired by the build script")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text stepText;      // "Step 2 / 5"
    [SerializeField] private TMP_Text messageText;   // the instruction line
    [SerializeField] private TMP_Text keyHintText;   // "[W] [A] [S] [D]" pill
    [SerializeField] private GameObject keyHintPill; // background behind keyHintText (hidden when no keys)
    [SerializeField] private Transform dotsParent;
    [SerializeField] private Image dotTemplate;
    [SerializeField] private Button skipButton;
    [SerializeField] private RectTransform pulseTarget; // scaled briefly on step change (the card)

    [SerializeField] private float fadeTime = 0.25f;

    static readonly Color DotOff = new Color(0.62f, 0.50f, 0.34f, 0.55f);
    static readonly Color DotOn = new Color(0.95f, 0.80f, 0.28f, 1f);
    static readonly Color DotDone = new Color(0.36f, 0.66f, 0.30f, 1f);

    /// Raised when the player clicks Skip (the controller decides what skipping means).
    public event Action SkipRequested;

    private readonly List<Image> dots = new List<Image>();
    private Coroutine fadeCo, pulseCo;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (pulseTarget == null) pulseTarget = transform as RectTransform;
        if (dotTemplate != null) dotTemplate.gameObject.SetActive(false);
        if (skipButton != null) skipButton.onClick.AddListener(() => SkipRequested?.Invoke());
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Begin(int totalSteps)
    {
        gameObject.SetActive(true);
        BuildDots(totalSteps);
        FadeTo(1f);
    }

    /// Show or hide the Skip button (the combat tutorial is mandatory, so it hides it).
    public void SetSkippable(bool on)
    {
        if (skipButton != null) skipButton.gameObject.SetActive(on);
    }

    public void SetStep(int index, int total, string message, params string[] keys)
    {
        if (stepText != null) stepText.text = $"Step {index + 1} / {total}";
        if (messageText != null) messageText.text = message;
        for (int i = 0; i < dots.Count; i++)
            dots[i].color = i < index ? DotDone : (i == index ? DotOn : DotOff);

        bool hasKeys = keys != null && keys.Length > 0;
        if (keyHintText != null) keyHintText.text = hasKeys ? "  " + string.Join("   ", keys) + "  " : string.Empty;
        if (keyHintPill != null) keyHintPill.SetActive(hasKeys);

        if (isActiveAndEnabled) { if (pulseCo != null) StopCoroutine(pulseCo); pulseCo = StartCoroutine(Pulse()); }
    }

    /// Mark every dot done, show a closing message, then fade out and disable after a short beat.
    public void Finish(string message, float hold = 1.6f)
    {
        for (int i = 0; i < dots.Count; i++) dots[i].color = DotDone;
        if (stepText != null) stepText.text = "Done!";
        if (messageText != null) messageText.text = message;
        if (keyHintText != null) keyHintText.text = string.Empty;
        if (keyHintPill != null) keyHintPill.SetActive(false);
        if (skipButton != null) skipButton.gameObject.SetActive(false);
        if (isActiveAndEnabled) StartCoroutine(FinishRoutine(hold));
        else gameObject.SetActive(false);
    }

    public void HideImmediate()
    {
        StopAllCoroutines();
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private IEnumerator FinishRoutine(float hold)
    {
        if (pulseCo != null) StopCoroutine(pulseCo);
        pulseCo = StartCoroutine(Pulse());
        float t = 0f;
        while (t < hold) { t += Time.unscaledDeltaTime; yield return null; }
        FadeTo(0f, disableAtEnd: true);
    }

    private void BuildDots(int n)
    {
        dots.Clear();
        if (dotsParent == null) return;

        // Reuse the REAL dot objects already placed in the scene (so they're editable in the
        // Hierarchy); only clone the template as a fallback if the scene is short on dots.
        var pool = new List<Image>();
        foreach (RectTransform child in dotsParent)
        {
            if (dotTemplate != null && child == dotTemplate.rectTransform) continue;
            var img = child.GetComponent<Image>();
            if (img != null) pool.Add(img);
        }
        for (int i = pool.Count; i < n && dotTemplate != null; i++)
            pool.Add(Instantiate(dotTemplate, dotsParent));

        for (int i = 0; i < pool.Count; i++)
        {
            bool use = i < n;
            pool[i].gameObject.SetActive(use);
            if (use) { pool[i].color = DotOff; dots.Add(pool[i]); }
        }
    }

    private void FadeTo(float target, bool disableAtEnd = false)
    {
        if (canvasGroup == null) { if (disableAtEnd) gameObject.SetActive(false); return; }
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRoutine(target, disableAtEnd));
    }

    private IEnumerator FadeRoutine(float target, bool disableAtEnd)
    {
        float from = canvasGroup.alpha;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, target, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = target;
        if (disableAtEnd && target <= 0.01f) gameObject.SetActive(false);
    }

    private IEnumerator Pulse()
    {
        if (pulseTarget == null) yield break;
        float t = 0f, dur = 0.22f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = t / dur;
            float s = 1f + 0.05f * Mathf.Sin(k * Mathf.PI);
            pulseTarget.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        pulseTarget.localScale = Vector3.one;
    }
}
