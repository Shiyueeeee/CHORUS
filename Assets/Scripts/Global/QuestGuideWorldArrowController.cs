using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestGuideWorldArrowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform arrowTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private QuestGuideSceneReferences sceneReferences;
    [SerializeField] private SpriteRenderer arrowSpriteRenderer;
    [SerializeField] private UnityEngine.Camera worldCamera;

    [Header("Placement")]
    [SerializeField] private float distanceFromPlayer = 1.5f;
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float localZOffset = 0f;
    [SerializeField] private float cameraPadding = 0.5f;

    private void Awake()
    {
        CacheReferences();
        SetArrowVisible(false);
    }

    private void Update()
    {
        CacheReferences();
        RefreshArrow();
    }

    private void CacheReferences()
    {
        if (arrowTransform == null)
            arrowTransform = transform;

        if (arrowSpriteRenderer == null && arrowTransform != null)
            arrowSpriteRenderer = arrowTransform.GetComponent<SpriteRenderer>();

        if (worldCamera == null)
            worldCamera = UnityEngine.Camera.main;

        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                playerTransform = playerObject.transform;
        }

        if (sceneReferences == null)
            sceneReferences = FindFirstObjectByType<QuestGuideSceneReferences>();
    }

    private void RefreshArrow()
    {
        if (arrowTransform == null || playerTransform == null || sceneReferences == null)
        {
            SetArrowVisible(false);
            return;
        }

        if (!QuestGuideObjectiveResolver.TryGetCurrentObjective(out QuestGuideObjective objective) || !objective.IsTalkObjective)
        {
            SetArrowVisible(false);
            return;
        }

        if (!TryResolveTargetTransform(objective, out Transform targetTransform))
        {
            SetArrowVisible(false);
            return;
        }

        Vector3 playerPosition = playerTransform.position;
        Vector3 targetPosition = targetTransform.position;
        Vector2 direction = targetPosition - playerPosition;

        if (direction.sqrMagnitude < 0.0001f)
        {
            SetArrowVisible(false);
            return;
        }

        float distanceToTarget = direction.magnitude;
        float displayDistance = Mathf.Min(distanceFromPlayer, distanceToTarget);
        Vector2 normalizedDirection = direction.normalized;
        Vector3 worldArrowPosition = new Vector3(
            playerPosition.x + normalizedDirection.x * displayDistance,
            playerPosition.y + normalizedDirection.y * displayDistance,
            playerPosition.z + localZOffset);
        arrowTransform.position = worldArrowPosition;

        if (worldCamera != null)
            ClampArrowInsideCamera();

        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
        arrowTransform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        SetArrowVisible(true);
    }

    private bool TryResolveTargetTransform(QuestGuideObjective objective, out Transform targetTransform)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == objective.TargetSceneName)
            return sceneReferences.TryGetNpcAnchor(objective.AnchorId, out targetTransform);

        return sceneReferences.TryGetPortalAnchor(objective.TargetSceneName, out targetTransform);
    }

    private void SetArrowVisible(bool visible)
    {
        if (arrowSpriteRenderer != null)
        {
            if (arrowSpriteRenderer.enabled != visible)
                arrowSpriteRenderer.enabled = visible;

            return;
        }

        if (arrowTransform != null)
        {
            Renderer rendererComponent = arrowTransform.GetComponent<Renderer>();
            if (rendererComponent != null)
                rendererComponent.enabled = visible;
        }
    }

    private void ClampArrowInsideCamera()
    {
        if (arrowTransform == null || worldCamera == null || !worldCamera.orthographic)
            return;

        Vector3 cameraPosition = worldCamera.transform.position;
        float halfHeight = worldCamera.orthographicSize;
        float halfWidth = halfHeight * worldCamera.aspect;

        float minX = cameraPosition.x - halfWidth + cameraPadding;
        float maxX = cameraPosition.x + halfWidth - cameraPadding;
        float minY = cameraPosition.y - halfHeight + cameraPadding;
        float maxY = cameraPosition.y + halfHeight - cameraPadding;

        Vector3 worldPosition = arrowTransform.position;
        worldPosition.x = Mathf.Clamp(worldPosition.x, minX, maxX);
        worldPosition.y = Mathf.Clamp(worldPosition.y, minY, maxY);
        arrowTransform.position = worldPosition;
    }
}
