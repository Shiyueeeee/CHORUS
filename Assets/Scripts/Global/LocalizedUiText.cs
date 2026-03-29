using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedUiText : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text legacyText;
    [SerializeField] private string englishText;
    [SerializeField] private string chineseText;
    [SerializeField] private TMP_FontAsset chineseFont;

    private TMP_FontAsset defaultTmpFont;

    private void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        if (legacyText == null)
            legacyText = GetComponent<Text>();

        if (tmpText != null)
            defaultTmpFont = tmpText.font;
    }

    private void OnEnable()
    {
        LanguageSettingsManager.LanguageChanged += HandleLanguageChanged;
        Apply(LanguageSettingsManager.CurrentLanguage);
    }

    private void OnDisable()
    {
        LanguageSettingsManager.LanguageChanged -= HandleLanguageChanged;
    }

    private void HandleLanguageChanged(SupportedLanguage language)
    {
        Apply(language);
    }

    private void Apply(SupportedLanguage language)
    {
        string localizedText = LanguageSettingsManager.GetLocalizedText(englishText, chineseText);

        if (tmpText != null)
        {
            tmpText.text = localizedText;

            TMP_FontAsset targetFont = null;
            if (language == SupportedLanguage.Chinese)
                targetFont = chineseFont != null ? chineseFont : LanguageSettingsManager.GetChineseUiFontOrNull();

            tmpText.font = targetFont != null ? targetFont : defaultTmpFont;
        }

        if (legacyText != null)
            legacyText.text = localizedText;
    }
}
