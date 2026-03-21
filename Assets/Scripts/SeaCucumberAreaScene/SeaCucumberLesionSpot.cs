using UnityEngine;
using UnityEngine.UI;

public class SeaCucumberLesionSpot : MonoBehaviour
{
    [SerializeField] private Button button;

    private SeaCucumberInternalTreatmentManager manager;
    private bool isClicked;

    public RectTransform SpotRectTransform => transform as RectTransform;
    public bool IsClicked => isClicked;

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
        if (manager == null || isClicked || !manager.IsLesionInteractionAllowed(this))
            return;

        isClicked = true;
        if (button != null)
            button.interactable = false;

        manager.NotifyLesionSpotClicked(this);
    }
}
