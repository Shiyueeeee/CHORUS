using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public static bool IsDialogueActive { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerText;

    private readonly List<DialogueLineData> currentLines = new List<DialogueLineData>();
    private int currentLineIndex = 0;

    private string currentSpeaker = "";

    private System.Action onDialogueFinished;

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
    }

    private void Update()
    {
        if (!IsDialogueActive) return;

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

        IsDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

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

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (speakerText != null)
            speakerText.text = "";

        currentLines.Clear();
        currentLineIndex = 0;

        Debug.Log($"Dialogue with {currentSpeaker} ended");

        onDialogueFinished?.Invoke();
    }
}
