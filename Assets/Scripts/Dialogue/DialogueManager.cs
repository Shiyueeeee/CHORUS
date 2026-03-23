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

    [Header("Dialogue SFX")]
    [SerializeField] private AudioSource dialogueSfxSource;
    [SerializeField] [Range(0f, 1f)] private float dialogueSfxBaseVolume = 1f;

    [Header("Typewriter")]
    [SerializeField] [Min(0f)] private float characterRevealInterval = 0.02f;

    private readonly List<DialogueLineData> currentLines = new List<DialogueLineData>();
    private int currentLineIndex = 0;

    private string currentSpeaker = "";
    private string currentFullText = "";
    private Coroutine typingCoroutine;
    private bool isTyping;

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

        EnsureDialogueSfxSource();
    }

    private void Update()
    {
        if (!IsDialogueActive) return;
        if (isChoiceActive) return;
        if (GlobalUIRoot.Instance != null && GlobalUIRoot.Instance.IsSettingsOpen) return;

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && IsPointerOverDialoguePanel(mouse.position.ReadValue()))
        {
            HandleDialoguePanelClick();
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
        currentFullText = line != null ? line.text ?? string.Empty : string.Empty;

        if (speakerText != null)
            speakerText.text = currentSpeaker;

        StartTypewriter(currentFullText);

        PlayLineSfx(line);
    }

    public void EndDialogue()
    {
        StopTypingCoroutine();
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
        currentFullText = "";

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

        System.Action callback = onYesSelected;
        CloseChoiceUi();
        callback?.Invoke();
    }

    public void ChooseNo()
    {
        if (!isChoiceActive)
            return;

        System.Action callback = onNoSelected;
        CloseChoiceUi();
        callback?.Invoke();
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

    private bool IsPointerOverDialoguePanel(Vector2 screenPosition)
    {
        if (dialoguePanel == null)
            return false;

        RectTransform panelRectTransform = dialoguePanel.transform as RectTransform;
        if (panelRectTransform == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(panelRectTransform, screenPosition);
    }

    private void HandleDialoguePanelClick()
    {
        if (isTyping)
        {
            CompleteCurrentLineInstantly();
            return;
        }

        ShowNextLine();
    }

    private void StartTypewriter(string fullText)
    {
        StopTypingCoroutine();

        if (dialogueText != null)
            dialogueText.text = "";

        if (string.IsNullOrEmpty(fullText) || characterRevealInterval <= 0f)
        {
            CompleteCurrentLineInstantly();
            return;
        }

        typingCoroutine = StartCoroutine(TypeLineRoutine(fullText));
    }

    private System.Collections.IEnumerator TypeLineRoutine(string fullText)
    {
        isTyping = true;

        for (int i = 1; i <= fullText.Length; i++)
        {
            if (dialogueText != null)
                dialogueText.text = fullText.Substring(0, i);

            if (i < fullText.Length)
                yield return new WaitForSecondsRealtime(characterRevealInterval);
        }

        typingCoroutine = null;
        isTyping = false;
    }

    private void CompleteCurrentLineInstantly()
    {
        StopTypingCoroutine();

        if (dialogueText != null)
            dialogueText.text = currentFullText;
    }

    private void StopTypingCoroutine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    private void EnsureDialogueSfxSource()
    {
        if (dialogueSfxSource == null)
            dialogueSfxSource = GetComponent<AudioSource>();

        if (dialogueSfxSource == null)
            dialogueSfxSource = gameObject.AddComponent<AudioSource>();

        dialogueSfxSource.playOnAwake = false;
        dialogueSfxSource.loop = false;
    }

    private void PlayLineSfx(DialogueLineData line)
    {
        if (line == null || line.sfxClip == null)
            return;

        EnsureDialogueSfxSource();
        AudioSettingsManager.ApplySfxVolume(dialogueSfxSource, dialogueSfxBaseVolume);
        dialogueSfxSource.PlayOneShot(line.sfxClip, Mathf.Clamp01(line.sfxVolumeScale));
    }
}
