using System;
using UnityEngine;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    public static event Action<float> MusicVolumeChanged;
    public static event Action<float> SfxVolumeChanged;

    private const string MusicVolumeKey = "Audio.MusicVolume";
    private const string SfxVolumeKey = "Audio.SfxVolume";

    [Header("Defaults")]
    [SerializeField] [Range(0f, 1f)] private float defaultMusicVolume = 0.8f;
    [SerializeField] [Range(0f, 1f)] private float defaultSfxVolume = 1f;

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume));
    }

    private void Start()
    {
        NotifyCurrentValues();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.Save();
        MusicVolumeChanged?.Invoke(MusicVolume);
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.Save();
        SfxVolumeChanged?.Invoke(SfxVolume);
    }

    public static float GetMusicVolumeOrDefault(float fallback = 0.8f)
    {
        return Instance != null ? Instance.MusicVolume : Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, fallback));
    }

    public static float GetSfxVolumeOrDefault(float fallback = 1f)
    {
        return Instance != null ? Instance.SfxVolume : Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, fallback));
    }

    public static void ApplyMusicVolume(AudioSource audioSource, float baseVolume = 1f)
    {
        if (audioSource == null) return;
        audioSource.volume = Mathf.Clamp01(baseVolume) * GetMusicVolumeOrDefault();
    }

    public static void ApplySfxVolume(AudioSource audioSource, float baseVolume = 1f)
    {
        if (audioSource == null) return;
        audioSource.volume = Mathf.Clamp01(baseVolume) * GetSfxVolumeOrDefault();
    }

    private void NotifyCurrentValues()
    {
        MusicVolumeChanged?.Invoke(MusicVolume);
        SfxVolumeChanged?.Invoke(SfxVolume);
    }
}
