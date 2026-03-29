using System;
using TMPro;
using UnityEngine;

public enum SupportedLanguage
{
    English = 0,
    Chinese = 1
}

public static class LanguageSettingsManager
{
    public static event Action<SupportedLanguage> LanguageChanged;

    private const string LanguageKey = "Language.Selected";

    private static bool initialized;
    private static SupportedLanguage currentLanguage = SupportedLanguage.English;
    private static LanguageFontSettings cachedFontSettings;

    public static SupportedLanguage CurrentLanguage
    {
        get
        {
            EnsureInitialized();
            return currentLanguage;
        }
    }

    public static string GetLocalizedText(string englishText, string chineseText)
    {
        if (CurrentLanguage == SupportedLanguage.Chinese && !string.IsNullOrWhiteSpace(chineseText))
            return chineseText;

        return englishText ?? string.Empty;
    }

    public static TMP_FontAsset GetChineseUiFontOrNull()
    {
        if (cachedFontSettings == null)
            cachedFontSettings = Resources.Load<LanguageFontSettings>("LanguageFontSettings");

        return cachedFontSettings != null ? cachedFontSettings.uiChineseFont : null;
    }

    public static void ToggleLanguage()
    {
        SetLanguage(CurrentLanguage == SupportedLanguage.Chinese ? SupportedLanguage.English : SupportedLanguage.Chinese);
    }

    public static void SetLanguage(SupportedLanguage language)
    {
        EnsureInitialized();
        if (currentLanguage == language)
            return;

        currentLanguage = language;
        PlayerPrefs.SetInt(LanguageKey, (int)currentLanguage);
        PlayerPrefs.Save();
        LanguageChanged?.Invoke(currentLanguage);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        initialized = false;
        currentLanguage = SupportedLanguage.English;
        cachedFontSettings = null;
        LanguageChanged = null;
    }

    private static void EnsureInitialized()
    {
        if (initialized)
            return;

        int savedValue = PlayerPrefs.GetInt(LanguageKey, (int)SupportedLanguage.English);
        if (!Enum.IsDefined(typeof(SupportedLanguage), savedValue))
            savedValue = (int)SupportedLanguage.English;

        currentLanguage = (SupportedLanguage)savedValue;
        initialized = true;
    }
}
