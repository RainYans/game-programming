using UnityEngine;

/// Tiny persistent flag store for the new-player guidance. Tracks whether the farm onboarding and
/// the combat tutorial have been seen, and names the dedicated tutorial battle scene that the very
/// first raid routes into. Backed by PlayerPrefs so it survives between sessions; ResetAll() lets a
/// debug/inspector toggle replay the whole thing.
public static class TutorialState
{
    const string KeyFarm = "mf_tutorial_farm_done";
    const string KeyBattle = "mf_tutorial_battle_done";

    /// The dedicated combat-tutorial scene the first expedition loads instead of a real city raid.
    public const string TutorialSceneName = "Tutorial";

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

    /// Wipe both flags so the guidance plays again from scratch (used by the inspector replay toggle).
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(KeyFarm);
        PlayerPrefs.DeleteKey(KeyBattle);
        PlayerPrefs.Save();
    }
}
