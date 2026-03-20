using UnityEngine;

public class ScenePlayerSpawnSetter : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        if (playerTransform == null || string.IsNullOrEmpty(SceneSpawnData.returnSpawnPointName))
            return;

        GameObject spawnPoint = GameObject.Find(SceneSpawnData.returnSpawnPointName);
        if (spawnPoint != null)
            playerTransform.position = spawnPoint.transform.position;

        SceneSpawnData.returnSpawnPointName = null;
    }
}
