using Environments.Ocean;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seahorse
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SeahorseController))]
    public class SeahorseDash : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SeahorseController seahorseController;
        [SerializeField] private SeahorseMotor seahorseMotor;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private OceanBound oceanBound;

        [Header("Dash")]
        [SerializeField] private float dashDistance = 4f;
        [SerializeField] private float dashDuration = 0.08f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private float longCooldown = 2.5f;
        [SerializeField] private int dashesBeforeLongCooldown = 2;
        [SerializeField] private float chainedDashResetDelay = 4f;
        [SerializeField] private Vector2 defaultDashDirection = Vector2.right;
        [SerializeField] private float directionDeadzone = 0.01f;

        public bool IsDashing => dashTimer > 0f;
        public float CooldownRemaining => cooldownTimer;
        public float CooldownDuration => currentCooldownDuration;
        public float CooldownProgress01 => cooldownTimer > 0f && currentCooldownDuration > 0f
            ? 1f - (cooldownTimer / currentCooldownDuration)
            : 1f;

        private Vector2 lastDashDirection = Vector2.right;
        private Vector2 dashDirection = Vector2.right;
        private float dashTimer;
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

            if (oceanBound == null)
                oceanBound = FindFirstObjectByType<OceanBound>();

            lastDashDirection = GetDefaultDirection();
            dashDirection = lastDashDirection;
        }

        private void OnEnable()
        {
            dashTimer = 0f;
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
            dashTimer = 0f;
            currentCooldownDuration = 0f;

            if (wasDashing && rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
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
            if (!IsDashing || rb == null)
                return;

            float safeDuration = Mathf.Max(0.01f, dashDuration);
            float stepTime = Mathf.Min(Time.fixedDeltaTime, dashTimer);
            float dashSpeed = dashDistance / safeDuration;
            Vector2 nextPosition = rb.position + dashDirection * dashSpeed * stepTime;

            rb.MovePosition(ClampInsideOceanBounds(nextPosition));
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0f)
                EndDash();
        }

        private bool CanUseDash()
        {
            return !GlobalUIRoot.IsModalInputLocked && !DialogueManager.IsDialogueActive;
        }

        private void StartDash()
        {
            if (rb == null)
                return;

            if (Time.time - lastDashTime > chainedDashResetDelay)
                chainedDashes = 0;

            dashDirection = ResolveDashDirection();
            dashTimer = Mathf.Max(0.01f, dashDuration);
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
            dashTimer = 0f;
            rb.linearVelocity = Vector2.zero;

            if (disabledMotorForDash && seahorseMotor != null)
                seahorseMotor.enabled = true;

            disabledMotorForDash = false;
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
