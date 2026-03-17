using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SeaCucumberTriageManager : MonoBehaviour
{
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

    private int sortedCardCount;
    private bool isCompleting;
    private readonly List<SeaCucumberPatientCard> leftZoneCards = new List<SeaCucumberPatientCard>();
    private SeaCucumberPatientCard previewingCard;

    public bool IsPuzzleOpen => puzzlePanel != null && puzzlePanel.activeSelf;
    public bool IsPreviewOpen => patientPreviewPanel != null && patientPreviewPanel.activeSelf;
    public RectTransform DragLayer => dragLayer;

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
            completionText.text = "Complete";
        }

        if (patientPreviewPanel != null)
            patientPreviewPanel.SetActive(false);
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
    }

    public void ClosePatientPreview()
    {
        if (previewingCard != null)
        {
            previewingCard.UnlockDraggingAfterPreview();
            previewingCard = null;
        }

        if (patientPreviewPanel != null)
            patientPreviewPanel.SetActive(false);
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
}
