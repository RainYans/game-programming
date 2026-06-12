using System.Collections;
using TMPro;
using UnityEngine;

/// Shows temporary message on screen. Call Show(message) from any script via FindFirstObjectByType.
public class MessageToast : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private float defaultDuration = 2f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.gameObject.SetActive(false);
    }

    public void Show(string message, float? duration = null)
    {
        if (label == null) return;
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        label.text = message;
        label.gameObject.SetActive(true);
        hideRoutine = StartCoroutine(HideAfter(duration ?? defaultDuration));
    }

    private IEnumerator HideAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (label != null) label.gameObject.SetActive(false);
    }
}
