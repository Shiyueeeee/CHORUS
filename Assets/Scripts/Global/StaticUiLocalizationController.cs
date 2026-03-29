using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaticUiLocalizationController : MonoBehaviour
{
    private static readonly Dictionary<string, string> ChineseTextByEnglish = new Dictionary<string, string>
    {
        { "NEW GAME", "新游戏" },
        { "CONTINUE", "继续" },
        { "Settings", "设置" },
        { "Exit Game", "退出游戏" },
        { "Gallery", "相册" },
        { "Music", "音乐" },
        { "SFX", "音效" },
        { "jump last", "跳到最后" }
    };

    private readonly Dictionary<TMP_Text, TMP_FontAsset> originalTmpFonts = new Dictionary<TMP_Text, TMP_FontAsset>();
    private readonly Dictionary<TMP_Text, string> originalTmpTexts = new Dictionary<TMP_Text, string>();
    private readonly Dictionary<Text, string> originalUiTexts = new Dictionary<Text, string>();

    private void OnEnable()
    {
        LanguageSettingsManager.LanguageChanged += HandleLanguageChanged;
        CacheTargets();
        ApplyLanguage(LanguageSettingsManager.CurrentLanguage);
    }

    private void OnDisable()
    {
        LanguageSettingsManager.LanguageChanged -= HandleLanguageChanged;
    }

    private void CacheTargets()
    {
        CacheTmpTexts();
        CacheLegacyTexts();
    }

    private void CacheTmpTexts()
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text == null || originalTmpTexts.ContainsKey(text))
                continue;

            if (text.GetComponent<LocalizedUiText>() != null)
                continue;

            originalTmpTexts[text] = text.text;
            originalTmpFonts[text] = text.font;
        }
    }

    private void CacheLegacyTexts()
    {
        Text[] texts = GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            if (text == null || originalUiTexts.ContainsKey(text))
                continue;

            if (text.GetComponent<LocalizedUiText>() != null)
                continue;

            originalUiTexts[text] = text.text;
        }
    }

    private void HandleLanguageChanged(SupportedLanguage language)
    {
        ApplyLanguage(language);
    }

    private void ApplyLanguage(SupportedLanguage language)
    {
        CacheTargets();

        TMP_FontAsset chineseFont = LanguageSettingsManager.GetChineseUiFontOrNull();

        foreach (KeyValuePair<TMP_Text, string> entry in originalTmpTexts)
        {
            TMP_Text text = entry.Key;
            if (text == null)
                continue;

            string english = entry.Value ?? string.Empty;
            text.text = language == SupportedLanguage.Chinese && ChineseTextByEnglish.TryGetValue(english, out string chinese)
                ? chinese
                : english;

            if (originalTmpFonts.TryGetValue(text, out TMP_FontAsset originalFont))
                text.font = language == SupportedLanguage.Chinese && chineseFont != null ? chineseFont : originalFont;
        }

        foreach (KeyValuePair<Text, string> entry in originalUiTexts)
        {
            Text text = entry.Key;
            if (text == null)
                continue;

            string english = entry.Value ?? string.Empty;
            text.text = language == SupportedLanguage.Chinese && ChineseTextByEnglish.TryGetValue(english, out string chinese)
                ? chinese
                : english;
        }
    }
}
