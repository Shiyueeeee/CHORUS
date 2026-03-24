using System;
using System.Collections.Generic;
using Seahorse;
using UnityEngine;
using UnityEngine.SceneManagement;
using Environments.Ocean;


public class DolphinFollowPositionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SeahorseController playerController;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Rigidbody2D dolphinRigidbody;
    [SerializeField] private SpriteRenderer dolphinSpriteRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite swimSprite;

    [Header("Offsets")]
    [SerializeField] private Vector2 leftOffset = new Vector2(-2.2f, 0f);
    [SerializeField] private Vector2 rightOffset = new Vector2(2.2f, 0f);
    [SerializeField] private Vector2 upLeftOffset = new Vector2(-1.8f, -1.8f);
    [SerializeField] private Vector2 upRightOffset = new Vector2(1.8f, -1.8f);
    [SerializeField] private Vector2 downLeftOffset = new Vector2(-1.8f, 1.8f);
    [SerializeField] private Vector2 downRightOffset = new Vector2(1.8f, 1.8f);

    [Header("Follow")] 
    public bool isFollowing = false;
    public bool canFollow = false;
    [SerializeField] private float followSmooth = 8f;
    [SerializeField] private float inputThreshold = 0.1f;
    [SerializeField] private float sideSwitchThreshold = 0.75f;
    [SerializeField] private float rushFollowDelay = 0.1f;
    
    [Header("Bounds")]
    [SerializeField] private OceanBound oceanBound;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private float wallCastPadding = 0.02f;
    [SerializeField] private int overlapResolutionIterations = 4;

    [Header("Animation")]
    [SerializeField] private float swimFrameRate = 6f;
    [SerializeField] private float movementAnimationThreshold = 0.01f;

    [Header("Ship Follow Hint")]
    [SerializeField] private Vector3 shipFollowHintOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private Vector2 shipFollowHintSize = new Vector2(120f, 120f);
    [SerializeField] private float shipFollowHintScale = 1.5f;
    [SerializeField] private float shipFollowHintBlinkSpeed = 6f;
    [SerializeField] private Color shipFollowHintColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Sprite shipFollowHintSprite;

    private int horizontalSide = 1;
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private readonly RaycastHit2D[] castResults = new RaycastHit2D[16];
    private readonly Queue<FollowSample> followSamples = new Queue<FollowSample>();
    private ContactFilter2D solidWallFilter;
    private bool useDelayedVerticalFollow;
    private bool useRushTrailFollow;
    private bool showShipFollowHint;
    private float swimAnimationTimer;
    private GUIStyle shipFollowHintStyle;

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        showShipFollowHint = sceneName == "ShipScene";
        AutoAssignReferences();
        ConfigureRigidbody();
        ConfigureCollisionFilter();
        InitializeSide();
        useRushTrailFollow = sceneName == "DolphinFinalRushScene";
        useDelayedVerticalFollow = !useRushTrailFollow;
        isFollowing = sceneName != "DolphinAreaScene";
        canFollow = sceneName != "DolphinAreaScene";
        UpdateSpriteAnimation(false);
    }
    

    private void Update()
    {
        AutoAssignReferences();

        if (!canFollow) return;
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFollowing = !isFollowing;
        }
    }

    private void FixedUpdate()
    {
        AutoAssignReferences();

        if (playerTransform == null || bodyCollider == null || dolphinRigidbody == null)
        {
            UpdateSpriteAnimation(false);
            return;
        }

        RecordFollowSample();
        ResolveCurrentWallOverlaps();

        Vector2 move = GetMoveDirection();
        UpdateHorizontalSide(move);
        if (isFollowing)
        {
            UpdateSpriteFacing(move);
        }

        if (!isFollowing)
        {
            UpdateSpriteAnimation(false);
            return;
        }

        Vector2 currentPosition = dolphinRigidbody.position;
        Vector2 targetPosition = GetFollowTargetPosition(move);
        targetPosition = GetConstrainedTargetPosition(targetPosition);

        float lerpFactor = 1f - Mathf.Exp(-followSmooth * Time.fixedDeltaTime);
        Vector2 desiredPosition = Vector2.Lerp(currentPosition, targetPosition, lerpFactor);
        desiredPosition = GetConstrainedTargetPosition(desiredPosition);
        desiredPosition = PreventWallPenetration(currentPosition, desiredPosition);

        dolphinRigidbody.MovePosition(desiredPosition);
        UpdateSpriteAnimation((desiredPosition - currentPosition).sqrMagnitude > movementAnimationThreshold * movementAnimationThreshold);
    }

    private void AutoAssignReferences()
    {
        GameObject player = null;
        if (playerTransform == null || playerController == null || playerRigidbody == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerTransform == null && player != null)
            playerTransform = player.transform;

        if (playerController == null)
        {
            if (playerTransform != null)
                playerController = playerTransform.GetComponent<SeahorseController>();
            else if (player != null)
                playerController = player.GetComponent<SeahorseController>();
        }

        if (playerRigidbody == null)
        {
            if (playerTransform != null)
                playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
            else if (player != null)
                playerRigidbody = player.GetComponent<Rigidbody2D>();
        }

        if (oceanBound == null)
            oceanBound = FindFirstObjectByType<OceanBound>();

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (dolphinRigidbody == null)
        {
            dolphinRigidbody = GetComponent<Rigidbody2D>();

            if (dolphinRigidbody == null)
                dolphinRigidbody = gameObject.AddComponent<Rigidbody2D>();
        }

        if (dolphinSpriteRenderer == null)
        {
            dolphinSpriteRenderer = GetComponent<SpriteRenderer>();

            if (dolphinSpriteRenderer == null)
                dolphinSpriteRenderer = transform.Find("Body")?.GetComponent<SpriteRenderer>();
        }
    }

    private void InitializeSide()
    {
        if (playerTransform == null)
            return;

        horizontalSide = transform.position.x < playerTransform.position.x ? -1 : 1;
    }

    private void ConfigureRigidbody()
    {
        if (dolphinRigidbody == null)
            return;

        dolphinRigidbody.bodyType = RigidbodyType2D.Kinematic;
        dolphinRigidbody.gravityScale = 0f;
        dolphinRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        dolphinRigidbody.linearDamping = 0f;
        dolphinRigidbody.angularDamping = 0f;
        dolphinRigidbody.freezeRotation = true;
    }

    private void ConfigureCollisionFilter()
    {
        solidWallFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = false,
            useDepth = false
        };
    }

    private void UpdateHorizontalSide(Vector2 move)
    {
        if (move.x > inputThreshold)
        {
            horizontalSide = -1;
        }
        else if (move.x < -inputThreshold)
        {
            horizontalSide = 1;
        }
    }

    private Vector2 GetMoveDirection()
    {
        if (playerController != null && playerController.Move.sqrMagnitude > inputThreshold * inputThreshold)
        {
            return playerController.Move;
        }

        if (playerRigidbody != null && playerRigidbody.linearVelocity.sqrMagnitude > 0.0001f)
        {
            return playerRigidbody.linearVelocity.normalized;
        }

        return Vector2.zero;
    }

    private Vector2 GetTargetOffset(Vector2 move)
    {
        bool hasVertical = Mathf.Abs(move.y) > inputThreshold;

        if (hasVertical)
        {
            if (move.y > 0f)
                return horizontalSide < 0 ? upLeftOffset : upRightOffset;

            return horizontalSide < 0 ? downLeftOffset : downRightOffset;
        }

        return horizontalSide < 0 ? leftOffset : rightOffset;
    }

    private Vector2 GetFollowTargetPosition(Vector2 move)
    {
        Vector2 targetOffset = GetTargetOffset(move);

        if (useRushTrailFollow)
        {
            Vector2 delayedPlayerPosition = GetDelayedPlayerPosition();
            float horizontalOffset = horizontalSide < 0 ? leftOffset.x : rightOffset.x;
            return delayedPlayerPosition + new Vector2(horizontalOffset, 0f);
        }

        if (useDelayedVerticalFollow)
        {
            Vector2 delayedPlayerPosition = GetDelayedPlayerPosition();
            return new Vector2(playerTransform.position.x + targetOffset.x, delayedPlayerPosition.y);
        }

        return (Vector2)playerTransform.position + targetOffset;
    }

    private void RecordFollowSample()
    {
        if (!UsesDelayedPlayerSamples() || playerTransform == null)
            return;

        float now = Time.time;
        followSamples.Enqueue(new FollowSample(now, playerTransform.position));

        while (followSamples.Count > 1 && now - followSamples.Peek().time > rushFollowDelay)
        {
            followSamples.Dequeue();
        }
    }

    private Vector2 GetDelayedPlayerPosition()
    {
        if (!UsesDelayedPlayerSamples() || followSamples.Count == 0)
            return playerTransform != null ? (Vector2)playerTransform.position : Vector2.zero;

        return followSamples.Peek().position;
    }

    private bool UsesDelayedPlayerSamples()
    {
        return useRushTrailFollow || useDelayedVerticalFollow;
    }
    
    private Vector2 GetConstrainedTargetPosition(Vector2 targetPos)
    {
        if (oceanBound == null)
            return targetPos;

        Bounds b = oceanBound.OceanBounds;
        float halfW = 0f;
        float halfH = 0f;
        
        if (bodyCollider != null)
        {
            Vector3 ext = bodyCollider.bounds.extents;
            halfW = ext.x;
            halfH = ext.y;
        }

        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;
        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        Vector2 clampedTarget = new Vector2(
            Mathf.Clamp(targetPos.x, minX, maxX),
            Mathf.Clamp(targetPos.y, minY, maxY)
        );

        if (oceanBound.TryClampPointInsideShape(clampedTarget, out Vector2 shapeClamped))
        {
            clampedTarget = shapeClamped;
        }

        return clampedTarget;
    }

    private Vector2 PreventWallPenetration(Vector2 currentPosition, Vector2 desiredPosition)
    {
        if (bodyCollider == null)
            return desiredPosition;

        Vector2 delta = desiredPosition - currentPosition;
        float distance = delta.magnitude;
        if (distance <= Mathf.Epsilon)
            return desiredPosition;

        Vector2 direction = delta / distance;
        int hitCount = bodyCollider.Cast(direction, solidWallFilter, castResults, distance + wallCastPadding);
        float allowedDistance = distance;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = castResults[i];
            if (ShouldIgnoreCollider(hit.collider))
                continue;

            allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, hit.distance - wallCastPadding));
        }

        return currentPosition + (direction * allowedDistance);
    }

    private void ResolveCurrentWallOverlaps()
    {
        if (bodyCollider == null || dolphinRigidbody == null)
            return;

        for (int iteration = 0; iteration < overlapResolutionIterations; iteration++)
        {
            int overlapCount = bodyCollider.Overlap(solidWallFilter, overlapResults);
            if (overlapCount == 0)
                break;

            Vector2 correction = Vector2.zero;

            for (int i = 0; i < overlapCount; i++)
            {
                Collider2D other = overlapResults[i];
                if (ShouldIgnoreCollider(other))
                    continue;

                ColliderDistance2D distance = bodyCollider.Distance(other);
                if (!distance.isOverlapped)
                    continue;

                correction += distance.normal * distance.distance;
            }

            if (correction.sqrMagnitude <= Mathf.Epsilon)
                break;

            dolphinRigidbody.position += correction;
        }
    }

    private bool ShouldIgnoreCollider(Collider2D other)
    {
        if (other == null || other.isTrigger)
            return true;

        if (other.attachedRigidbody == dolphinRigidbody)
            return true;

        if (playerRigidbody != null && other.attachedRigidbody == playerRigidbody)
            return true;

        if (other.CompareTag("Player"))
            return true;

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
            return true;

        return false;
    }

    private void UpdateSpriteAnimation(bool isMoving)
    {
        if (dolphinSpriteRenderer == null || idleSprite == null)
            return;

        if (!isMoving || swimSprite == null || swimFrameRate <= 0f)
        {
            swimAnimationTimer = 0f;
            dolphinSpriteRenderer.sprite = idleSprite;
            return;
        }

        swimAnimationTimer += Time.fixedDeltaTime;
        float frameDuration = 1f / swimFrameRate;
        bool useSwimSprite = Mathf.FloorToInt(swimAnimationTimer / frameDuration) % 2 == 1;
        dolphinSpriteRenderer.sprite = useSwimSprite ? swimSprite : idleSprite;
    }

    private void UpdateSpriteFacing(Vector2 move)
    {
        if (dolphinSpriteRenderer == null)
            return;

        if (PlayerFacingUtility.TryGetCurrentFacing(playerTransform, out bool playerFacingRight))
        {
            dolphinSpriteRenderer.flipX = playerFacingRight;
            return;
        }

        if (move.x > inputThreshold)
        {
            dolphinSpriteRenderer.flipX = true;
        }
        else if (move.x < -inputThreshold)
        {
            dolphinSpriteRenderer.flipX = false;
        }
    }

    private void OnGUI()
    {
        if (!showShipFollowHint || !canFollow || isFollowing || playerTransform == null)
            return;

        UnityEngine.Camera currentCamera = UnityEngine.Camera.main;
        Vector3 worldPosition = playerTransform.position + shipFollowHintOffset;
        Vector3 screenPoint = currentCamera != null
            ? currentCamera.WorldToScreenPoint(worldPosition)
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        if (screenPoint.z < 0f)
            return;

        Vector2 scaledHintSize = shipFollowHintSize * Mathf.Max(0.01f, shipFollowHintScale);
        Rect labelRect = new Rect(
            screenPoint.x - (scaledHintSize.x * 0.5f),
            Screen.height - screenPoint.y - (scaledHintSize.y * 0.5f),
            scaledHintSize.x,
            scaledHintSize.y);

        Color originalColor = GUI.color;
        Color hintColor = shipFollowHintColor;
        hintColor.a = Mathf.Lerp(0.25f, 1f, (Mathf.Sin(Time.unscaledTime * shipFollowHintBlinkSpeed) + 1f) * 0.5f);
        GUI.color = hintColor;

        if (shipFollowHintSprite != null)
        {
            DrawShipFollowHintSprite(labelRect);
        }
        else
        {
            EnsureShipFollowHintStyle();
            GUI.Label(labelRect, "F", shipFollowHintStyle);
        }

        GUI.color = originalColor;
    }

    private void DrawShipFollowHintSprite(Rect targetRect)
    {
        Texture2D texture = shipFollowHintSprite.texture;
        if (texture == null)
            return;

        Rect spriteRect = shipFollowHintSprite.rect;
        float aspect = spriteRect.height > 0f ? spriteRect.width / spriteRect.height : 1f;
        Rect drawRect = GetAspectFittedRect(targetRect, aspect);
        Rect uvRect = new Rect(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height,
            spriteRect.width / texture.width,
            spriteRect.height / texture.height);

        GUI.DrawTextureWithTexCoords(drawRect, texture, uvRect, true);
    }

    private Rect GetAspectFittedRect(Rect targetRect, float aspect)
    {
        if (aspect <= 0f)
            return targetRect;

        float targetAspect = targetRect.width / targetRect.height;
        if (targetAspect > aspect)
        {
            float width = targetRect.height * aspect;
            float x = targetRect.x + ((targetRect.width - width) * 0.5f);
            return new Rect(x, targetRect.y, width, targetRect.height);
        }

        float height = targetRect.width / aspect;
        float y = targetRect.y + ((targetRect.height - height) * 0.5f);
        return new Rect(targetRect.x, y, targetRect.width, height);
    }

    private void EnsureShipFollowHintStyle()
    {
        if (shipFollowHintStyle != null)
            return;

        shipFollowHintStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 72,
            fontStyle = FontStyle.Bold
        };
    }

    private readonly struct FollowSample
    {
        public FollowSample(float time, Vector2 position)
        {
            this.time = time;
            this.position = position;
        }

        public float time { get; }
        public Vector2 position { get; }
    }
    
}
