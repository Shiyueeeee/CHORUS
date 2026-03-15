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
    [SerializeField] private string successMessage = "You passed the shark's test.";
    [SerializeField] private string failMessage = "Wrong answer. Think carefully and try again.";
    [SerializeField] private float correctMessageDuration = 0.8f;
    [SerializeField] private float successMessageDuration = 1.2f;

    private int currentQuestionIndex;
    private bool isResolvingAnswer;
    private Coroutine answerResolutionRoutine;

    private void OnEnable()
    {
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
            feedbackText.text = failMessage;

        currentQuestionIndex = 0;
        RefreshQuestion();
    }

    private IEnumerator HandleCorrectAnswer()
    {
        isResolvingAnswer = true;
        SetButtonsInteractable(false);

        bool isLastQuestion = currentQuestionIndex >= questions.Length - 1;

        if (feedbackText != null)
            feedbackText.text = isLastQuestion ? successMessage : correctMessage;

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
        GameProgress.sharkCompleted = true;

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
                questionText.text = "No quiz questions configured.";

            ApplyAnswers(null);
            return;
        }

        SharkQuizQuestion currentQuestion = questions[currentQuestionIndex];
        if (currentQuestion == null)
        {
            if (questionText != null)
                questionText.text = "Question data is missing.";

            ApplyAnswers(null);
            return;
        }

        if (questionText != null)
            questionText.text = currentQuestion.question;

        ApplyAnswers(currentQuestion);
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
                answerButtonTexts[i].text = question.answers[i];
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
}
