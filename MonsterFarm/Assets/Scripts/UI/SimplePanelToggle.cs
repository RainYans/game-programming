using UnityEngine;
using UnityEngine.UI;

/// Minimal open/close wiring for a self-contained panel (used by the "How to Play" cheat-sheet).
/// An open button shows the panel, a close button (and an optional backdrop button) hides it. The
/// panel starts hidden. No dependencies — just SetActive toggling on real scene objects.
public class SimplePanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backdropButton;
    [SerializeField] private bool startHidden = true;

    private void Awake()
    {
        if (openButton != null) openButton.onClick.AddListener(() => Set(true));
        if (closeButton != null) closeButton.onClick.AddListener(() => Set(false));
        if (backdropButton != null) backdropButton.onClick.AddListener(() => Set(false));
        if (startHidden && panel != null) panel.SetActive(false);
    }

    public void Set(bool visible)
    {
        if (panel != null) panel.SetActive(visible);
    }

    public void Toggle()
    {
        if (panel != null) panel.SetActive(!panel.activeSelf);
    }
}
