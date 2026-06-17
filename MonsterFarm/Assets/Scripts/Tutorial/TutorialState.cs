using UnityEngine;

/// Tiny persistent flag store for the new-player guidance. Tracks whether the How-to-Play manual has
/// been seen, whether the farm onboarding and the combat tutorial have been completed, and names the
/// dedicated tutorial battle scene that the very first raid routes into. Backed by PlayerPrefs so it
/// survives between sessions; ResetAll() lets a debug/inspector toggle (or New Game) replay it all.
public static class TutorialState
{
    const string KeyManual = "mf_manual_seen";
    const string KeyFarm = "mf_tutorial_farm_done";
    const string KeyBattle = "mf_tutorial_battle_done";

    /// The dedicated combat-tutorial scene the first expedition loads instead of a real city raid.
    public const string TutorialSceneName = "Tutorial";

    /// Whether the player has already been shown the How-to-Play manual book at least once. The farm
    /// auto-opens it on first entry (before onboarding); after that it is review-only (pause menu).
    public static bool ManualSeen
    {
        get => PlayerPrefs.GetInt(KeyManual, 0) == 1;
        set { PlayerPrefs.SetInt(KeyManual, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool FarmOnboardDone
    {
        get => PlayerPrefs.GetInt(KeyFarm, 0) == 1;
        set { PlayerPrefs.SetInt(KeyFarm, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool BattleTutorialDone
    {
        get => PlayerPrefs.GetInt(KeyBattle, 0) == 1;
        set { PlayerPrefs.SetInt(KeyBattle, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    /// Wipe every flag so the manual + onboarding + combat tutorial all play again from scratch
    /// (used by New Game and the inspector replay toggle).
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(KeyManual);
        PlayerPrefs.DeleteKey(KeyFarm);
        PlayerPrefs.DeleteKey(KeyBattle);
        PlayerPrefs.Save();
    }
}
