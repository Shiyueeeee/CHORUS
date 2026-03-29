using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SharkQuizController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SharkEncounterManager encounterManager;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TMP_Text[] answerButtonTexts;

    [Header("Questions")]
    [SerializeField] private SharkQuizQuestion[] questions;

    [Header("Feedback")]
    [SerializeField] private string correctMessage = "Correct.";
    [SerializeField] private string correctMessageChinese = "回答正确。";
    [SerializeField] private string successMessage = "You passed the shark's test.";
    [SerializeField] private string successMessageChinese = "你通过了鲨鱼的试炼。";
    [SerializeField] private string failMessage = "Wrong answer. Think carefully and try again.";
    [SerializeField] private string failMessageChinese = "回答错误。再仔细想想。";
    [SerializeField] private string noQuestionsConfiguredMessage = "No quiz questions configured.";
    [SerializeField] private string noQuestionsConfiguredMessageChinese = "还没有配置问答题目。";
    [SerializeField] private string missingQuestionDataMessage = "Question data is missing.";
    [SerializeField] private string missingQuestionDataMessageChinese = "题目数据缺失。";
    [SerializeField] private float correctMessageDuration = 0.8f;
    [SerializeField] private float successMessageDuration = 1.2f;

    private int currentQuestionIndex;
    private bool isResolvingAnswer;
    private Coroutine answerResolutionRoutine;
    private TMP_FontAsset defaultQuestionFont;
    private TMP_FontAsset defaultFeedbackFont;
    private TMP_FontAsset[] defaultAnswerFonts;

    private void Awake()
    {
        CacheDefaultFonts();
    }

    private void OnEnable()
    {
        LanguageSettingsManager.LanguageChanged += HandleLanguageChanged;

        currentQuestionIndex = 0;
        isResolvingAnswer = false;

        if (answerResolutionRoutine != null)
        {
            StopCoroutine(answerResolutionRoutine);
            answerResolutionRoutine = null;
        }

        if (feedbackText != null)
            feedbackText.text = string.Empty;

        RefreshQuestion();
    }

    private void OnDisable()
    {
        LanguageSettingsManager.LanguageChanged -= HandleLanguageChanged;
    }

    public void SelectAnswer(int answerIndex)
    {
        if (isResolvingAnswer)
            return;

        if (questions == null || questions.Length == 0)
            return;

        SharkQuizQuestion currentQuestion = questions[currentQuestionIndex];
        if (currentQuestion == null)
            return;

        if (answerIndex == currentQuestion.correctAnswerIndex)
        {
            answerResolutionRoutine = StartCoroutine(HandleCorrectAnswer());
            return;
        }

        if (feedbackText != null)
            feedbackText.text = GetLocalizedFailMessage();

        currentQuestionIndex = 0;
        RefreshQuestion();
    }

    private IEnumerator HandleCorrectAnswer()
    {
        isResolvingAnswer = true;
        SetButtonsInteractable(false);

        bool isLastQuestion = currentQuestionIndex >= questions.Length - 1;

        if (feedbackText != null)
            feedbackText.text = isLastQuestion ? GetLocalizedSuccessMessage() : GetLocalizedCorrectMessage();

        yield return new WaitForSeconds(isLastQuestion ? successMessageDuration : correctMessageDuration);

        if (isLastQuestion)
        {
            CompleteQuiz();
            yield break;
        }

        currentQuestionIndex++;
        if (feedbackText != null)
            feedbackText.text = string.Empty;

        isResolvingAnswer = false;
        answerResolutionRoutine = null;
        RefreshQuestion();
    }

    private void CompleteQuiz()
    {
        if (encounterManager != null)
            encounterManager.CompleteEncounter();

        isResolvingAnswer = false;
        answerResolutionRoutine = null;
    }

    private void RefreshQuestion()
    {
        if (questions == null || questions.Length == 0)
        {
            if (questionText != null)
                questionText.text = GetLocalizedNoQuestionsConfiguredMessage();

            ApplyAnswers(null);
            ApplyLocalizedFonts();
            return;
        }

        SharkQuizQuestion currentQuestion = questions[currentQuestionIndex];
        if (currentQuestion == null)
        {
            if (questionText != null)
                questionText.text = GetLocalizedMissingQuestionDataMessage();

            ApplyAnswers(null);
            ApplyLocalizedFonts();
            return;
        }

        if (questionText != null)
            questionText.text = currentQuestion.GetLocalizedQuestion();

        ApplyAnswers(currentQuestion);
        ApplyLocalizedFonts();
        SetButtonsInteractable(true);
    }

    private void ApplyAnswers(SharkQuizQuestion question)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool hasAnswer = question != null &&
                question.answers != null &&
                i < question.answers.Length &&
                !string.IsNullOrWhiteSpace(question.answers[i]);

            if (answerButtons[i] != null)
                answerButtons[i].gameObject.SetActive(hasAnswer);

            if (hasAnswer && i < answerButtonTexts.Length && answerButtonTexts[i] != null)
                answerButtonTexts[i].text = question.GetLocalizedAnswer(i);
        }
    }

    private void SetButtonsInteractable(bool isInteractable)
    {
        foreach (Button answerButton in answerButtons)
        {
            if (answerButton != null)
                answerButton.interactable = isInteractable;
        }
    }

    private void HandleLanguageChanged(SupportedLanguage language)
    {
        RefreshQuestion();

        if (feedbackText != null && !string.IsNullOrWhiteSpace(feedbackText.text))
        {
            if (feedbackText.text == correctMessage || feedbackText.text == correctMessageChinese)
                feedbackText.text = GetLocalizedCorrectMessage();
            else if (feedbackText.text == successMessage || feedbackText.text == successMessageChinese)
                feedbackText.text = GetLocalizedSuccessMessage();
            else if (feedbackText.text == failMessage || feedbackText.text == failMessageChinese)
                feedbackText.text = GetLocalizedFailMessage();
        }

        ApplyLocalizedFonts();
    }

    private void CacheDefaultFonts()
    {
        defaultQuestionFont = questionText != null ? questionText.font : null;
        defaultFeedbackFont = feedbackText != null ? feedbackText.font : null;

        if (answerButtonTexts == null)
            return;

        defaultAnswerFonts = new TMP_FontAsset[answerButtonTexts.Length];
        for (int i = 0; i < answerButtonTexts.Length; i++)
        {
            defaultAnswerFonts[i] = answerButtonTexts[i] != null ? answerButtonTexts[i].font : null;
        }
    }

    private void ApplyLocalizedFonts()
    {
        TMP_FontAsset chineseFont = LanguageSettingsManager.GetChineseUiFontOrNull();
        bool useChineseFont = LanguageSettingsManager.CurrentLanguage == SupportedLanguage.Chinese && chineseFont != null;

        if (questionText != null)
            questionText.font = useChineseFont ? chineseFont : defaultQuestionFont;

        if (feedbackText != null)
            feedbackText.font = useChineseFont ? chineseFont : defaultFeedbackFont;

        if (answerButtonTexts == null)
            return;

        for (int i = 0; i < answerButtonTexts.Length; i++)
        {
            if (answerButtonTexts[i] == null)
                continue;

            TMP_FontAsset defaultFont = defaultAnswerFonts != null && i < defaultAnswerFonts.Length ? defaultAnswerFonts[i] : null;
            answerButtonTexts[i].font = useChineseFont ? chineseFont : defaultFont;
        }
    }

    private string GetLocalizedCorrectMessage()
    {
        return LanguageSettingsManager.GetLocalizedText(correctMessage, correctMessageChinese);
    }

    private string GetLocalizedSuccessMessage()
    {
        return LanguageSettingsManager.GetLocalizedText(successMessage, successMessageChinese);
    }

    private string GetLocalizedFailMessage()
    {
        return LanguageSettingsManager.GetLocalizedText(failMessage, failMessageChinese);
    }

    private string GetLocalizedNoQuestionsConfiguredMessage()
    {
        return LanguageSettingsManager.GetLocalizedText(noQuestionsConfiguredMessage, noQuestionsConfiguredMessageChinese);
    }

    private string GetLocalizedMissingQuestionDataMessage()
    {
        return LanguageSettingsManager.GetLocalizedText(missingQuestionDataMessage, missingQuestionDataMessageChinese);
    }
}
