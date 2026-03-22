using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ManagedAudioSourceVolume : MonoBehaviour
{
    public enum AudioChannel
    {
        Music,
        Sfx
    }

    [SerializeField] private AudioChannel channel = AudioChannel.Sfx;
    [SerializeField] [Range(0f, 1f)] private float baseVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        AudioSettingsManager.MusicVolumeChanged += HandleMusicVolumeChanged;
        AudioSettingsManager.SfxVolumeChanged += HandleSfxVolumeChanged;
        RefreshVolume();
    }

    private void OnDisable()
    {
        AudioSettingsManager.MusicVolumeChanged -= HandleMusicVolumeChanged;
        AudioSettingsManager.SfxVolumeChanged -= HandleSfxVolumeChanged;
    }

    private void HandleMusicVolumeChanged(float _)
    {
        if (channel == AudioChannel.Music)
            RefreshVolume();
    }

    private void HandleSfxVolumeChanged(float _)
    {
        if (channel == AudioChannel.Sfx)
            RefreshVolume();
    }

    private void RefreshVolume()
    {
        if (audioSource == null) return;

        if (channel == AudioChannel.Music)
            AudioSettingsManager.ApplyMusicVolume(audioSource, baseVolume);
        else
            AudioSettingsManager.ApplySfxVolume(audioSource, baseVolume);
    }
}
