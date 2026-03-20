using System.Collections;
using UnityEngine;
using TMPro;

public class SeaCucumberExternalTreatmentManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject treatmentPanel;
    [SerializeField] private RectTransform dragLayer;
    [SerializeField] private TMP_Text trashRemainingText;
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private GameObject sharedTrashBinObject;
    [SerializeField] private float completionDisplayDuration = 1f;

    [Header("Cases")]
    [SerializeField] private SeaCucumberExternalPatientCase[] patientCases;
    [SerializeField] private SeaCucumberTrashBinDropZone trashBinDropZone;

    private SeaCucumberExternalPatientCase currentCase;
    private int removedTrashCount;
    private int requiredTrashCount;
    private bool isResolvingCase;

    public bool IsTreatmentOpen => treatmentPanel != null && treatmentPanel.activeSelf;
    public RectTransform DragLayer => dragLayer;

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
            foreach (SeaCucumberTrashPiece trashPiece in trashPieces)
            {
                if (trashPiece != null)
                {
                    trashPiece.Initialize(this);
                    requiredTrashCount++;
                }
            }
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

        if (trashRemainingText != null)
            trashRemainingText.text = string.Empty;

        if (AreAllCasesCompleted())
        {
            GameProgress.seaCucumberExternalTreatmentCompleted = true;
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
}
