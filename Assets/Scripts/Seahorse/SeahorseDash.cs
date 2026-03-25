using Environments.Ocean;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seahorse
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SeahorseController))]
    public class SeahorseDash : MonoBehaviour
    {
        [Header("Availability")]
        [Tooltip("Override this on a scene instance to disable dashing in that scene.")]
        [SerializeField] private bool dashEnabled = true;

        [Header("References")]
        [SerializeField] private SeahorseController seahorseController;
        [SerializeField] private SeahorseMotor seahorseMotor;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private OceanBound oceanBound;
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Dash")]
        [SerializeField] private float dashDistance = 3.25f;
        [SerializeField] private float dashDuration = 0.14f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private float longCooldown = 2.5f;
        [SerializeField] private int dashesBeforeLongCooldown = 2;
        [SerializeField] private float chainedDashResetDelay = 4f;
        [SerializeField] private Vector2 defaultDashDirection = Vector2.right;
        [SerializeField] private float directionDeadzone = 0.01f;
        [Tooltip("Normalized travel profile for the dash. X = elapsed time, Y = distance covered.")]
        [SerializeField] private AnimationCurve dashTravelProfile = null;

        public bool DashEnabled => dashEnabled;
        public bool IsDashing => activeDashDuration > 0f;
        public float CooldownRemaining => cooldownTimer;
        public float CooldownDuration => currentCooldownDuration;
        public float CooldownProgress01 => cooldownTimer > 0f && currentCooldownDuration > 0f
            ? 1f - (cooldownTimer / currentCooldownDuration)
            : 1f;

        private Vector2 lastDashDirection = Vector2.right;
        private Vector2 dashDirection = Vector2.right;
        private Vector2 dashStartPosition;
        private Vector2 dashTargetPosition;
        private float dashElapsed;
        private float activeDashDuration;
        private float cooldownTimer;
        private float currentCooldownDuration;
        private float lastDashTime = float.NegativeInfinity;
        private int chainedDashes;
        private bool disabledMotorForDash;

        private void Awake()
        {
            seahorseController = GetComponent<SeahorseController>();
            seahorseMotor = GetComponent<SeahorseMotor>();
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            playerHealth = GetComponent<PlayerHealth>();

            if (oceanBound == null)
                oceanBound = FindFirstObjectByType<OceanBound>();

            EnsureDashTravelProfile();
            lastDashDirection = GetDefaultDirection();
            dashDirection = lastDashDirection;
        }

        private void OnValidate()
        {
            dashDistance = Mathf.Max(0f, dashDistance);
            dashDuration = Mathf.Max(0.01f, dashDuration);
            dashCooldown = Mathf.Max(0f, dashCooldown);
            longCooldown = Mathf.Max(0f, longCooldown);
            chainedDashResetDelay = Mathf.Max(0f, chainedDashResetDelay);
            directionDeadzone = Mathf.Max(0f, directionDeadzone);
            EnsureDashTravelProfile();
        }

        private void OnEnable()
        {
            dashStartPosition = Vector2.zero;
            dashTargetPosition = Vector2.zero;
            dashElapsed = 0f;
            activeDashDuration = 0f;
            cooldownTimer = 0f;
            currentCooldownDuration = 0f;
            chainedDashes = 0;
            lastDashTime = float.NegativeInfinity;
            disabledMotorForDash = false;
            lastDashDirection = GetDefaultDirection();
            dashDirection = lastDashDirection;
        }

        private void OnDisable()
        {
            bool wasDashing = IsDashing || disabledMotorForDash;

            if (disabledMotorForDash && seahorseMotor != null)
                seahorseMotor.enabled = true;

            disabledMotorForDash = false;
            dashElapsed = 0f;
            activeDashDuration = 0f;
            currentCooldownDuration = 0f;

            if (wasDashing && rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            ApplyAvailabilityState();
            if (!dashEnabled)
                return;

            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();

            UpdateLastDashDirection();

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer < 0f)
                    cooldownTimer = 0f;
            }

            if (IsDashing || cooldownTimer > 0f || !CanUseDash())
                return;

            Keyboard kb = Keyboard.current;
            if (kb != null && kb.shiftKey.wasPressedThisFrame)
                StartDash();
        }

        private void FixedUpdate()
        {
            if (!dashEnabled)
            {
                ApplyAvailabilityState();
                return;
            }

            if (!IsDashing || rb == null)
                return;

            dashElapsed = Mathf.Min(activeDashDuration, dashElapsed + Time.fixedDeltaTime);
            float dashProgress = activeDashDuration > 0f ? dashElapsed / activeDashDuration : 1f;
            float travelProgress = EvaluateDashTravel(dashProgress);
            Vector2 nextPosition = Vector2.LerpUnclamped(dashStartPosition, dashTargetPosition, travelProgress);

            rb.MovePosition(ClampInsideOceanBounds(nextPosition));

            if (dashElapsed >= activeDashDuration)
                EndDash();
        }

        private bool CanUseDash()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();

            bool isGameOverVisible = playerHealth != null && playerHealth.IsGameOverVisible;
            return dashEnabled
                && !isGameOverVisible
                && !GlobalUIRoot.IsModalInputLocked
                && !DialogueManager.IsDialogueActive
                && !SeahorseController.IsGameplayInputLocked;
        }

        public void ForceCancelDash()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            cooldownTimer = Mathf.Max(cooldownTimer, 0f);
            EndDash();
        }

        private void StartDash()
        {
            if (rb == null)
                return;

            EnsureDashTravelProfile();

            if (Time.time - lastDashTime > chainedDashResetDelay)
                chainedDashes = 0;

            dashDirection = ResolveDashDirection();
            dashStartPosition = rb.position;
            dashTargetPosition = ClampInsideOceanBounds(dashStartPosition + dashDirection * dashDistance);
            dashElapsed = 0f;
            activeDashDuration = Mathf.Max(0.01f, dashDuration);
            lastDashTime = Time.time;
            chainedDashes++;

            bool useLongCooldown = chainedDashes >= Mathf.Max(2, dashesBeforeLongCooldown);
            currentCooldownDuration = useLongCooldown ? longCooldown : dashCooldown;
            cooldownTimer = currentCooldownDuration;
            if (useLongCooldown)
                chainedDashes = 0;

            if (seahorseMotor != null && seahorseMotor.enabled)
            {
                seahorseMotor.enabled = false;
                disabledMotorForDash = true;
            }
            else
            {
                disabledMotorForDash = false;
            }

            rb.linearVelocity = Vector2.zero;
            rb.WakeUp();
        }

        private void EndDash()
        {
            dashElapsed = 0f;
            activeDashDuration = 0f;
            dashStartPosition = rb != null ? rb.position : Vector2.zero;
            dashTargetPosition = dashStartPosition;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (disabledMotorForDash && seahorseMotor != null)
                seahorseMotor.enabled = true;

            disabledMotorForDash = false;
        }

        private void ApplyAvailabilityState()
        {
            if (dashEnabled)
                return;

            if (IsDashing || disabledMotorForDash)
                EndDash();

            cooldownTimer = 0f;
            currentCooldownDuration = 0f;
            chainedDashes = 0;
            lastDashTime = float.NegativeInfinity;
        }

        private void EnsureDashTravelProfile()
        {
            if (dashTravelProfile != null && dashTravelProfile.length > 0)
                return;

            dashTravelProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        private float EvaluateDashTravel(float normalizedTime)
        {
            if (dashTravelProfile == null || dashTravelProfile.length == 0)
                return Mathf.Clamp01(normalizedTime);

            return Mathf.Clamp01(dashTravelProfile.Evaluate(Mathf.Clamp01(normalizedTime)));
        }

        private void UpdateLastDashDirection()
        {
            if (seahorseController != null && seahorseController.Move.sqrMagnitude > directionDeadzone * directionDeadzone)
            {
                lastDashDirection = seahorseController.Move.normalized;
                return;
            }

            if (rb != null && rb.linearVelocity.sqrMagnitude > directionDeadzone * directionDeadzone)
                lastDashDirection = rb.linearVelocity.normalized;
        }

        private Vector2 ResolveDashDirection()
        {
            if (seahorseController != null && seahorseController.Move.sqrMagnitude > directionDeadzone * directionDeadzone)
            {
                lastDashDirection = seahorseController.Move.normalized;
                return lastDashDirection;
            }

            if (rb != null && rb.linearVelocity.sqrMagnitude > directionDeadzone * directionDeadzone)
            {
                lastDashDirection = rb.linearVelocity.normalized;
                return lastDashDirection;
            }

            return lastDashDirection.sqrMagnitude > 0f ? lastDashDirection.normalized : GetDefaultDirection();
        }

        private Vector2 GetDefaultDirection()
        {
            return defaultDashDirection.sqrMagnitude > 0f ? defaultDashDirection.normalized : Vector2.right;
        }

        private bool TryGetPlayableBounds(out float minX, out float maxX, out float minY, out float maxY)
        {
            minX = maxX = minY = maxY = 0f;
            if (oceanBound == null)
                return false;

            Bounds bounds = oceanBound.OceanBounds;
            float halfWidth = 0f;
            float halfHeight = 0f;

            if (bodyCollider != null)
            {
                Vector3 extents = bodyCollider.bounds.extents;
                halfWidth = extents.x;
                halfHeight = extents.y;
            }

            minX = bounds.min.x + halfWidth;
            maxX = bounds.max.x - halfWidth;
            minY = bounds.min.y + halfHeight;
            maxY = bounds.max.y - halfHeight;
            return true;
        }

        private Vector2 ClampInsideOceanBounds(Vector2 targetPosition)
        {
            if (!TryGetPlayableBounds(out float minX, out float maxX, out float minY, out float maxY))
                return targetPosition;

            float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);
            return new Vector2(clampedX, clampedY);
        }
    }
}
