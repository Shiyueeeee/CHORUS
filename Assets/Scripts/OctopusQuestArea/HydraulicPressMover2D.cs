using UnityEngine;

public class HydraulicPressMover2D : MonoBehaviour
{
    [Header("Moving Part")]
    [SerializeField] private Transform movingPart;

    [Header("Path")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arrivalThreshold = 0.02f;

    private Transform currentTarget;
    private Vector3 lastPosition;

    public Vector2 CurrentVelocity { get; private set; }

    private void Start()
    {
        if (movingPart == null)
            movingPart = transform;

        lastPosition = movingPart.position;
        currentTarget = rightPoint != null ? rightPoint : leftPoint;
    }

    private void Update()
    {
        if (leftPoint == null || rightPoint == null || currentTarget == null)
        {
            CurrentVelocity = Vector2.zero;
            return;
        }

        Vector3 nextPosition = Vector3.MoveTowards(movingPart.position, currentTarget.position, moveSpeed * Time.deltaTime);
        movingPart.position = nextPosition;
        CurrentVelocity = (movingPart.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = movingPart.position;

        if (Vector3.Distance(movingPart.position, currentTarget.position) <= arrivalThreshold)
        {
            currentTarget = currentTarget == rightPoint ? leftPoint : rightPoint;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (leftPoint == null || rightPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftPoint.position, rightPoint.position);
    }
}
