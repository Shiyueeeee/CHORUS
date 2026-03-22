using UnityEngine;
using UnityEngine.UI;

public class SettingsAudioTabBinder : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Optional Labels")]
    [SerializeField] private Text musicValueText;
    [SerializeField] private Text sfxValueText;

    private void Awake()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.wholeNumbers = false;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.wholeNumbers = false;
        }
    }

    private void OnEnable()
    {
        AudioSettingsManager.MusicVolumeChanged += HandleMusicVolumeChanged;
        AudioSettingsManager.SfxVolumeChanged += HandleSfxVolumeChanged;

        SyncFromSettings();

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(HandleMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(HandleSfxSliderChanged);
    }

    private void OnDisable()
    {
        AudioSettingsManager.MusicVolumeChanged -= HandleMusicVolumeChanged;
        AudioSettingsManager.SfxVolumeChanged -= HandleSfxVolumeChanged;

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(HandleMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(HandleSfxSliderChanged);
    }

    private void SyncFromSettings()
    {
        HandleMusicVolumeChanged(AudioSettingsManager.GetMusicVolumeOrDefault());
        HandleSfxVolumeChanged(AudioSettingsManager.GetSfxVolumeOrDefault());
    }

    private void HandleMusicSliderChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetMusicVolume(value);
        else
            UpdateValueLabel(musicValueText, value);
    }

    private void HandleSfxSliderChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetSfxVolume(value);
        else
            UpdateValueLabel(sfxValueText, value);
    }

    private void HandleMusicVolumeChanged(float value)
    {
        if (musicSlider != null && !Mathf.Approximately(musicSlider.value, value))
            musicSlider.SetValueWithoutNotify(value);

        UpdateValueLabel(musicValueText, value);
    }

    private void HandleSfxVolumeChanged(float value)
    {
        if (sfxSlider != null && !Mathf.Approximately(sfxSlider.value, value))
            sfxSlider.SetValueWithoutNotify(value);

        UpdateValueLabel(sfxValueText, value);
    }

    private static void UpdateValueLabel(Text target, float value)
    {
        if (target == null) return;
        target.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
    }
}
