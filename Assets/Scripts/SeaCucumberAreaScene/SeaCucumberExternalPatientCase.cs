using UnityEngine;
using UnityEngine.UI;

public class SeaCucumberExternalPatientCase : MonoBehaviour
{
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject zoomPanel;

    private SeaCucumberExternalTreatmentManager manager;
    private bool isCompleted;

    public GameObject ZoomPanel => zoomPanel;
    public bool IsCompleted => isCompleted;

    private void Awake()
    {
        if (openButton == null)
            openButton = GetComponent<Button>();
    }

    public void Initialize(SeaCucumberExternalTreatmentManager treatmentManager)
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
        if (manager == null || isCompleted)
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
