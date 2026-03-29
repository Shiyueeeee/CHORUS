using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SeaCucumberTriageManager : MonoBehaviour
{
    private enum TriageTutorialStep
    {
        None,
        ClickToZoom,
        PreviewHint,
        DragToZone,
        Completed
    }

    [Header("Scene Objects")]
    [SerializeField] private GameObject patientPileObject;
    [SerializeField] private GameObject sortedPatientsRoot;

    [Header("Puzzle UI")]
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private Transform leftCardsRoot;
    [SerializeField] private RectTransform dragLayer;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private GameObject patientPreviewPanel;
    [SerializeField] private Image patientPreviewImage;
    [SerializeField] private GameObject patientGroupObject;
    [SerializeField] private SeaCucumberPatientCard[] patientCards;
    [SerializeField] private SeaCucumberPatientDropZone[] dropZones;
    [SerializeField] private float completionPanelDuration = 1f;

    [Header("Left Zone Layout")]
    [SerializeField] private Vector2 leftZoneTopLeftPadding = new Vector2(24f, 24f);
    [SerializeField] private Vector2 leftZoneCellSpacing = new Vector2(120f, 120f);
    [SerializeField] private int leftZoneColumns = 2;

    [Header("Tutorial")]
    [SerializeField] private SeaCucumberPatientCard tutorialPatientCard;
    [SerializeField] private RectTransform tutorialArrow;
    [SerializeField] private RectTransform tutorialHintRoot;
    [SerializeField] private TMP_Text tutorialHintText;
    [SerializeField] private RectTransform previewCloseButtonTarget;
    [SerializeField] private RectTransform previewHintTarget;
    [SerializeField] private Vector2 clickToZoomArrowOffset = new Vector2(0f, 80f);
    [SerializeField] private Vector2 clickToZoomHintOffset = new Vector2(120f, 40f);
    [SerializeField] private Vector2 previewArrowOffset = new Vector2(0f, 80f);
    [SerializeField] private Vector2 previewHintOffset = new Vector2(-220f, 0f);
    [SerializeField] private Vector2 dragToZoneArrowOffset = new Vector2(0f, 80f);
    [SerializeField] private Vector2 dragToZoneHintOffset = new Vector2(120f, 40f);
    [SerializeField] [TextArea] private string clickToZoomHintText = "Click to zoom";
    [SerializeField] [TextArea] private string clickToZoomHintTextChinese = "点击查看";
    [SerializeField] [TextArea] private string previewDiagnosisHintText = "This patient isn't tangled in trash. It should be an internal case.";
    [SerializeField] [TextArea] private string previewDiagnosisHintTextChinese = "这个病人没有被垃圾缠住，应该归到内部病症。";
    [SerializeField] [TextArea] private string dragToCorrectZoneHintText = "Drag to the correct zone";
    [SerializeField] [TextArea] private string dragToCorrectZoneHintTextChinese = "拖到正确区域";
    [SerializeField] private string completionTextValue = "Complete";
    [SerializeField] private string completionTextValueChinese = "完成";

    private int sortedCardCount;
    private bool isCompleting;
    private readonly List<SeaCucumberPatientCard> leftZoneCards = new List<SeaCucumberPatientCard>();
    private SeaCucumberPatientCard previewingCard;
    private TriageTutorialStep tutorialStep = TriageTutorialStep.None;
    private RectTransform currentArrowTarget;
    private RectTransform currentHintTarget;
    private TMP_FontAsset defaultCompletionFont;
    private TMP_FontAsset defaultTutorialHintFont;

    public bool IsPuzzleOpen => puzzlePanel != null && puzzlePanel.activeSelf;
    public bool IsPreviewOpen => patientPreviewPanel != null && patientPreviewPanel.activeSelf;
    public RectTransform DragLayer => dragLayer;

    private void Update()
    {
        RefreshTutorialVisualPositions();
    }

    private void Awake()
    {
        defaultCompletionFont = completionText != null ? completionText.font : null;
        defaultTutorialHintFont = tutorialHintText != null ? tutorialHintText.font : null;
    }

    private void OnEnable()
    {
        LanguageSettingsManager.LanguageChanged += HandleLanguageChanged;
        ApplyLocalizedFonts();
    }

    private void OnDisable()
    {
        LanguageSettingsManager.LanguageChanged -= HandleLanguageChanged;
    }

    private void Start()
    {
        InitializePuzzleState();
        ApplyWorldState();
        SetPuzzleOpen(false);
    }

    public void TryOpenPuzzle()
    {
        if (!GameProgress.seaCucumberQuestAccepted || GameProgress.seaCucumberTriageCompleted || isCompleting)
            return;

        ResetCardsToLeftZone();
        SetPuzzleOpen(true);
        BeginTutorial();
    }

    public void ReturnCardToLeftZone(SeaCucumberPatientCard card)
    {
        if (card == null || leftZoneCards.Contains(card))
            return;

        leftZoneCards.Add(card);
        card.SetAssigned(false);
        LayoutLeftZoneCards();
    }

    public void AssignCardToDropZone(SeaCucumberPatientCard card, SeaCucumberPatientDropZone zone)
    {
        if (card == null || zone == null)
            return;

        leftZoneCards.Remove(card);
        LayoutLeftZoneCards();

        card.SetAssigned(true);
        zone.AddCard(card);
        if (card == GetTutorialPatientCard() && tutorialStep == TriageTutorialStep.DragToZone)
            CompleteTutorial();
        NotifyCardSorted(card);
    }

    public void NotifyCardSorted(SeaCucumberPatientCard card)
    {
        sortedCardCount++;
        if (sortedCardCount >= patientCards.Length)
        {
            StartCoroutine(CompletePuzzle());
        }
    }

    private void InitializePuzzleState()
    {
        sortedCardCount = 0;
        isCompleting = false;
        RefreshPatientCardsFromGroup();

        foreach (SeaCucumberPatientCard card in patientCards)
        {
            if (card != null)
                card.Initialize(this);
        }

        foreach (SeaCucumberPatientDropZone zone in dropZones)
        {
            if (zone != null)
                zone.Initialize(this);
        }

        if (completionText != null)
        {
            completionText.gameObject.SetActive(false);
            completionText.text = GetLocalizedCompletionText();
        }

        if (patientPreviewPanel != null)
            patientPreviewPanel.SetActive(false);

        HideTutorialVisuals();
    }

    private IEnumerator CompletePuzzle()
    {
        isCompleting = true;
        GameProgress.seaCucumberTriageCompleted = true;

        if (completionText != null)
            completionText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(completionPanelDuration);

        if (completionText != null)
            completionText.gameObject.SetActive(false);

        SetPuzzleOpen(false);
        ApplyWorldState();
        isCompleting = false;
    }

    private void ApplyWorldState()
    {
        if (patientPileObject != null)
            patientPileObject.SetActive(!GameProgress.seaCucumberTriageCompleted);

        if (sortedPatientsRoot != null)
            sortedPatientsRoot.SetActive(GameProgress.seaCucumberTriageCompleted);
    }

    private void SetPuzzleOpen(bool isOpen)
    {
        if (puzzlePanel != null)
            puzzlePanel.SetActive(isOpen);

        if (!isOpen && patientPreviewPanel != null)
            patientPreviewPanel.SetActive(false);

        if (!isOpen)
            HideTutorialVisuals();

        if (GlobalUIRoot.Instance != null)
            GlobalUIRoot.Instance.SetExternalModalInputLock(isOpen);
    }

    public void ShowPatientPreview(SeaCucumberPatientCard card)
    {
        if (card == null || patientPreviewPanel == null)
            return;

        previewingCard = card;

        if (patientPreviewImage != null)
        {
            patientPreviewImage.sprite = card.PreviewSprite;
            patientPreviewImage.color = card.PreviewColor;
            patientPreviewImage.preserveAspect = true;
        }

        patientPreviewPanel.SetActive(true);

        if (card == GetTutorialPatientCard() && tutorialStep == TriageTutorialStep.ClickToZoom)
        {
            tutorialStep = TriageTutorialStep.PreviewHint;
            ShowTutorial(
                previewCloseButtonTarget,
                previewHintTarget != null ? previewHintTarget : patientPreviewImage != null ? patientPreviewImage.rectTransform : null,
                LanguageSettingsManager.GetLocalizedText(previewDiagnosisHintText, previewDiagnosisHintTextChinese),
                previewArrowOffset,
                previewHintOffset);
        }
    }

    public void ClosePatientPreview()
    {
        SeaCucumberPatientCard closedCard = previewingCard;

        if (previewingCard != null)
        {
            previewingCard.UnlockDraggingAfterPreview();
            previewingCard = null;
        }

        if (patientPreviewPanel != null)
            patientPreviewPanel.SetActive(false);

        if (closedCard == GetTutorialPatientCard() && tutorialStep == TriageTutorialStep.PreviewHint)
        {
            tutorialStep = TriageTutorialStep.DragToZone;
            RectTransform cardTarget = closedCard.CardRectTransform;
            ShowTutorial(
                cardTarget,
                cardTarget,
                LanguageSettingsManager.GetLocalizedText(dragToCorrectZoneHintText, dragToCorrectZoneHintTextChinese),
                dragToZoneArrowOffset,
                dragToZoneHintOffset);
        }
    }

    public bool IsCardInteractionAllowed(SeaCucumberPatientCard card)
    {
        if (card == null)
            return false;

        if (!IsTutorialActive())
            return true;

        return card == GetTutorialPatientCard();
    }

    private void RefreshPatientCardsFromGroup()
    {
        if (patientGroupObject == null)
            return;

        patientCards = patientGroupObject.GetComponentsInChildren<SeaCucumberPatientCard>(true);
    }

    private void ResetCardsToLeftZone()
    {
        leftZoneCards.Clear();
        tutorialStep = TriageTutorialStep.None;
        previewingCard = null;
        HideTutorialVisuals();

        foreach (SeaCucumberPatientDropZone zone in dropZones)
        {
            if (zone != null)
                zone.Initialize(this);
        }

        foreach (SeaCucumberPatientCard card in patientCards)
        {
            if (card != null)
                leftZoneCards.Add(card);
        }

        LayoutLeftZoneCards();
    }

    private void LayoutLeftZoneCards()
    {
        if (leftCardsRoot == null)
            return;

        int safeColumns = Mathf.Max(1, leftZoneColumns);

        for (int i = 0; i < leftZoneCards.Count; i++)
        {
            SeaCucumberPatientCard card = leftZoneCards[i];
            if (card == null)
                continue;

            int column = i % safeColumns;
            int row = i / safeColumns;
            Vector2 anchoredPosition = new Vector2(
                leftZoneTopLeftPadding.x + leftZoneCellSpacing.x * column,
                -(leftZoneTopLeftPadding.y + leftZoneCellSpacing.y * row)
            );
            card.PlaceIntoContainer(leftCardsRoot, anchoredPosition, i, false);
        }
    }

    private void BeginTutorial()
    {
        SeaCucumberPatientCard card = GetTutorialPatientCard();
        if (card == null || !leftZoneCards.Contains(card))
        {
            tutorialStep = TriageTutorialStep.Completed;
            HideTutorialVisuals();
            return;
        }

        tutorialStep = TriageTutorialStep.ClickToZoom;
        RectTransform cardTarget = card.CardRectTransform;
        ShowTutorial(
            cardTarget,
            cardTarget,
            LanguageSettingsManager.GetLocalizedText(clickToZoomHintText, clickToZoomHintTextChinese),
            clickToZoomArrowOffset,
            clickToZoomHintOffset);
    }

    private SeaCucumberPatientCard GetTutorialPatientCard()
    {
        if (tutorialPatientCard != null)
            return tutorialPatientCard;

        if (patientCards != null && patientCards.Length > 0)
            return patientCards[0];

        return null;
    }

    private bool IsTutorialActive()
    {
        return tutorialStep != TriageTutorialStep.None && tutorialStep != TriageTutorialStep.Completed;
    }

    private void CompleteTutorial()
    {
        tutorialStep = TriageTutorialStep.Completed;
        HideTutorialVisuals();
    }

    private void ShowTutorial(RectTransform arrowTarget, RectTransform hintTarget, string hintText, Vector2 arrowOffset, Vector2 hintOffset)
    {
        currentArrowTarget = arrowTarget;
        currentHintTarget = hintTarget;

        if (tutorialArrow != null)
            tutorialArrow.gameObject.SetActive(currentArrowTarget != null);

        if (tutorialHintRoot != null)
            tutorialHintRoot.gameObject.SetActive(currentHintTarget != null && !string.IsNullOrWhiteSpace(hintText));

        if (tutorialHintText != null)
            tutorialHintText.text = hintText;

        ApplyLocalizedFonts();
        RefreshTutorialVisualPositions();
    }

    private void HideTutorialVisuals()
    {
        currentArrowTarget = null;
        currentHintTarget = null;

        if (tutorialArrow != null)
            tutorialArrow.gameObject.SetActive(false);

        if (tutorialHintRoot != null)
            tutorialHintRoot.gameObject.SetActive(false);
    }

    private void RefreshTutorialVisualPositions()
    {
        Vector2 arrowOffset = GetCurrentArrowOffset();
        Vector2 hintOffset = GetCurrentHintOffset();

        if (tutorialArrow != null && tutorialArrow.gameObject.activeSelf && currentArrowTarget != null)
            tutorialArrow.position = currentArrowTarget.position + (Vector3)arrowOffset;

        if (tutorialHintRoot != null && tutorialHintRoot.gameObject.activeSelf && currentHintTarget != null)
            tutorialHintRoot.position = currentHintTarget.position + (Vector3)hintOffset;
    }

    private Vector2 GetCurrentArrowOffset()
    {
        switch (tutorialStep)
        {
            case TriageTutorialStep.ClickToZoom:
                return clickToZoomArrowOffset;
            case TriageTutorialStep.PreviewHint:
                return previewArrowOffset;
            case TriageTutorialStep.DragToZone:
                return dragToZoneArrowOffset;
            default:
                return Vector2.zero;
        }
    }

    private Vector2 GetCurrentHintOffset()
    {
        switch (tutorialStep)
        {
            case TriageTutorialStep.ClickToZoom:
                return clickToZoomHintOffset;
            case TriageTutorialStep.PreviewHint:
                return previewHintOffset;
            case TriageTutorialStep.DragToZone:
                return dragToZoneHintOffset;
            default:
                return Vector2.zero;
        }
    }

    private void HandleLanguageChanged(SupportedLanguage language)
    {
        if (completionText != null)
            completionText.text = GetLocalizedCompletionText();

        if (tutorialHintText != null && tutorialHintRoot != null && tutorialHintRoot.gameObject.activeSelf)
            tutorialHintText.text = GetCurrentLocalizedTutorialHint();

        ApplyLocalizedFonts();
    }

    private void ApplyLocalizedFonts()
    {
        TMP_FontAsset chineseFont = LanguageSettingsManager.GetChineseUiFontOrNull();
        bool useChineseFont = LanguageSettingsManager.CurrentLanguage == SupportedLanguage.Chinese && chineseFont != null;

        if (completionText != null)
            completionText.font = useChineseFont ? chineseFont : defaultCompletionFont;

        if (tutorialHintText != null)
            tutorialHintText.font = useChineseFont ? chineseFont : defaultTutorialHintFont;
    }

    private string GetLocalizedCompletionText()
    {
        return LanguageSettingsManager.GetLocalizedText(completionTextValue, completionTextValueChinese);
    }

    private string GetCurrentLocalizedTutorialHint()
    {
        switch (tutorialStep)
        {
            case TriageTutorialStep.ClickToZoom:
                return LanguageSettingsManager.GetLocalizedText(clickToZoomHintText, clickToZoomHintTextChinese);
            case TriageTutorialStep.PreviewHint:
                return LanguageSettingsManager.GetLocalizedText(previewDiagnosisHintText, previewDiagnosisHintTextChinese);
            case TriageTutorialStep.DragToZone:
                return LanguageSettingsManager.GetLocalizedText(dragToCorrectZoneHintText, dragToCorrectZoneHintTextChinese);
            default:
                return string.Empty;
        }
    }
}
