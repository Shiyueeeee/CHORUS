using UnityEngine;

public class CrusherMover2D : MonoBehaviour
{
    private enum CrusherState
    {
        WaitingAtTop,
        SlammingDown,
        WaitingAtBottom,
        RetractingUp
    }

    [Header("References")]
    [SerializeField] private Transform movingPart;
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;

    [Header("Motion")]
    [SerializeField] private float slamSpeed = 18f;
    [SerializeField] private float retractSpeed = 3f;
    [SerializeField] private float topWaitTime = 0.8f;
    [SerializeField] private float bottomWaitTime = 0.2f;
    [SerializeField] private float arrivalThreshold = 0.02f;

    private CrusherState currentState;
    private float stateTimer;
    private Vector3 lastPosition;

    public Vector2 CurrentVelocity { get; private set; }

    private void Reset()
    {
        movingPart = transform;
    }

    private void Start()
    {
        if (movingPart == null)
            movingPart = transform;

        if (topPoint != null)
            movingPart.position = topPoint.position;

        lastPosition = movingPart.position;
        EnterState(CrusherState.WaitingAtTop);
    }

    private void Update()
    {
        if (movingPart == null || topPoint == null || bottomPoint == null)
        {
            CurrentVelocity = Vector2.zero;
            return;
        }

        switch (currentState)
        {
            case CrusherState.WaitingAtTop:
                TickWaitState(topWaitTime, CrusherState.SlammingDown);
                break;
            case CrusherState.SlammingDown:
                MoveTowards(bottomPoint.position, slamSpeed, CrusherState.WaitingAtBottom);
                break;
            case CrusherState.WaitingAtBottom:
                TickWaitState(bottomWaitTime, CrusherState.RetractingUp);
                break;
            case CrusherState.RetractingUp:
                MoveTowards(topPoint.position, retractSpeed, CrusherState.WaitingAtTop);
                break;
        }
    }

    private void TickWaitState(float waitDuration, CrusherState nextState)
    {
        CurrentVelocity = Vector2.zero;
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            EnterState(nextState);
    }

    private void MoveTowards(Vector3 targetPosition, float moveSpeed, CrusherState nextState)
    {
        Vector3 nextPosition = Vector3.MoveTowards(movingPart.position, targetPosition, moveSpeed * Time.deltaTime);
        movingPart.position = nextPosition;
        CurrentVelocity = (movingPart.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = movingPart.position;

        if (Vector3.Distance(movingPart.position, targetPosition) <= arrivalThreshold)
            EnterState(nextState);
    }

    private void EnterState(CrusherState nextState)
    {
        currentState = nextState;
        stateTimer = nextState == CrusherState.WaitingAtTop
            ? topWaitTime
            : nextState == CrusherState.WaitingAtBottom
                ? bottomWaitTime
                : 0f;

        lastPosition = movingPart != null ? movingPart.position : Vector3.zero;
        if (nextState == CrusherState.WaitingAtTop || nextState == CrusherState.WaitingAtBottom)
            CurrentVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (topPoint == null || bottomPoint == null)
            return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawLine(topPoint.position, bottomPoint.position);
        Gizmos.DrawWireSphere(topPoint.position, 0.12f);
        Gizmos.DrawWireSphere(bottomPoint.position, 0.12f);
    }
}
