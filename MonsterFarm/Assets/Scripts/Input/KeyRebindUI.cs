using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Drives the key-rebinding rows in the options menu. Each row shows an action's current key on a
/// button; clicking it listens for the next key pressed and rebinds. The rows + reset button are
/// real, editable scene objects wired in the Inspector. Persists via KeyBindings (PlayerPrefs).
public class KeyRebindUI : MonoBehaviour
{
    [System.Serializable]
    public struct RebindRow
    {
        public BindAction action;
        public Button button;
        public TMP_Text keyLabel;
    }

    [SerializeField] private RebindRow[] rows;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;

    private int listening = -1;

    private void Awake()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int idx = i;
            if (rows[i].button != null) { rows[i].button.onClick.RemoveAllListeners(); rows[i].button.onClick.AddListener(() => StartListen(idx)); }
        }
        if (resetButton != null) { resetButton.onClick.RemoveAllListeners(); resetButton.onClick.AddListener(ResetAll); }
        if (backButton != null) { backButton.onClick.RemoveAllListeners(); backButton.onClick.AddListener(() => gameObject.SetActive(false)); }
        Refresh();
    }

    private void OnEnable() { listening = -1; Refresh(); }

    private void StartListen(int idx)
    {
        listening = idx;
        if (rows[idx].keyLabel != null) rows[idx].keyLabel.text = "press a key";
        SfxManager.Play(SfxKind.ButtonClick);
    }

    private void ResetAll()
    {
        KeyBindings.ResetDefaults();
        listening = -1;
        Refresh();
        SfxManager.Play(SfxKind.ButtonClick);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (listening < 0)
        {
            if (kb.escapeKey.wasPressedThisFrame) gameObject.SetActive(false); // Esc backs out of controls
            return;
        }

        if (kb.escapeKey.wasPressedThisFrame) { listening = -1; Refresh(); return; } // cancel rebind

        foreach (var kc in kb.allKeys)
        {
            if (!kc.wasPressedThisFrame) continue;
            Key key = kc.keyCode;
            if (key == Key.None || key == Key.Escape) continue;
            KeyBindings.Set(rows[listening].action, key);
            listening = -1;
            Refresh();
            SfxManager.Play(SfxKind.ButtonClick);
            break;
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < rows.Length; i++)
            if (rows[i].keyLabel != null) rows[i].keyLabel.text = KeyBindings.Label(rows[i].action);
    }
}
