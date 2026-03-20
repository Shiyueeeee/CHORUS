using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToOctopusAreaPortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "OctopusAreaScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_OctopusQuestReturn";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneSpawnData.returnSpawnPointName = returnSpawnPointName;
        SceneManager.LoadScene(targetSceneName);
    }
}
