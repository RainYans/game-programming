using System.Collections.Generic;
using UnityEngine;

public enum SfxKind
{
    Plant, Harvest, Buy,
    Hit, Death,
    Win, Lose,
    GateOpen, ItemThrow,
    Dash, ButtonClick,
}

/// Dirt-simple SFX layer: a DontDestroyOnLoad singleton that lazily generates short sine-wave
/// "blips" the first time each kind plays. No audio assets needed — easy to swap in real clips
/// later by replacing Generate(...) with a serialized clip lookup.
///
/// Usage from any script: SfxManager.Play(SfxKind.Hit); — the instance auto-creates itself.
public class SfxManager : MonoBehaviour
{
    private const int SampleRate = 44100;
    [SerializeField] private float masterVolume = 0.55f;

    private static SfxManager instance;
    private AudioSource source;
    private readonly Dictionary<SfxKind, AudioClip> cache = new Dictionary<SfxKind, AudioClip>();

    public static void Play(SfxKind kind)
    {
        EnsureInstance();
        if (instance != null) instance.PlayInternal(kind);
    }

    /// Master SFX volume (0..1) for the options menu; persisted in PlayerPrefs.
    public static void SetVolume(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("SfxVolume", v);
        if (instance != null) instance.masterVolume = v;
    }

    public static float GetVolume()
        => instance != null ? instance.masterVolume : PlayerPrefs.GetFloat("SfxVolume", 0.55f);

    private static void EnsureInstance()
    {
        if (instance != null) return;
        var go = new GameObject("SfxManager");
        instance = go.AddComponent<SfxManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        masterVolume = PlayerPrefs.GetFloat("SfxVolume", masterVolume);
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0f;
        source.playOnAwake = false;
    }

    private void PlayInternal(SfxKind kind)
    {
        if (!cache.TryGetValue(kind, out AudioClip clip))
        {
            // Prefer a real clip at Resources/SFX/<Kind>.ogg (drop in / swap files freely);
            // fall back to the generated sine blip when none is present.
            clip = Resources.Load<AudioClip>("SFX/" + kind) ?? Generate(kind);
            cache[kind] = clip;
        }
        if (clip != null && source != null)
            source.PlayOneShot(clip, masterVolume * VolumeFor(kind));
    }

    private float VolumeFor(SfxKind kind) => kind switch
    {
        SfxKind.Hit => 0.35f,         // fires often — keep it quiet
        SfxKind.Dash => 0.45f,
        SfxKind.ItemThrow => 0.55f,
        _ => 0.85f,
    };

    /// Maps each SFX kind to a short tone (or a 3-note sequence for win/lose).
    private static AudioClip Generate(SfxKind kind)
    {
        switch (kind)
        {
            case SfxKind.Plant:      return Blip(620f, 0.10f, env: 9f);
            case SfxKind.Harvest:    return Sequence(new[] { 660f, 880f }, 0.07f);
            case SfxKind.Buy:        return Sequence(new[] { 700f, 950f }, 0.05f, env: 11f);
            case SfxKind.Hit:        return Blip(220f, 0.06f, env: 18f);     // thud
            case SfxKind.Death:      return Sequence(new[] { 180f, 110f }, 0.10f, env: 6f);
            case SfxKind.Win:        return Sequence(new[] { 660f, 880f, 1175f }, 0.10f, env: 5f);
            case SfxKind.Lose:       return Sequence(new[] { 520f, 415f, 330f }, 0.13f, env: 4f);
            case SfxKind.GateOpen:   return Blip(400f, 0.20f, env: 5f);
            case SfxKind.ItemThrow:  return Sequence(new[] { 1100f, 820f }, 0.04f, env: 14f);
            case SfxKind.Dash:       return Blip(900f, 0.06f, env: 18f);
            case SfxKind.ButtonClick: return Blip(1000f, 0.04f, env: 20f);
            default: return Blip(440f, 0.08f);
        }
    }

    private static AudioClip Blip(float freq, float duration, float volume = 0.5f, float env = 8f)
    {
        int samples = Mathf.Max(1, (int)(SampleRate * duration));
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * env);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * volume;
        }
        var clip = AudioClip.Create($"Sfx_{freq:F0}", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip Sequence(float[] freqs, float perNote, float volume = 0.5f, float env = 8f)
    {
        int perNoteSamples = Mathf.Max(1, (int)(SampleRate * perNote));
        int total = perNoteSamples * freqs.Length;
        float[] data = new float[total];
        for (int n = 0; n < freqs.Length; n++)
        {
            for (int i = 0; i < perNoteSamples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-t * env);
                data[n * perNoteSamples + i] = Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * envelope * volume;
            }
        }
        var clip = AudioClip.Create("Sfx_seq", total, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
