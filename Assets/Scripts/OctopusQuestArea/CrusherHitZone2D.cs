using UnityEngine;
using UnityEngine.SceneManagement;

public class CrusherHitZone2D : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private Transform fallbackRespawnPoint;
    [SerializeField] private float hitCooldown = 0.3f;
    [SerializeField] private bool resetPlayerVelocity = true;

    private float hitCooldownTimer;

    private void Update()
    {
        if (hitCooldownTimer > 0f)
            hitCooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitCooldownTimer > 0f || !other.CompareTag("Player"))
            return;

        Rigidbody2D playerRigidbody = other.attachedRigidbody;
        Transform playerTransform = playerRigidbody != null
            ? playerRigidbody.transform
            : other.transform;

        Vector3 respawnPosition = GetRespawnPosition();
        playerTransform.position = respawnPosition;

        if (resetPlayerVelocity && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.position = respawnPosition;
        }

        hitCooldownTimer = Mathf.Max(0f, hitCooldown);
    }

    private Vector3 GetRespawnPosition()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (SceneSpawnData.HasCheckpointForScene(sceneName))
            return SceneSpawnData.checkpointPosition;

        return fallbackRespawnPoint != null ? fallbackRespawnPoint.position : Vector3.zero;
    }
}
