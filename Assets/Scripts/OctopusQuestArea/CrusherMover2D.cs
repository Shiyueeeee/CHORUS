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
    [SerializeField] [Range(0f, 1f)] private float startCycleOffset = 0f;

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

        InitializeFromCycleOffset();
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

    private void InitializeFromCycleOffset()
    {
        if (movingPart == null || topPoint == null || bottomPoint == null)
            return;

        float slamDuration = GetTravelDuration(topPoint.position, bottomPoint.position, slamSpeed);
        float retractDuration = GetTravelDuration(bottomPoint.position, topPoint.position, retractSpeed);
        float totalCycleDuration = topWaitTime + slamDuration + bottomWaitTime + retractDuration;

        if (totalCycleDuration <= 0.0001f)
        {
            movingPart.position = topPoint.position;
            lastPosition = movingPart.position;
            EnterState(CrusherState.WaitingAtTop);
            return;
        }

        float cycleTime = Mathf.Repeat(startCycleOffset, 1f) * totalCycleDuration;

        if (cycleTime < topWaitTime)
        {
            movingPart.position = topPoint.position;
            lastPosition = movingPart.position;
            EnterState(CrusherState.WaitingAtTop);
            stateTimer = topWaitTime - cycleTime;
            return;
        }

        cycleTime -= topWaitTime;
        if (cycleTime < slamDuration)
        {
            float t = slamDuration <= 0.0001f ? 1f : cycleTime / slamDuration;
            movingPart.position = Vector3.Lerp(topPoint.position, bottomPoint.position, t);
            lastPosition = movingPart.position;
            currentState = CrusherState.SlammingDown;
            CurrentVelocity = Vector2.zero;
            return;
        }

        cycleTime -= slamDuration;
        if (cycleTime < bottomWaitTime)
        {
            movingPart.position = bottomPoint.position;
            lastPosition = movingPart.position;
            EnterState(CrusherState.WaitingAtBottom);
            stateTimer = bottomWaitTime - cycleTime;
            return;
        }

        cycleTime -= bottomWaitTime;
        float retractT = retractDuration <= 0.0001f ? 1f : cycleTime / retractDuration;
        movingPart.position = Vector3.Lerp(bottomPoint.position, topPoint.position, retractT);
        lastPosition = movingPart.position;
        currentState = CrusherState.RetractingUp;
        CurrentVelocity = Vector2.zero;
    }

    private static float GetTravelDuration(Vector3 from, Vector3 to, float speed)
    {
        if (speed <= 0.0001f)
            return 0f;

        return Vector3.Distance(from, to) / speed;
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
