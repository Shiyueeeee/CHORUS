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

    private int horizontalSide = 1;
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private readonly RaycastHit2D[] castResults = new RaycastHit2D[16];
    private readonly Queue<FollowSample> followSamples = new Queue<FollowSample>();
    private ContactFilter2D solidWallFilter;
    private bool useDelayedVerticalFollow;
    private bool useRushTrailFollow;

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        AutoAssignReferences();
        ConfigureRigidbody();
        ConfigureCollisionFilter();
        InitializeSide();
        useRushTrailFollow = sceneName == "DolphinFinalRushScene";
        useDelayedVerticalFollow = !useRushTrailFollow;
        isFollowing = sceneName != "DolphinAreaScene";
        canFollow = sceneName != "DolphinAreaScene";
    }
    

    private void Update()
    {
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
            return;

        RecordFollowSample();
        ResolveCurrentWallOverlaps();

        Vector2 move = GetMoveDirection();
        UpdateHorizontalSide(move);

        if (!isFollowing)
            return;

        Vector2 currentPosition = dolphinRigidbody.position;
        Vector2 targetPosition = GetFollowTargetPosition(move);
        targetPosition = GetConstrainedTargetPosition(targetPosition);

        float lerpFactor = 1f - Mathf.Exp(-followSmooth * Time.fixedDeltaTime);
        Vector2 desiredPosition = Vector2.Lerp(currentPosition, targetPosition, lerpFactor);
        desiredPosition = GetConstrainedTargetPosition(desiredPosition);
        desiredPosition = PreventWallPenetration(currentPosition, desiredPosition);

        dolphinRigidbody.MovePosition(desiredPosition);
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
