using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// Press Esc in battle to pause + open the menu. Defers to BattleCommandController while it's
/// in item targeting (Esc there cancels the throw). UI is hand-built in the scene by
/// BattleSceneSetup so layout is editable in the Hierarchy.
public class BattlePauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private BattleManager manager;
    [SerializeField] private BattleCommandController commandController;

    private bool paused;

    private void Awake()
    {
        if (manager == null) manager = FindFirstObjectByType<BattleManager>();
        if (commandController == null) commandController = FindFirstObjectByType<BattleCommandController>();
        if (panel != null) panel.SetActive(false);
        WireButton(resumeButton, Resume);
        WireButton(returnButton, ReturnToFarm);
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;
        // While the player is aiming an item, let the command controller eat Esc (to cancel).
        if (commandController != null && commandController.IsTargeting) return;
        TogglePause();
    }

    public void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (panel != null) panel.SetActive(paused);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        if (panel != null) panel.SetActive(false);
    }

    public void ReturnToFarm()
    {
        Time.timeScale = 1f;
        // Quitting mid-raid: no result is applied (no permadeath, no reward).
        BattleHandoff.ClearDeployment();
        BattleHandoff.ClearResult();
        if (manager != null) manager.ReturnToFarm();
    }

    private void OnDisable()
    {
        // Safety: don't leave the scene running at scale 0 if this gets disabled mid-pause.
        if (paused) { paused = false; Time.timeScale = 1f; }
    }

    private static void WireButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }
}
