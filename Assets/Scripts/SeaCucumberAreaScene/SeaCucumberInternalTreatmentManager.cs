using System.Collections;
using UnityEngine;
using TMPro;

public class SeaCucumberInternalTreatmentManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject treatmentPanel;
    [SerializeField] private TMP_Text lesionsRemainingText;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private float completionDisplayDuration = 1f;

    [Header("Cases")]
    [SerializeField] private SeaCucumberInternalPatientCase[] patientCases;

    private SeaCucumberInternalPatientCase currentCase;
    private int clickedLesionCount;
    private bool isResolvingCase;

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
            foreach (SeaCucumberLesionSpot spot in spots)
            {
                if (spot != null)
                    spot.Initialize(this);
            }
        }

        RefreshRemainingText();
    }

    public void NotifyLesionSpotClicked(SeaCucumberLesionSpot spot)
    {
        if (currentCase == null || isResolvingCase)
            return;

        clickedLesionCount++;
        RefreshRemainingText();

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
        if (lesionsRemainingText != null)
            lesionsRemainingText.text = string.Empty;

        if (AreAllCasesCompleted())
        {
            GameProgress.seaCucumberInternalTreatmentCompleted = true;
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
}
