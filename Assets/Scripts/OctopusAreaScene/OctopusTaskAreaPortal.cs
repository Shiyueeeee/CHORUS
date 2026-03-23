using UnityEngine;
using UnityEngine.SceneManagement;

public class OctopusTaskAreaPortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "OctopusQuestArea";

    private void Start()
    {
        RefreshPortalState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneTransition.LoadScene(targetSceneName);
    }

    private void RefreshPortalState()
    {
        bool shouldBeActive = GameProgress.octopusQuestAccepted &&
                              !GameProgress.octopusTaskReadyToTurnIn &&
                              !GameProgress.octopusCompleted;

        gameObject.SetActive(shouldBeActive);
    }
}
