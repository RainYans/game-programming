using UnityEngine;

/// Master volume on top of the per-channel Music/SFX volumes. Drives the global
/// AudioListener.volume and persists in PlayerPrefs; applied at startup in every scene.
public static class MasterAudio
{
    private const string Pref = "MasterVolume";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyAtStartup()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(Pref, 1f));
    }

    public static void SetMaster(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(Pref, v);
        AudioListener.volume = v;
    }

    public static float GetMaster() => Mathf.Clamp01(PlayerPrefs.GetFloat(Pref, 1f));
}
