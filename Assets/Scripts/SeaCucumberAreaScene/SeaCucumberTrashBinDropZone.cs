using UnityEngine;
using UnityEngine.EventSystems;

public class SeaCucumberTrashBinDropZone : MonoBehaviour, IDropHandler
{
    private SeaCucumberExternalTreatmentManager manager;

    public void Initialize(SeaCucumberExternalTreatmentManager treatmentManager)
    {
        manager = treatmentManager;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (manager == null || !manager.IsTreatmentOpen)
            return;

        SeaCucumberTrashPiece trashPiece = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<SeaCucumberTrashPiece>() ?? eventData.pointerDrag.GetComponentInParent<SeaCucumberTrashPiece>()
            : null;

        if (trashPiece == null || trashPiece.IsRemoved)
            return;

        manager.NotifyTrashRemoved(trashPiece);
    }
}
