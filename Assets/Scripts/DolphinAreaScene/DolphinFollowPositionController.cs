using Seahorse;
using UnityEngine;

public class DolphinFollowPositionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SeahorseController playerController;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Offsets")]
    [SerializeField] private Vector2 leftOffset = new Vector2(-2.2f, 0f);
    [SerializeField] private Vector2 rightOffset = new Vector2(2.2f, 0f);
    [SerializeField] private Vector2 upLeftOffset = new Vector2(-1.8f, -1.8f);
    [SerializeField] private Vector2 upRightOffset = new Vector2(1.8f, -1.8f);
    [SerializeField] private Vector2 downLeftOffset = new Vector2(-1.8f, 1.8f);
    [SerializeField] private Vector2 downRightOffset = new Vector2(1.8f, 1.8f);

    [Header("Follow")]
    [SerializeField] private float followSmooth = 8f;
    [SerializeField] private float inputThreshold = 0.1f;
    [SerializeField] private float sideSwitchThreshold = 0.75f;

    private int horizontalSide = 1;

    private void Awake()
    {
        AutoAssignReferences();
        InitializeSide();
    }

    private void LateUpdate()
    {
        if (playerTransform == null)
            return;

        UpdateHorizontalSide();

        Vector2 move = GetMoveDirection();
        Vector3 targetPosition = playerTransform.position + (Vector3)GetTargetOffset(move);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSmooth * Time.deltaTime);
    }

    private void AutoAssignReferences()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        playerTransform = player.transform;
        if (playerController == null)
            playerController = player.GetComponent<SeahorseController>();
        if (playerRigidbody == null)
            playerRigidbody = player.GetComponent<Rigidbody2D>();
    }

    private void InitializeSide()
    {
        if (playerTransform == null)
            return;

        horizontalSide = transform.position.x < playerTransform.position.x ? -1 : 1;
    }

    private void UpdateHorizontalSide()
    {
        float relativeX = transform.position.x - playerTransform.position.x;

        if (relativeX < -sideSwitchThreshold)
        {
            horizontalSide = -1;
        }
        else if (relativeX > sideSwitchThreshold)
        {
            horizontalSide = 1;
        }
    }

    private Vector2 GetMoveDirection()
    {
        if (playerController != null)
        {
            return playerController.Move;
        }

        if (playerRigidbody != null)
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
}
