using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeaCucumberPatientCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private SeaCucumberPatientType patientType;
    [SerializeField] private RectTransform dragVisual;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image previewSourceImage;

    private SeaCucumberTriageManager triageManager;
    private RectTransform rectTransform;
    private bool isAssigned;
    private bool hasBeenPreviewed;
    private Vector2 dragPointerOffset;
    private Transform dragOriginParent;
    private Vector2 dragOriginAnchoredPosition;
    private int dragOriginSiblingIndex;

    public SeaCucumberPatientType PatientType => patientType;
    public bool IsAssigned => isAssigned;
    public bool CanBeDragged => hasBeenPreviewed;
    public Sprite PreviewSprite => previewSourceImage != null ? previewSourceImage.sprite : null;
    public Color PreviewColor => previewSourceImage != null ? previewSourceImage.color : Color.white;

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        rectTransform = dragVisual != null ? dragVisual : GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (previewSourceImage == null)
            previewSourceImage = (dragVisual != null ? dragVisual.GetComponent<Image>() : GetComponent<Image>());
    }

    public void Initialize(SeaCucumberTriageManager manager)
    {
        CacheReferences();
        triageManager = manager;
        isAssigned = false;
        hasBeenPreviewed = false;
    }

    public void PlaceIntoContainer(Transform parent, Vector2 targetAnchoredPosition, int siblingIndex, bool assigned)
    {
        CacheReferences();

        if (rectTransform == null)
        {
            Debug.LogWarning("SeaCucumberPatientCard requires a RectTransform on the card itself or in Drag Visual: " + name, this);
            return;
        }

        isAssigned = assigned;

        if (parent != null)
        {
            rectTransform.SetParent(parent, false);
            rectTransform.SetSiblingIndex(siblingIndex);
        }

        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = targetAnchoredPosition;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CacheReferences();

        if (rectTransform == null || isAssigned || triageManager == null || !triageManager.IsPuzzleOpen || triageManager.IsPreviewOpen || !hasBeenPreviewed)
            return;

        dragOriginParent = rectTransform.parent;
        dragOriginAnchoredPosition = rectTransform.anchoredPosition;
        dragOriginSiblingIndex = rectTransform.GetSiblingIndex();

        rectTransform.SetParent(triageManager.DragLayer, true);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                triageManager.DragLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
        {
            dragPointerOffset = rectTransform.anchoredPosition - localPointerPosition;
        }
        else
        {
            dragPointerOffset = Vector2.zero;
        }

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        CacheReferences();

        if (rectTransform == null || isAssigned || triageManager == null || !triageManager.IsPuzzleOpen || triageManager.IsPreviewOpen || !hasBeenPreviewed)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                triageManager.DragLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
        {
            rectTransform.anchoredPosition = localPointerPosition + dragPointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAssigned || !hasBeenPreviewed)
            return;

        ReturnToOrigin();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (triageManager == null || !triageManager.IsPuzzleOpen || triageManager.IsPreviewOpen || hasBeenPreviewed)
            return;

        triageManager.ShowPatientPreview(this);
    }

    public void SetAssigned(bool assigned)
    {
        isAssigned = assigned;
    }

    public void UnlockDraggingAfterPreview()
    {
        hasBeenPreviewed = true;
    }

    public void ReturnToOrigin()
    {
        CacheReferences();

        if (rectTransform == null)
            return;

        if (dragOriginParent != null)
        {
            rectTransform.SetParent(dragOriginParent, false);
            rectTransform.SetSiblingIndex(dragOriginSiblingIndex);
        }

        rectTransform.anchoredPosition = dragOriginAnchoredPosition;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}
