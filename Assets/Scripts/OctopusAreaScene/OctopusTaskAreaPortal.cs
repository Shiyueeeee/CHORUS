using UnityEngine;
using UnityEngine.SceneManagement;

public class OctopusTaskAreaPortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "OctopusQuestArea";

    private void Start()
    {
        gameObject.SetActive(GameProgress.octopusQuestAccepted);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneManager.LoadScene(targetSceneName);
    }
}
