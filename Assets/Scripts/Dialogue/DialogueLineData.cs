using UnityEngine;

[System.Serializable]
public class DialogueLineData
{
    public string speaker;
    public string translatedSpeaker;

    [TextArea(2, 5)]
    public string text;

    [TextArea(2, 5)]
    public string translatedText;

    public AudioClip sfxClip;

    [Range(0f, 1f)]
    public float sfxVolumeScale = 1f;

    public string GetLocalizedText()
    {
        return LanguageSettingsManager.GetLocalizedText(text, translatedText);
    }

    public string GetLocalizedSpeaker()
    {
        return LanguageSettingsManager.GetLocalizedText(speaker, translatedSpeaker);
    }
}
