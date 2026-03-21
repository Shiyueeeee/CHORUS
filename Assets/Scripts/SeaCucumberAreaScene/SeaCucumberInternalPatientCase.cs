using UnityEngine;
using UnityEngine.UI;

public class SeaCucumberInternalPatientCase : MonoBehaviour
{
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject zoomPanel;
    [SerializeField] private int requiredLesionCount = 1;

    private SeaCucumberInternalTreatmentManager manager;
    private bool isCompleted;

    public GameObject ZoomPanel => zoomPanel;
    public int RequiredLesionCount => requiredLesionCount;
    public bool IsCompleted => isCompleted;
    public RectTransform CaseRectTransform => transform as RectTransform;

    private void Awake()
    {
        if (openButton == null)
            openButton = GetComponent<Button>();
    }

    public void Initialize(SeaCucumberInternalTreatmentManager treatmentManager)
    {
        manager = treatmentManager;

        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenCase);
        }

        if (zoomPanel != null)
            zoomPanel.SetActive(false);

        gameObject.SetActive(!isCompleted);
    }

    public void OpenCase()
    {
        if (manager == null || isCompleted || !manager.IsCaseInteractionAllowed(this))
            return;

        manager.OpenPatientCase(this);
    }

    public void MarkCompleted()
    {
        isCompleted = true;

        if (zoomPanel != null)
            zoomPanel.SetActive(false);

        gameObject.SetActive(false);
    }
}
