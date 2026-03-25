using System.Collections.Generic;
using Camera;
using UnityEngine;

public class SharkAttackController : MonoBehaviour
{
    private enum AttackState
    {
        Barrage,
        Cooldown,
        Warning,
        Dashing,
        Recovering,
        Dialogue,
        Finished
    }

    [Header("References")]
    [SerializeField] private Transform sharkTransform;
    [SerializeField] private Transform warningOrigin;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SharkEncounterManager encounterManager;
    [SerializeField] private LineRenderer warningLine;
    [SerializeField] private SharkHealth sharkHealth;
    [SerializeField] private SharkBarrageController barrageController;

    [Header("Phase 2 End Dialogue")]
    [SerializeField] private string sharkSpeakerName = "Shark";
    [SerializeField] private DialogueLineData[] phase2EndDialogueLines;

    [Header("Barrage")]
    [SerializeField] private float barrageDuration = 10f;

    [Header("Attack Timing")]
    [SerializeField] private float initialDelay = 0.75f;
    [SerializeField] private float attackCooldown = 1.25f;
    [SerializeField] private float warningDuration = 0.8f;
    [SerializeField] private float recoveryDuration = 0.4f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float maxDashDistance = 18f;
    [SerializeField] private float dashOvershootDistance = 1.5f;
    [SerializeField] private int playerDamage = 50;

    [Header("Aim Stability")]
    [SerializeField] private float closeAimLockDistance = 0.6f;
    [SerializeField] private float facingFlipThreshold = 0.2f;

    [Header("Progression")]
    [SerializeField] private int trashHitsToEndPhase = 3;

