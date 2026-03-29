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
    [SerializeField] private TMP_Text yesButtonText;
    [SerializeField] private TMP_Text noButtonText;

    [Header("Dialogue SFX")]
    [SerializeField] private AudioSource dialogueSfxSource;
    [SerializeField] [Range(0f, 1f)] private float dialogueSfxBaseVolume = 1f;

    [Header("Localization")]
    [SerializeField] private TMP_FontAsset chineseFont;

    [Header("Typewriter")]
    [SerializeField] [Min(0f)] private float characterRevealInterval = 0.02f;
    [SerializeField] private bool playTypewriterBlip = true;
    [SerializeField] private AudioClip typewriterBlipClip;
    [SerializeField] [Range(0f, 1f)] private float typewriterBlipVolume = 0.15f;
    [SerializeField] [Min(1)] private int charactersPerBlip = 2;

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
    private AudioClip generatedTypewriterBlipClip;
    private TMP_FontAsset defaultDialogueFont;
    private TMP_FontAsset defaultSpeakerFont;
    private string currentYesLabelEnglish = "Yes";
    private string currentNoLabelEnglish = "No";
    private string currentYesLabelChinese = "是";
    private string currentNoLabelChinese = "否";

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

            if (yesButtonText == null)
                yesButtonText = yesButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(ChooseNo);

            if (noButtonText == null)
                noButtonText = noButton.GetComponentInChildren<TMP_Text>(true);
        }

        defaultDialogueFont = dialogueText != null ? dialogueText.font : null;
        defaultSpeakerFont = speakerText != null ? speakerText.font : null;

        EnsureDialogueSfxSource();
    }

    private void OnEnable()
    {
        LanguageSettingsManager.LanguageChanged += HandleLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageSettingsManager.LanguageChanged -= HandleLanguageChanged;
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
        currentSpeaker = line != null ? line.GetLocalizedSpeaker() : string.Empty;
        currentFullText = line != null ? line.GetLocalizedText() : string.Empty;

        ApplyLocalizedFonts();

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

        ApplyLocalizedFonts();

        currentLines.Clear();
        currentLineIndex = 0;
        currentFullText = "";

        onDialogueFinished?.Invoke();
    }

    public void StartYesNoChoice(
        string speaker,
        string question,
        System.Action onYes,
        System.Action onNo = null,
        string yesLabel = "Yes",
        string noLabel = "No",
        string yesLabelChinese = "是",
        string noLabelChinese = "否")
    {
        if (IsDialogueActive)
            return;

        IsDialogueActive = true;
        isChoiceActive = true;
        currentSpeaker = speaker ?? string.Empty;
        currentYesLabelEnglish = string.IsNullOrWhiteSpace(yesLabel) ? "Yes" : yesLabel;
        currentNoLabelEnglish = string.IsNullOrWhiteSpace(noLabel) ? "No" : noLabel;
        currentYesLabelChinese = string.IsNullOrWhiteSpace(yesLabelChinese) ? "是" : yesLabelChinese;
        currentNoLabelChinese = string.IsNullOrWhiteSpace(noLabelChinese) ? "否" : noLabelChinese;
        onDialogueFinished = null;
        onYesSelected = onYes;
        onNoSelected = onNo;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ApplyLocalizedFonts();

        if (speakerText != null)
            speakerText.text = currentSpeaker;

        if (dialogueText != null)
            dialogueText.text = question ?? string.Empty;

        ApplyChoiceLabels();

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

        ApplyLocalizedFonts();
        ApplyChoiceLabels();

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

            if (ShouldPlayTypewriterBlip(fullText, i))
                PlayTypewriterBlip();

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

    private bool ShouldPlayTypewriterBlip(string fullText, int revealedCharacterCount)
    {
        if (!playTypewriterBlip || string.IsNullOrEmpty(fullText))
            return false;

        int charIndex = revealedCharacterCount - 1;
        if (charIndex < 0 || charIndex >= fullText.Length)
            return false;

        char currentChar = fullText[charIndex];
        if (char.IsWhiteSpace(currentChar))
            return false;

        return revealedCharacterCount % Mathf.Max(1, charactersPerBlip) == 0 || revealedCharacterCount == 1;
    }

    private void PlayTypewriterBlip()
    {
        EnsureDialogueSfxSource();

        AudioClip clipToPlay = typewriterBlipClip != null ? typewriterBlipClip : GetGeneratedTypewriterBlipClip();
        if (clipToPlay == null)
            return;

        AudioSettingsManager.ApplySfxVolume(dialogueSfxSource, dialogueSfxBaseVolume);
        dialogueSfxSource.PlayOneShot(clipToPlay, Mathf.Clamp01(typewriterBlipVolume));
    }

    private AudioClip GetGeneratedTypewriterBlipClip()
    {
        if (generatedTypewriterBlipClip != null)
            return generatedTypewriterBlipClip;

        const int sampleRate = 44100;
        const float durationSeconds = 0.035f;
        const float frequency = 880f;

        int sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (i / (float)sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.25f;
        }

        generatedTypewriterBlipClip = AudioClip.Create("DialogueTypewriterBlip", sampleCount, 1, sampleRate, false);
        generatedTypewriterBlipClip.SetData(samples, 0);
        return generatedTypewriterBlipClip;
    }

    private void HandleLanguageChanged(SupportedLanguage language)
    {
        ApplyLocalizedFonts();

        if (!IsDialogueActive)
            return;

        if (isChoiceActive)
        {
            if (speakerText != null)
                speakerText.text = currentSpeaker;

            ApplyChoiceLabels();

            return;
        }

        if (currentLineIndex < 0 || currentLineIndex >= currentLines.Count)
            return;

        DialogueLineData line = currentLines[currentLineIndex];
        currentSpeaker = line != null ? line.GetLocalizedSpeaker() : string.Empty;
        currentFullText = line != null ? line.GetLocalizedText() : string.Empty;

        if (speakerText != null)
            speakerText.text = currentSpeaker;

        StartTypewriter(currentFullText);
    }

    private void ApplyLocalizedFonts()
    {
        TMP_FontAsset activeFont = null;
        if (LanguageSettingsManager.CurrentLanguage == SupportedLanguage.Chinese)
            activeFont = chineseFont != null ? chineseFont : LanguageSettingsManager.GetChineseUiFontOrNull();

        if (dialogueText != null)
            dialogueText.font = activeFont != null ? activeFont : defaultDialogueFont;

        if (speakerText != null)
            speakerText.font = activeFont != null ? activeFont : defaultSpeakerFont;
    }

    private void ApplyChoiceLabels()
    {
        string localizedYes = LanguageSettingsManager.GetLocalizedText(currentYesLabelEnglish, currentYesLabelChinese);
        string localizedNo = LanguageSettingsManager.GetLocalizedText(currentNoLabelEnglish, currentNoLabelChinese);

        if (yesButtonText != null)
            yesButtonText.text = localizedYes;

        if (noButtonText != null)
            noButtonText.text = localizedNo;

        TMP_FontAsset activeFont = LanguageSettingsManager.CurrentLanguage == SupportedLanguage.Chinese
            ? (chineseFont != null ? chineseFont : LanguageSettingsManager.GetChineseUiFontOrNull())
            : null;

        if (yesButtonText != null && defaultDialogueFont != null)
            yesButtonText.font = activeFont != null ? activeFont : defaultDialogueFont;

        if (noButtonText != null && defaultDialogueFont != null)
            noButtonText.font = activeFont != null ? activeFont : defaultDialogueFont;
    }
}
