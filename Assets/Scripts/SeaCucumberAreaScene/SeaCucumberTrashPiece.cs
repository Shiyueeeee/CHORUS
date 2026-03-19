using UnityEngine;
using UnityEngine.EventSystems;

public class SeaCucumberTrashPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform dragVisual;
    [SerializeField] private CanvasGroup canvasGroup;

    private SeaCucumberExternalTreatmentManager manager;
    private RectTransform rectTransform;
    private Vector2 dragPointerOffset;
    private Transform dragOriginParent;
    private Vector2 dragOriginAnchoredPosition;
    private int dragOriginSiblingIndex;
    private bool isRemoved;

    public bool IsRemoved => isRemoved;

    private void Awake()
    {
        CacheReferences();
    }

    private void CacheReferences()
    {
        rectTransform = dragVisual != null ? dragVisual : GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(SeaCucumberExternalTreatmentManager treatmentManager)
    {
        CacheReferences();
        manager = treatmentManager;
        isRemoved = false;
        gameObject.SetActive(true);

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CacheReferences();

        if (isRemoved || rectTransform == null || manager == null || !manager.IsTreatmentOpen)
            return;

        dragOriginParent = rectTransform.parent;
        dragOriginAnchoredPosition = rectTransform.anchoredPosition;
        dragOriginSiblingIndex = rectTransform.GetSiblingIndex();

        rectTransform.SetParent(manager.DragLayer, true);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                manager.DragLayer,
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

        if (isRemoved || rectTransform == null || manager == null || !manager.IsTreatmentOpen)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                manager.DragLayer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointerPosition))
        {
            rectTransform.anchoredPosition = localPointerPosition + dragPointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRemoved)
            return;

        ReturnToOrigin();
    }

    public void RemoveIntoBin()
    {
        isRemoved = true;
        gameObject.SetActive(false);
    }

    public void ReturnToOrigin()
    {
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
