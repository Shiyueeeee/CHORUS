using UnityEngine;
using UnityEngine.SceneManagement;

public class SweepingSpotlightHazard2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform detectionOrigin;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Collider2D spotlightCollider;

    [Header("Sweep")]
    [SerializeField] private float centerAngle = 0f;
    [SerializeField] private float sweepAngle = 45f;
    [SerializeField] private float sweepSpeed = 1f;
    [SerializeField] [Range(0f, 360f)] private float startPhase = 0f;

    [Header("Detection")]
    [SerializeField] private bool useSpotlightCollider = true;
    [SerializeField] private float detectionDistance = 6f;
    [SerializeField] [Range(1f, 179f)] private float coneAngle = 40f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private bool useObstacleBlocking = false;

    [Header("Penalty")]
    [SerializeField] private float hitCooldown = 0.5f;
    [SerializeField] private bool resetPlayerVelocity = true;

    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private readonly Collider2D[] spotlightOverlapResults = new Collider2D[16];
    private float hitCooldownTimer;
    private float currentLocalAngle;

    private void Reset()
    {
        detectionOrigin = transform;
    }

    private void Awake()
    {
        if (detectionOrigin == null)
            detectionOrigin = transform;
    }

    private void Update()
    {
        UpdateSweepRotation();

        if (hitCooldownTimer > 0f)
            hitCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (hitCooldownTimer > 0f || detectionOrigin == null)
            return;

        if (respawnPoint == null && !SceneSpawnData.HasCheckpointForScene(SceneManager.GetActiveScene().name))
            return;

        if (useSpotlightCollider && spotlightCollider != null)
        {
            TryHandleSpotlightColliderHit();
            return;
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(detectionOrigin.position, detectionDistance, overlapResults);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapResults[i];
            if (hit == null || !hit.CompareTag("Player"))
                continue;

            if (IsPlayerHidden(hit))
                continue;

            if (!IsInsideCone(hit.bounds.center))
                continue;

            if (useObstacleBlocking && IsBlocked(hit.bounds.center))
                continue;

            SendPlayerBack(hit);
            break;
        }
    }

    private void UpdateSweepRotation()
    {
        float phaseRadians = startPhase * Mathf.Deg2Rad;
        float offset = Mathf.Sin(Time.time * sweepSpeed + phaseRadians) * sweepAngle;
        currentLocalAngle = centerAngle + offset;
        transform.localRotation = Quaternion.Euler(0f, 0f, currentLocalAngle);
    }

    private void TryHandleSpotlightColliderHit()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;

        int hitCount = spotlightCollider.Overlap(filter, spotlightOverlapResults);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = spotlightOverlapResults[i];
            if (hit == null || !hit.CompareTag("Player"))
                continue;

            if (IsPlayerHidden(hit))
                continue;

            SendPlayerBack(hit);
            break;
        }
    }

    private bool IsInsideCone(Vector2 targetPosition)
    {
        Vector2 origin = detectionOrigin.position;
        Vector2 toTarget = targetPosition - origin;
        float sqrDistance = toTarget.sqrMagnitude;

        if (sqrDistance <= 0.0001f)
            return true;

        if (sqrDistance > detectionDistance * detectionDistance)
            return false;

        Vector2 forward = GetForwardDirection();
        float angleToTarget = Vector2.Angle(forward, toTarget.normalized);
        return angleToTarget <= coneAngle * 0.5f;
    }

    private bool IsBlocked(Vector2 targetPosition)
    {
        Vector2 origin = detectionOrigin.position;
        Vector2 direction = targetPosition - origin;
        float distance = direction.magnitude;
        if (distance <= 0.0001f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, distance, obstacleMask);
        return hit.collider != null && !hit.collider.CompareTag("Player");
    }

    private void SendPlayerBack(Collider2D playerCollider)
    {
        Rigidbody2D playerRigidbody = playerCollider.attachedRigidbody;
        Transform playerTransform = playerRigidbody != null
            ? playerRigidbody.transform
            : playerCollider.transform;

        Vector3 targetPosition = GetRespawnPosition();
        playerTransform.position = targetPosition;

        if (resetPlayerVelocity && playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.position = targetPosition;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (PlayerFacingUtility.TryResolveRespawnFacing(sceneName, respawnPoint, out bool facingRight))
            PlayerFacingUtility.TryApplyFacing(playerTransform, facingRight);

        hitCooldownTimer = Mathf.Max(0f, hitCooldown);
    }

    private static bool IsPlayerHidden(Collider2D playerCollider)
    {
        SpotlightHideTarget2D hideTarget = playerCollider.GetComponent<SpotlightHideTarget2D>();
        if (hideTarget == null && playerCollider.attachedRigidbody != null)
            hideTarget = playerCollider.attachedRigidbody.GetComponent<SpotlightHideTarget2D>();

        return hideTarget != null && hideTarget.IsHiddenFromSpotlight;
    }

    private Vector3 GetRespawnPosition()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (SceneSpawnData.HasCheckpointForScene(sceneName))
            return SceneSpawnData.checkpointPosition;

        return respawnPoint != null ? respawnPoint.position : Vector3.zero;
    }

    private Vector2 GetForwardDirection()
    {
        float radians = transform.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private void OnDrawGizmosSelected()
    {
        Transform gizmoOrigin = detectionOrigin != null ? detectionOrigin : transform;
        Vector3 origin = gizmoOrigin.position;

        float baseWorldAngle = transform.eulerAngles.z;
        float previewAngle = baseWorldAngle + (Application.isPlaying
            ? currentLocalAngle
            : centerAngle);

        Vector3 forward = DirectionFromAngle(previewAngle);
        Vector3 leftEdge = DirectionFromAngle(previewAngle - coneAngle * 0.5f);
        Vector3 rightEdge = DirectionFromAngle(previewAngle + coneAngle * 0.5f);

        Gizmos.color = new Color(1f, 0.95f, 0.2f, 0.9f);
        Gizmos.DrawLine(origin, origin + leftEdge * detectionDistance);
        Gizmos.DrawLine(origin, origin + rightEdge * detectionDistance);
        Gizmos.DrawLine(origin, origin + forward * detectionDistance);
        Gizmos.DrawWireSphere(origin, detectionDistance);

        Vector3 leftSweep = DirectionFromAngle(baseWorldAngle + centerAngle - sweepAngle);
        Vector3 rightSweep = DirectionFromAngle(baseWorldAngle + centerAngle + sweepAngle);

        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.85f);
        Gizmos.DrawLine(origin, origin + leftSweep * detectionDistance * 0.7f);
        Gizmos.DrawLine(origin, origin + rightSweep * detectionDistance * 0.7f);

        if (respawnPoint != null)
        {
            Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.9f);
            Gizmos.DrawWireSphere(respawnPoint.position, 0.25f);
        }
    }

    private static Vector3 DirectionFromAngle(float angleInDegrees)
    {
        float radians = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
    }
}
