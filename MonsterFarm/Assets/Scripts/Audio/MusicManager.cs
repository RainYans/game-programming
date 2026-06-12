using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Background music. This lives as a REAL GameObject in the entry scenes (MainMenu / Farm /
/// Battle) — drop the component on an object, drag the three clips onto the fields, and it is
/// fully editable in the Inspector. It is a DontDestroyOnLoad singleton: the first instance
/// persists across scene loads and picks the right looping track per scene, crossfading on
/// change; any duplicate placed in a later scene removes itself.
///
/// Volume is shared with the pause / options menu via SetVolume()/GetVolume(), persisted in
/// PlayerPrefs("MusicVolume").
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Header("Tracks (assign in the Inspector)")]
    [SerializeField] private AudioClip menuClip;    // MainMenu / Intro
    [SerializeField] private AudioClip farmClip;    // Farm
    [SerializeField] private AudioClip battleClip;  // Battle / Tutorial

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    private const string PrefVolume = "MusicVolume";
    private const float FadeSeconds = 0.8f;

    private static MusicManager instance;
    private AudioSource source;
    private AudioClip currentClip;
    private Coroutine fade;

    /// Maps a scene name to the clip to loop there (null = silence).
    private AudioClip ClipForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
            case "Intro":    return menuClip;
            case "Farm":     return farmClip;
            case "Battle":   return battleClip;
            case "Tutorial": return battleClip;
            default:         return null;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        musicVolume = PlayerPrefs.GetFloat(PrefVolume, musicVolume);

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlayForScene(scene.name);

    /// Switch to the track for this scene (crossfading), or fade to silence if none maps.
    public void PlayForScene(string sceneName)
    {
        AudioClip clip = ClipForScene(sceneName);
        if (clip == currentClip) return;        // already in the right mood — don't restart
        currentClip = clip;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(CrossfadeTo(clip));
    }

    private IEnumerator CrossfadeTo(AudioClip next)
    {
        float startVol = source.volume;
        for (float ti = 0f; source.isPlaying && ti < FadeSeconds; ti += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(startVol, 0f, ti / FadeSeconds);
            yield return null;
        }
        source.Stop();

        if (next == null) { source.clip = null; fade = null; yield break; }

        source.clip = next;
        source.volume = 0f;
        source.Play();
        for (float ti = 0f; ti < FadeSeconds; ti += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(0f, musicVolume, ti / FadeSeconds);
            yield return null;
        }
        source.volume = musicVolume;
        fade = null;
    }

    /// Called by the options/volume slider (0..1). Persists and applies immediately.
    public static void SetVolume(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(PrefVolume, v);
        if (instance != null)
        {
            instance.musicVolume = v;
            if (instance.fade == null && instance.source != null) instance.source.volume = v;
        }
    }

    public static float GetVolume()
        => instance != null ? instance.musicVolume : PlayerPrefs.GetFloat(PrefVolume, 0.5f);
}
