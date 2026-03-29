using UnityEngine;

[System.Serializable]
public class SharkQuizQuestion
{
    [TextArea(2, 4)]
    public string question;

    [TextArea(2, 4)]
    public string translatedQuestion;

    public string[] answers = new string[3];
    public string[] translatedAnswers = new string[3];

    [Min(0)]
    public int correctAnswerIndex;

    public string GetLocalizedQuestion()
    {
        return LanguageSettingsManager.GetLocalizedText(question, translatedQuestion);
    }

    public string GetLocalizedAnswer(int index)
    {
        string englishAnswer = answers != null && index >= 0 && index < answers.Length ? answers[index] : string.Empty;
        string chineseAnswer = translatedAnswers != null && index >= 0 && index < translatedAnswers.Length ? translatedAnswers[index] : string.Empty;
        return LanguageSettingsManager.GetLocalizedText(englishAnswer, chineseAnswer);
    }
}
