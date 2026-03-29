using TMPro;
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

    [Header("Language")]
    [SerializeField] private Button languageToggleButton;
    [SerializeField] private TMP_Text languageToggleLabel;
    [SerializeField] private string englishLanguageLabel = "Language: English";
    [SerializeField] private string chineseLanguageLabel = "Language: 中文";

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
        LanguageSettingsManager.LanguageChanged += HandleLanguageChanged;

        SyncFromSettings();

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(HandleMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(HandleSfxSliderChanged);

        if (languageToggleButton != null)
            languageToggleButton.onClick.AddListener(HandleLanguageToggleClicked);
    }

    private void OnDisable()
    {
        AudioSettingsManager.MusicVolumeChanged -= HandleMusicVolumeChanged;
        AudioSettingsManager.SfxVolumeChanged -= HandleSfxVolumeChanged;
        LanguageSettingsManager.LanguageChanged -= HandleLanguageChanged;

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(HandleMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(HandleSfxSliderChanged);

        if (languageToggleButton != null)
            languageToggleButton.onClick.RemoveListener(HandleLanguageToggleClicked);
    }

    private void SyncFromSettings()
    {
        HandleMusicVolumeChanged(AudioSettingsManager.GetMusicVolumeOrDefault());
        HandleSfxVolumeChanged(AudioSettingsManager.GetSfxVolumeOrDefault());
        HandleLanguageChanged(LanguageSettingsManager.CurrentLanguage);
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

    private void HandleLanguageToggleClicked()
    {
        LanguageSettingsManager.ToggleLanguage();
    }

    private void HandleLanguageChanged(SupportedLanguage language)
    {
        if (languageToggleLabel != null)
            languageToggleLabel.text = language == SupportedLanguage.Chinese ? chineseLanguageLabel : englishLanguageLabel;
    }
}
