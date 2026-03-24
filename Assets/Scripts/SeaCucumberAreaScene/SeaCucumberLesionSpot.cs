using UnityEngine;
using UnityEngine.UI;

public class SeaCucumberLesionSpot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image visualImage;
    [SerializeField] private Sprite clickedSprite;
    [SerializeField] private Color hiddenColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color revealedColor = Color.white;

    private SeaCucumberInternalTreatmentManager manager;
    private bool isClicked;

    public RectTransform SpotRectTransform => transform as RectTransform;
    public bool IsClicked => isClicked;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (visualImage == null)
            visualImage = GetComponent<Image>();
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

        ApplyInitialVisualState();
    }

    private void OnClicked()
    {
        if (manager == null || isClicked || !manager.IsLesionInteractionAllowed(this))
            return;

        isClicked = true;
        if (button != null)
            button.interactable = false;

        ApplyClickedVisualState();

        manager.NotifyLesionSpotClicked(this);
    }

    private void ApplyInitialVisualState()
    {
        if (visualImage == null)
            return;

        visualImage.color = hiddenColor;
    }

    private void ApplyClickedVisualState()
    {
        if (visualImage == null)
            return;

        if (clickedSprite != null)
            visualImage.sprite = clickedSprite;

        visualImage.color = revealedColor;
    }
}
