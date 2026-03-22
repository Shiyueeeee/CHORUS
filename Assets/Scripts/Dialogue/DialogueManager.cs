using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public static bool IsDialogueActive { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private GameObject yesNoChoicePanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private readonly List<DialogueLineData> currentLines = new List<DialogueLineData>();
    private int currentLineIndex = 0;

    private string currentSpeaker = "";

    private System.Action onDialogueFinished;
    private System.Action onYesSelected;
    private System.Action onNoSelected;
    private bool isChoiceActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (speakerText != null)
            speakerText.text = "";

        if (yesNoChoicePanel != null)
            yesNoChoicePanel.SetActive(false);

        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(ChooseYes);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(ChooseNo);
        }
    }

    private void Update()
    {
        if (!IsDialogueActive) return;
        if (isChoiceActive) return;

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(List<DialogueLineData> lines, System.Action onFinish = null)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("No dialogue lines provided.");
            return;
        }

        if (IsDialogueActive)
            return;

        currentLines.Clear();
        currentLines.AddRange(lines);
        currentLineIndex = 0;

        onDialogueFinished = onFinish;
        isChoiceActive = false;
        onYesSelected = null;
        onNoSelected = null;

        IsDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (yesNoChoicePanel != null)
            yesNoChoicePanel.SetActive(false);

        ApplyCurrentLine();
    }

    private void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentLines.Count)
        {
            ApplyCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ApplyCurrentLine()
    {
        if (currentLineIndex < 0 || currentLineIndex >= currentLines.Count)
            return;

        DialogueLineData line = currentLines[currentLineIndex];
        currentSpeaker = line != null ? line.speaker : string.Empty;

        if (speakerText != null)
            speakerText.text = currentSpeaker;

        if (dialogueText != null)
            dialogueText.text = line != null ? line.text : string.Empty;
    }

    public void EndDialogue()
    {
        IsDialogueActive = false;
        isChoiceActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (yesNoChoicePanel != null)
            yesNoChoicePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (speakerText != null)
            speakerText.text = "";

        currentLines.Clear();
        currentLineIndex = 0;

        Debug.Log($"Dialogue with {currentSpeaker} ended");

        onDialogueFinished?.Invoke();
    }

    public void StartYesNoChoice(string speaker, string question, System.Action onYes, System.Action onNo = null, string yesLabel = "Yes", string noLabel = "No")
    {
        if (IsDialogueActive)
            return;

        IsDialogueActive = true;
        isChoiceActive = true;
        currentSpeaker = speaker ?? string.Empty;
        onDialogueFinished = null;
        onYesSelected = onYes;
        onNoSelected = onNo;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (speakerText != null)
            speakerText.text = currentSpeaker;

        if (dialogueText != null)
            dialogueText.text = question ?? string.Empty;

        if (yesNoChoicePanel != null)
            yesNoChoicePanel.SetActive(true);
    }

    public void ChooseYes()
    {
        if (!isChoiceActive)
            return;

        CloseChoiceUi();
        onYesSelected?.Invoke();
    }

    public void ChooseNo()
    {
        if (!isChoiceActive)
            return;

        CloseChoiceUi();
        onNoSelected?.Invoke();
    }

    private void CloseChoiceUi()
    {
        IsDialogueActive = false;
        isChoiceActive = false;

        if (yesNoChoicePanel != null)
            yesNoChoicePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (speakerText != null)
            speakerText.text = "";

        onDialogueFinished = null;
        onYesSelected = null;
        onNoSelected = null;
    }
}
