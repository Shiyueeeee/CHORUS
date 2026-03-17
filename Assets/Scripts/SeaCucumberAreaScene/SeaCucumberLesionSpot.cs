using UnityEngine;
using UnityEngine.UI;

public class SeaCucumberLesionSpot : MonoBehaviour
{
    [SerializeField] private Button button;

    private SeaCucumberInternalTreatmentManager manager;
    private bool isClicked;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Initialize(SeaCucumberInternalTreatmentManager treatmentManager)
    {
        manager = treatmentManager;
        isClicked = false;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
            button.interactable = true;
        }
    }

    private void OnClicked()
    {
        if (manager == null || isClicked)
            return;

        isClicked = true;
        if (button != null)
            button.interactable = false;

        manager.NotifyLesionSpotClicked(this);
    }
}
