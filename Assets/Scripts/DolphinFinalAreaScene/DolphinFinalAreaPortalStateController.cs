using UnityEngine;

public class DolphinFinalAreaPortalStateController : MonoBehaviour
{
    [SerializeField] private GameObject portalObject;

    private void Start()
    {
        RefreshPortalState();
    }

    private void OnEnable()
    {
        RefreshPortalState();
    }

    private void RefreshPortalState()
    {
        if (portalObject != null)
            portalObject.SetActive(GameProgress.dolphinCompleted);
    }
}
