using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SeaCucumberInternalTreatmentManager : MonoBehaviour
{
    private enum InternalTutorialStep
    {
        None,
        ClickToZoom,
        ClickLesionSequence,
        Completed
    }

    [Header("UI")]
    [SerializeField] private GameObject treatmentPanel;
    [SerializeField] private TMP_Text lesionsRemainingText;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private float completionDisplayDuration = 1f;
    [SerializeField] private GameObject interactableObjectToDisable;

    [Header("Cases")]
    [SerializeField] private SeaCucumberInternalPatientCase[] patientCases;

    [Header("Tutorial")]
    [SerializeField] private SeaCucumberInternalPatientCase tutorialPatientCase;
    [SerializeField] private RectTransform tutorialArrow;
    [SerializeField] private RectTransform tutorialHintRoot;
    [SerializeField] private TMP_Text tutorialHintText;
    [SerializeField] private Vector2 tutorialArrowOffset = new Vector2(0f, 80f);
    [SerializeField] private Vector2 tutorialHintOffset = new Vector2(120f, 40f);
    [SerializeField] [TextArea] private string clickToZoomHintText = "Click to zoom";
    [SerializeField] [TextArea] private string clickLesionHintText = "Click the lesion";

    private SeaCucumberInternalPatientCase currentCase;
    private int clickedLesionCount;
    private bool isResolvingCase;
    private InternalTutorialStep tutorialStep = InternalTutorialStep.None;
    private readonly List<SeaCucumberLesionSpot> tutorialLesionSpots = new List<SeaCucumberLesionSpot>();
    private int tutorialLesionIndex;
    private RectTransform currentArrowTarget;
    private RectTransform currentHintTarget;

    public bool IsTreatmentOpen => treatmentPanel != null && treatmentPanel.activeSelf;

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

    public void OpenPatientCase(SeaCucumberInternalPatientCase patientCase)
    {
        if (patientCase == null || patientCase.IsCompleted)
            return;

        CloseCurrentZoomPanel();

        currentCase = patientCase;
        clickedLesionCount = 0;

        if (currentCase.ZoomPanel != null)
        {
            currentCase.ZoomPanel.SetActive(true);
            SeaCucumberLesionSpot[] spots = currentCase.ZoomPanel.GetComponentsInChildren<SeaCucumberLesionSpot>(true);
            tutorialLesionSpots.Clear();
            foreach (SeaCucumberLesionSpot spot in spots)
            {
                if (spot != null)
                {
                    spot.Initialize(this);
                    tutorialLesionSpots.Add(spot);
                }
            }
        }

        if (patientCase == GetTutorialPatientCase() && tutorialStep == InternalTutorialStep.ClickToZoom)
        {
            tutorialStep = InternalTutorialStep.ClickLesionSequence;
            tutorialLesionIndex = 0;
            ShowCurrentTutorialLesionHint();
        }

        RefreshRemainingText();
    }

    public void NotifyLesionSpotClicked(SeaCucumberLesionSpot spot)
    {
        if (currentCase == null || isResolvingCase)
            return;

        clickedLesionCount++;
        RefreshRemainingText();

        if (currentCase == GetTutorialPatientCase() && tutorialStep == InternalTutorialStep.ClickLesionSequence)
        {
            if (tutorialLesionIndex < tutorialLesionSpots.Count && tutorialLesionSpots[tutorialLesionIndex] == spot)
                tutorialLesionIndex++;

            ShowCurrentTutorialLesionHint();
        }

        if (clickedLesionCount >= currentCase.RequiredLesionCount)
        {
            StartCoroutine(CompleteCurrentCase());
        }
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
        tutorialLesionSpots.Clear();
        if (lesionsRemainingText != null)
            lesionsRemainingText.text = string.Empty;

        if (tutorialStep == InternalTutorialStep.ClickLesionSequence)
            CompleteTutorial();

        if (AreAllCasesCompleted())
        {
            GameProgress.seaCucumberInternalTreatmentCompleted = true;
            if (interactableObjectToDisable != null)
                interactableObjectToDisable.SetActive(false);
            ClosePanel();
        }

        isResolvingCase = false;
    }

    private void InitializeCases()
    {
        foreach (SeaCucumberInternalPatientCase patientCase in patientCases)
        {
            if (patientCase != null)
                patientCase.Initialize(this);
        }
    }

    private void CloseCurrentZoomPanel()
    {
        if (currentCase != null && currentCase.ZoomPanel != null)
            currentCase.ZoomPanel.SetActive(false);
    }

    public bool IsCaseInteractionAllowed(SeaCucumberInternalPatientCase patientCase)
    {
        if (patientCase == null)
            return false;

        if (!IsTutorialActive())
            return true;

        return patientCase == GetTutorialPatientCase();
    }

    public bool IsLesionInteractionAllowed(SeaCucumberLesionSpot spot)
    {
        if (spot == null)
            return false;

        if (tutorialStep != InternalTutorialStep.ClickLesionSequence || currentCase != GetTutorialPatientCase())
            return true;

        SeaCucumberLesionSpot currentTarget = GetCurrentTutorialLesionSpot();
        return currentTarget == null || currentTarget == spot;
    }

    private void RefreshRemainingText()
    {
        if (lesionsRemainingText == null || currentCase == null)
            return;

        int remaining = Mathf.Max(0, currentCase.RequiredLesionCount - clickedLesionCount);
        lesionsRemainingText.text = "Remaining: " + remaining;
    }

    private bool AreAllCasesCompleted()
    {
        foreach (SeaCucumberInternalPatientCase patientCase in patientCases)
        {
            if (patientCase != null && !patientCase.IsCompleted)
                return false;
        }

        return true;
    }

    private void BeginTutorial()
    {
        SeaCucumberInternalPatientCase patientCase = GetTutorialPatientCase();
        if (patientCase == null || patientCase.IsCompleted)
        {
            tutorialStep = InternalTutorialStep.Completed;
            HideTutorialVisuals();
            return;
        }

        tutorialStep = InternalTutorialStep.ClickToZoom;
        ShowTutorial(patientCase.CaseRectTransform, patientCase.CaseRectTransform, clickToZoomHintText);
    }

    private SeaCucumberInternalPatientCase GetTutorialPatientCase()
    {
        if (tutorialPatientCase != null)
            return tutorialPatientCase;

        if (patientCases != null && patientCases.Length > 0)
            return patientCases[0];

        return null;
    }

    private bool IsTutorialActive()
    {
        return tutorialStep != InternalTutorialStep.None && tutorialStep != InternalTutorialStep.Completed;
    }

    private void CompleteTutorial()
    {
        tutorialStep = InternalTutorialStep.Completed;
        HideTutorialVisuals();
    }

    private SeaCucumberLesionSpot GetCurrentTutorialLesionSpot()
    {
        while (tutorialLesionIndex < tutorialLesionSpots.Count)
        {
            SeaCucumberLesionSpot spot = tutorialLesionSpots[tutorialLesionIndex];
            if (spot != null && !spot.IsClicked)
                return spot;

            tutorialLesionIndex++;
        }

        return null;
    }

    private void ShowCurrentTutorialLesionHint()
    {
        SeaCucumberLesionSpot currentSpot = GetCurrentTutorialLesionSpot();
        if (currentSpot == null)
        {
            HideTutorialVisuals();
            return;
        }

        RectTransform target = currentSpot.SpotRectTransform;
        ShowTutorial(target, target, clickLesionHintText);
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
