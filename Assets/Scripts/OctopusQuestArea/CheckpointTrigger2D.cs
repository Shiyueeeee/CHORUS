using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointTrigger2D : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField] private Transform checkpointPoint;
    [SerializeField] private int checkpointOrder = 0;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasBeenActivated;

    private void Reset()
    {
        checkpointPoint = transform;
    }

    private void Awake()
    {
        if (checkpointPoint == null)
            checkpointPoint = transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnlyOnce && hasBeenActivated)
            return;

        string sceneName = SceneManager.GetActiveScene().name;
        int previousOrder = SceneSpawnData.HasCheckpointForScene(sceneName)
            ? SceneSpawnData.checkpointOrder
            : -1;

        bool facingRight = true;
        if (!PlayerFacingUtility.TryGetCurrentFacing(other.transform, out facingRight))
            PlayerFacingUtility.TryResolveRespawnFacing(sceneName, checkpointPoint, out facingRight);

        SceneSpawnData.SetCheckpoint(sceneName, checkpointPoint.position, checkpointOrder, facingRight);

        if (SceneSpawnData.checkpointOrder == checkpointOrder && checkpointOrder >= previousOrder)
            hasBeenActivated = true;
    }
}