    [Header("Screen Shake")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    private AttackState currentState = AttackState.Barrage;
    private float stateTimer;
    private Vector3 dashTargetPosition;
    private bool dashResolved;
    private int trashHitCount;
    private float sharkBaseScaleX = 1f;
    private Quaternion sharkBaseRotation = Quaternion.identity;
    private Vector3 lastStableAimDirection = Vector3.left;
    private bool isFacingLeft = true;

    private void Awake()
    {
        AutoAssignReferences();
        CacheFacingScale();
        HideWarningLine();
    }

    private void OnEnable()
    {
        AutoAssignReferences();
        CacheFacingScale();
        trashHitCount = 0;
        currentState = AttackState.Barrage;
        stateTimer = barrageDuration;
        dashResolved = false;
        lastStableAimDirection = Vector3.left;
        isFacingLeft = true;
        FaceLeft();
        HideWarningLine();

        if (barrageController != null)
        {
            barrageController.BeginBarrage();
        }
    }

    private void OnDisable()
    {
        if (barrageController != null)
        {
            barrageController.EndBarrage();
        }
    }

    private void Update()
    {
        if (encounterManager != null && encounterManager.CurrentPhase != SharkEncounterPhase.AttackChallenge)
            return;

        switch (currentState)
        {
            case AttackState.Barrage:
                TickBarrage();
                break;
            case AttackState.Cooldown:
                TickCooldown();
                break;
            case AttackState.Warning:
                TickWarning();
                break;
            case AttackState.Dashing:
                TickDash();
                break;
            case AttackState.Recovering:
                TickRecovery();
                break;
        }
    }

    private void TickBarrage()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            if (barrageController != null)
            {
                barrageController.EndBarrage();
            }

            currentState = AttackState.Cooldown;
            stateTimer = initialDelay;
        }
    }

    private void TickCooldown()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            BeginWarning();
        }
    }

    private void TickWarning()
    {
        UpdateWarningTarget();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            LockDashTarget();
            BeginDash();
        }
    }

    private void TickDash()
    {
        if (sharkTransform == null)
            return;

        sharkTransform.position = Vector3.MoveTowards(
            sharkTransform.position,
            dashTargetPosition,
            dashSpeed * Time.deltaTime);

        if (!dashResolved && Vector3.Distance(sharkTransform.position, dashTargetPosition) <= 0.05f)
        {
            EndDash();
        }
    }

    private void TickRecovery()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = AttackState.Cooldown;
            stateTimer = attackCooldown;
        }
    }

    private void BeginWarning()
    {
        if (sharkTransform == null || playerTransform == null)
            return;

        currentState = AttackState.Warning;
        stateTimer = warningDuration;
        UpdateWarningTarget();
    }

    private void UpdateWarningTarget()
    {
        if (sharkTransform == null || playerTransform == null)
            return;

        Vector3 start = GetWarningStartPosition();
        Vector3 playerPosition = playerTransform.position;
        float playerDistance = Vector3.Distance(start, playerPosition);
        Vector3 direction = GetStableAimDirection(playerPosition - start, playerDistance);
        float dashDistance = Mathf.Min(maxDashDistance, playerDistance + dashOvershootDistance);
        dashDistance = Mathf.Max(dashDistance, 2f);
        dashTargetPosition = start + direction * dashDistance;
        UpdateFacing(direction);

        ShowWarningLine(start, playerPosition);
    }

    private void LockDashTarget()
    {
        UpdateWarningTarget();
    }

    private void BeginDash()
    {
        dashResolved = false;
        HideWarningLine();
        currentState = AttackState.Dashing;
    }

    private void EndDash()
    {
        if (dashResolved)
            return;

        dashResolved = true;
        HideWarningLine();
        currentState = AttackState.Recovering;
        stateTimer = recoveryDuration;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState != AttackState.Dashing || dashResolved)
            return;

        HandleDashCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (currentState != AttackState.Dashing || dashResolved)
            return;

        HandleDashCollision(other);
    }

    private void HandleDashCollision(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(playerDamage);
            }

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);

            EndDash();
            return;
        }

        SharkTrashObstacle trashObstacle = other.GetComponent<SharkTrashObstacle>();
        if (trashObstacle != null && trashObstacle.RegisterSharkHit())
        {
            trashHitCount++;

            if (sharkHealth != null)
            {
                sharkHealth.TakeDamage(1);
            }

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(shakeDuration * 1.1f, shakeMagnitude * 1.5f);

            if (trashHitCount >= trashHitsToEndPhase)
            {
                FinishAttackPhase();
                return;
            }

            EndDash();
        }
    }

    private void FinishAttackPhase()
    {
        currentState = AttackState.Dialogue;
        HideWarningLine();

        if (barrageController != null)
        {
            barrageController.EndBarrage();
        }

        List<DialogueLineData> lines = BuildDialogueList(phase2EndDialogueLines, sharkSpeakerName);
        if (lines == null || lines.Count == 0 || DialogueManager.Instance == null)
        {
            EnterQuizPhase();
            return;
        }

        DialogueManager.Instance.StartDialogue(lines, EnterQuizPhase);
    }

    private void EnterQuizPhase()
    {
        currentState = AttackState.Finished;

        if (encounterManager != null)
        {
            encounterManager.BeginQuizPhase();
        }
    }

    private List<DialogueLineData> BuildDialogueList(DialogueLineData[] sourceLines, string fallbackSpeaker)
    {
        if (sourceLines == null || sourceLines.Length == 0)
            return null;

        List<DialogueLineData> lines = new List<DialogueLineData>();
        foreach (DialogueLineData line in sourceLines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text))
                continue;

            lines.Add(new DialogueLineData
            {
                speaker = string.IsNullOrWhiteSpace(line.speaker) ? fallbackSpeaker : line.speaker,
                text = line.text
            });
        }

        return lines.Count > 0 ? lines : null;
    }

    private void ShowWarningLine(Vector3 start, Vector3 end)
    {
        if (warningLine == null)
            return;

        warningLine.enabled = true;
        warningLine.positionCount = 2;
        warningLine.SetPosition(0, start);
        warningLine.SetPosition(1, end);
    }

    private void HideWarningLine()
    {
        if (warningLine != null)
        {
            warningLine.enabled = false;
        }
    }

    private void AutoAssignReferences()
    {
        if (sharkTransform == null)
            sharkTransform = transform;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (sharkHealth == null)
        {
            sharkHealth = GetComponent<SharkHealth>();
        }
    }

    private Vector3 GetWarningStartPosition()
    {
        if (warningOrigin != null)
            return warningOrigin.position;

        return sharkTransform != null ? sharkTransform.position : transform.position;
    }

    private void CacheFacingScale()
    {
        if (sharkTransform == null)
            return;

        sharkBaseScaleX = Mathf.Abs(sharkTransform.localScale.x);
        if (sharkBaseScaleX <= 0.0001f)
        {
            sharkBaseScaleX = 1f;
        }

        sharkBaseRotation = sharkTransform.localRotation;
    }

    private Vector3 GetStableAimDirection(Vector3 rawDirection, float playerDistance)
    {
        if (playerDistance <= closeAimLockDistance)
            return lastStableAimDirection;

        if (rawDirection.sqrMagnitude <= 0.0001f)
            return lastStableAimDirection;

        lastStableAimDirection = rawDirection.normalized;
        return lastStableAimDirection;
    }

    private void UpdateFacing(Vector3 direction)
    {
        if (sharkTransform == null || direction.sqrMagnitude <= 0.0001f)
            return;

        direction.Normalize();

        if (direction.x <= -facingFlipThreshold)
        {
            isFacingLeft = true;
        }
        else if (direction.x >= facingFlipThreshold)
        {
            isFacingLeft = false;
        }

        Vector3 scale = sharkTransform.localScale;
        Vector2 forward = isFacingLeft ? Vector2.left : Vector2.right;
        float signedAngle = Vector2.SignedAngle(forward, direction);

        scale.x = isFacingLeft ? sharkBaseScaleX : -sharkBaseScaleX;
        sharkTransform.localScale = scale;
        sharkTransform.localRotation = sharkBaseRotation * Quaternion.Euler(0f, 0f, signedAngle);
    }

    private void FaceLeft()
    {
        if (sharkTransform == null)
            return;

        isFacingLeft = true;
        lastStableAimDirection = Vector3.left;

        Vector3 scale = sharkTransform.localScale;
        scale.x = sharkBaseScaleX;
        sharkTransform.localScale = scale;
        sharkTransform.localRotation = sharkBaseRotation;
    }
}
