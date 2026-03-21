using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SeaCucumberExternalTreatmentManager : MonoBehaviour
{
    private enum ExternalTutorialStep
    {
        None,
        ClickToZoom,
        DragTrashSequence,
        Completed
    }

    [Header("UI")]
    [SerializeField] private GameObject treatmentPanel;
    [SerializeField] private RectTransform dragLayer;
    [SerializeField] private TMP_Text trashRemainingText;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private GameObject sharedTrashBinObject;
    [SerializeField] private float completionDisplayDuration = 1f;
    [SerializeField] private GameObject interactableObjectToDisable;

    [Header("Cases")]
    [SerializeField] private SeaCucumberExternalPatientCase[] patientCases;
    [SerializeField] private SeaCucumberTrashBinDropZone trashBinDropZone;

    [Header("Tutorial")]
    [SerializeField] private SeaCucumberExternalPatientCase tutorialPatientCase;
    [SerializeField] private RectTransform tutorialArrow;
    [SerializeField] private RectTransform tutorialHintRoot;
    [SerializeField] private TMP_Text tutorialHintText;
    [SerializeField] private Vector2 tutorialArrowOffset = new Vector2(0f, 80f);
    [SerializeField] private Vector2 tutorialHintOffset = new Vector2(120f, 40f);
    [SerializeField] [TextArea] private string clickToZoomHintText = "Click to zoom";
    [SerializeField] [TextArea] private string dragToTrashBinHintText = "Drag to the trash bin";

    private SeaCucumberExternalPatientCase currentCase;
    private int removedTrashCount;
    private int requiredTrashCount;
    private bool isResolvingCase;
    private ExternalTutorialStep tutorialStep = ExternalTutorialStep.None;
    private readonly List<SeaCucumberTrashPiece> tutorialTrashPieces = new List<SeaCucumberTrashPiece>();
    private int tutorialTrashIndex;
    private RectTransform currentArrowTarget;
    private RectTransform currentHintTarget;

    public bool IsTreatmentOpen => treatmentPanel != null && treatmentPanel.activeSelf;
    public RectTransform DragLayer => dragLayer;

    private void Update()
    {
        RefreshTutorialVisualPositions();
    }

    private void Start()
    {
        InitializeCases();

        if (treatmentPanel != null)
            treatmentPanel.SetActive(false);

        if (completionText != null)
            completionText.gameObject.SetActive(false);

        if (trashBinDropZone != null)
            trashBinDropZone.Initialize(this);

        SetSharedTrashBinActive(false);
    }

    public void TryOpenPanel()
    {
        if (!GameProgress.seaCucumberTriageCompleted || isResolvingCase || AreAllCasesCompleted())
            return;

        if (treatmentPanel != null && treatmentPanel.activeSelf)
            return;

        if (treatmentPanel != null)
            treatmentPanel.SetActive(true);

        BeginTutorial();

        if (GlobalUIRoot.Instance != null)
            GlobalUIRoot.Instance.SetExternalModalInputLock(true);
    }

    public void ClosePanel()
    {
        if (treatmentPanel != null && !treatmentPanel.activeSelf)
            return;

        CloseCurrentZoomPanel();

        if (treatmentPanel != null)
            treatmentPanel.SetActive(false);

        HideTutorialVisuals();

        if (GlobalUIRoot.Instance != null)
            GlobalUIRoot.Instance.SetExternalModalInputLock(false);
    }

    public void OpenPatientCase(SeaCucumberExternalPatientCase patientCase)
    {
        if (patientCase == null || patientCase.IsCompleted)
            return;

        CloseCurrentZoomPanel();

        currentCase = patientCase;
        removedTrashCount = 0;
        requiredTrashCount = 0;

        if (currentCase.ZoomPanel != null)
        {
            currentCase.ZoomPanel.SetActive(true);
            SetSharedTrashBinActive(true);
            SeaCucumberTrashPiece[] trashPieces = currentCase.ZoomPanel.GetComponentsInChildren<SeaCucumberTrashPiece>(true);
            tutorialTrashPieces.Clear();
            foreach (SeaCucumberTrashPiece trashPiece in trashPieces)
            {
                if (trashPiece != null)
                {
                    trashPiece.Initialize(this);
                    requiredTrashCount++;
                    tutorialTrashPieces.Add(trashPiece);
                }
            }
        }

        if (patientCase == GetTutorialPatientCase() && tutorialStep == ExternalTutorialStep.ClickToZoom)
        {
            tutorialStep = ExternalTutorialStep.DragTrashSequence;
            tutorialTrashIndex = 0;
            ShowCurrentTutorialTrashHint();
        }

        RefreshRemainingText();
    }

    public void NotifyTrashRemoved(SeaCucumberTrashPiece trashPiece)
    {
        if (currentCase == null || isResolvingCase || trashPiece == null || trashPiece.IsRemoved)
            return;

        trashPiece.RemoveIntoBin();
        removedTrashCount++;
        RefreshRemainingText();

        if (currentCase == GetTutorialPatientCase() && tutorialStep == ExternalTutorialStep.DragTrashSequence)
        {
            if (tutorialTrashIndex < tutorialTrashPieces.Count && tutorialTrashPieces[tutorialTrashIndex] == trashPiece)
                tutorialTrashIndex++;

            ShowCurrentTutorialTrashHint();
        }

        if (removedTrashCount >= requiredTrashCount)
            StartCoroutine(CompleteCurrentCase());
    }

    private IEnumerator CompleteCurrentCase()
    {
        isResolvingCase = true;

        if (completionText != null)
            completionText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(completionDisplayDuration);

        if (completionText != null)
            completionText.gameObject.SetActive(false);

        if (currentCase != null)
            currentCase.MarkCompleted();

        CloseCurrentZoomPanel();
        currentCase = null;
        tutorialTrashPieces.Clear();

        if (trashRemainingText != null)
            trashRemainingText.text = string.Empty;

        if (tutorialStep == ExternalTutorialStep.DragTrashSequence)
            CompleteTutorial();

        if (AreAllCasesCompleted())
        {
            GameProgress.seaCucumberExternalTreatmentCompleted = true;
            if (interactableObjectToDisable != null)
                interactableObjectToDisable.SetActive(false);
            ClosePanel();
        }

        isResolvingCase = false;
    }

    private void InitializeCases()
    {
        foreach (SeaCucumberExternalPatientCase patientCase in patientCases)
        {
            if (patientCase != null)
                patientCase.Initialize(this);
        }
    }

    private void CloseCurrentZoomPanel()
    {
        if (currentCase != null && currentCase.ZoomPanel != null)
            currentCase.ZoomPanel.SetActive(false);

        SetSharedTrashBinActive(false);
    }

    public bool IsCaseInteractionAllowed(SeaCucumberExternalPatientCase patientCase)
    {
        if (patientCase == null)
            return false;

        if (!IsTutorialActive())
            return true;

        return patientCase == GetTutorialPatientCase();
    }

    public bool IsTrashInteractionAllowed(SeaCucumberTrashPiece trashPiece)
    {
        if (trashPiece == null)
            return false;

        if (tutorialStep != ExternalTutorialStep.DragTrashSequence || currentCase != GetTutorialPatientCase())
            return true;

        SeaCucumberTrashPiece currentTarget = GetCurrentTutorialTrashPiece();
        return currentTarget == null || currentTarget == trashPiece;
    }

    private void RefreshRemainingText()
    {
        if (trashRemainingText == null || currentCase == null)
            return;

        int remaining = Mathf.Max(0, requiredTrashCount - removedTrashCount);
        trashRemainingText.text = "Remaining: " + remaining;
    }

    private bool AreAllCasesCompleted()
    {
        foreach (SeaCucumberExternalPatientCase patientCase in patientCases)
        {
            if (patientCase != null && !patientCase.IsCompleted)
                return false;
        }

        return true;
    }

    private void SetSharedTrashBinActive(bool isActive)
    {
        if (sharedTrashBinObject != null)
            sharedTrashBinObject.SetActive(isActive);
    }

    private void BeginTutorial()
    {
        SeaCucumberExternalPatientCase patientCase = GetTutorialPatientCase();
        if (patientCase == null || patientCase.IsCompleted)
        {
            tutorialStep = ExternalTutorialStep.Completed;
            HideTutorialVisuals();
            return;
        }

        tutorialStep = ExternalTutorialStep.ClickToZoom;
        ShowTutorial(patientCase.CaseRectTransform, patientCase.CaseRectTransform, clickToZoomHintText);
    }

    private SeaCucumberExternalPatientCase GetTutorialPatientCase()
    {
        if (tutorialPatientCase != null)
            return tutorialPatientCase;

        if (patientCases != null && patientCases.Length > 0)
            return patientCases[0];

        return null;
    }

    private bool IsTutorialActive()
    {
        return tutorialStep != ExternalTutorialStep.None && tutorialStep != ExternalTutorialStep.Completed;
    }

    private void CompleteTutorial()
    {
        tutorialStep = ExternalTutorialStep.Completed;
        HideTutorialVisuals();
    }

    private SeaCucumberTrashPiece GetCurrentTutorialTrashPiece()
    {
        while (tutorialTrashIndex < tutorialTrashPieces.Count)
        {
            SeaCucumberTrashPiece trashPiece = tutorialTrashPieces[tutorialTrashIndex];
            if (trashPiece != null && !trashPiece.IsRemoved)
                return trashPiece;

            tutorialTrashIndex++;
        }

        return null;
    }

    private void ShowCurrentTutorialTrashHint()
    {
        SeaCucumberTrashPiece currentTrashPiece = GetCurrentTutorialTrashPiece();
        if (currentTrashPiece == null)
        {
            HideTutorialVisuals();
            return;
        }

        RectTransform target = currentTrashPiece.TrashRectTransform;
        ShowTutorial(target, target, dragToTrashBinHintText);
    }

    private void ShowTutorial(RectTransform arrowTarget, RectTransform hintTarget, string hintText)
    {
        currentArrowTarget = arrowTarget;
        currentHintTarget = hintTarget;

        if (tutorialArrow != null)
            tutorialArrow.gameObject.SetActive(currentArrowTarget != null);

        if (tutorialHintRoot != null)
            tutorialHintRoot.gameObject.SetActive(currentHintTarget != null && !string.IsNullOrWhiteSpace(hintText));

        if (tutorialHintText != null)
            tutorialHintText.text = hintText;

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
        if (tutorialArrow != null && tutorialArrow.gameObject.activeSelf && currentArrowTarget != null)
            tutorialArrow.position = currentArrowTarget.position + (Vector3)tutorialArrowOffset;

        if (tutorialHintRoot != null && tutorialHintRoot.gameObject.activeSelf && currentHintTarget != null)
            tutorialHintRoot.position = currentHintTarget.position + (Vector3)tutorialHintOffset;
    }
}
