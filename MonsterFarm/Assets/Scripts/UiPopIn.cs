using System.Collections;
using UnityEngine;

/// Reusable "pop in" entrance for a UI panel: when the GameObject is enabled it scales up with a
/// gentle overshoot and (optionally) fades a CanvasGroup from 0 to 1. Pure presentation — drop it
/// on a card/dialog you SetActive(true) and it animates every time it appears. Uses unscaled time
/// so it still plays while the game is paused (Time.timeScale == 0).
[DisallowMultipleComponent]
public class UiPopIn : MonoBehaviour
{
    [SerializeField] private float duration = 0.32f;
    [SerializeField] private float fromScale = 0.82f;
    [SerializeField] private float overshoot = 1.06f;
    [Tooltip("Optional — faded 0→1 alongside the scale. Auto-found on this object if left null.")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        Vector3 target = Vector3.one;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            // ease-out with a small overshoot near the end
            float eased = EaseOutBack(k);
            transform.localScale = target * Mathf.LerpUnclamped(fromScale, 1f, eased);
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(k * 1.4f);
            yield return null;
        }
        transform.localScale = target;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    // Standard "back" ease whose peak is tuned by the overshoot field (1 = no overshoot).
    private float EaseOutBack(float x)
    {
        float s = 1.70158f * Mathf.Max(0f, (overshoot - 1f) / 0.06f);
        x -= 1f;
        return x * x * ((s + 1f) * x + s) + 1f;
    }
}
