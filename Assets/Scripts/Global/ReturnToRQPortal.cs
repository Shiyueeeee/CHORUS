using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToRQPortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "RQScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_MapReturn";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneSpawnData.returnSpawnPointName = returnSpawnPointName;
        SceneManager.LoadScene(targetSceneName);
    }
}
