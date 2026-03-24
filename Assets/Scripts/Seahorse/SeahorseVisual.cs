using System;
using UnityEngine;

namespace Seahorse
{
    public class SeahorseVisual : MonoBehaviour
    {
        [SerializeField] private SeahorseController seahorseController;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float deadzone;
        [SerializeField] private Sprite[] animationSprites = System.Array.Empty<Sprite>();
        [SerializeField] private float animationFrameRate = 8f;
        [SerializeField] private float horizontalTiltAngle = 18f;
        [SerializeField] private float tiltSmoothSpeed = 180f;

        [SerializeField] private bool isFacingRight = true;
        [SerializeField] private bool positiveScaleFacesRight = false;

        private Quaternion baseLocalRotation;
        private float animationTimer;
        private float currentTilt;

        private void Awake()
        {
            seahorseController = GetComponentInParent<SeahorseController>();
            rb = GetComponentInParent<Rigidbody2D>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            baseLocalRotation = transform.localRotation;
            SyncFacingFromScale();
            ApplyCurrentSprite();
        }

        private void Start()
        {
            if (PlayerFacingUtility.TryGetSceneDefaultFacing(out bool facingRight))
                SetFacingImmediate(facingRight);
        }

        private void Update()
        {
            UpdateAnimation();

            if (seahorseController == null) return;

            if (seahorseController.MovementMode == PlayerMovementMode.VerticalOnly)
            {
                if (!isFacingRight) SetFacing(true);
                UpdateTilt(0f);
                return;
            }
            
            float x = 0f;

            // face where the player is steering; otherwise face where it's moving.
            # region Flip Sprite
            if (Mathf.Abs(seahorseController.Move.x) > deadzone)
                x = seahorseController.Move.x;
            else if (rb != null)
                x = rb.linearVelocity.x;

            if (x > deadzone && !isFacingRight) SetFacing(true);
            else if (x < -deadzone && isFacingRight) SetFacing(false);
            # endregion

            UpdateTilt(x);
        }

        private void SetFacing(bool right)
        {
            isFacingRight = right;
            Vector3 s = transform.localScale;
            float facingSign = isFacingRight
                ? (positiveScaleFacesRight ? 1f : -1f)
                : (positiveScaleFacesRight ? -1f : 1f);
            s.x = Mathf.Abs(s.x) * facingSign;
            transform.localScale = s;
        }

        public bool IsFacingRight => isFacingRight;

        public void SetFacingImmediate(bool right)
        {
            SetFacing(right);
        }

        private void SyncFacingFromScale()
        {
            float xScale = transform.localScale.x;
            if (Mathf.Approximately(xScale, 0f)) return;

            bool hasPositiveScale = xScale > 0f;
            isFacingRight = positiveScaleFacesRight ? hasPositiveScale : !hasPositiveScale;
        }

        private void UpdateAnimation()
        {
            if (animationSprites == null || animationSprites.Length == 0)
                return;

            if (animationFrameRate <= 0f)
            {
                animationTimer = 0f;
                ApplyCurrentSprite();
                return;
            }

            animationTimer += Time.deltaTime;
            ApplyCurrentSprite();
        }

        private void ApplyCurrentSprite()
        {
            if (spriteRenderer == null || animationSprites == null || animationSprites.Length == 0)
                return;

            int frameIndex = 0;
            if (animationFrameRate > 0f)
                frameIndex = Mathf.FloorToInt(animationTimer * animationFrameRate) % animationSprites.Length;

            Sprite nextSprite = animationSprites[frameIndex];
            if (nextSprite != null)
                spriteRenderer.sprite = nextSprite;
        }

        private void UpdateTilt(float horizontalInput)
        {
            float targetTilt = 0f;
            if (horizontalInput > deadzone)
                targetTilt = -horizontalTiltAngle;
            else if (horizontalInput < -deadzone)
                targetTilt = horizontalTiltAngle;

            currentTilt = Mathf.MoveTowardsAngle(currentTilt, targetTilt, tiltSmoothSpeed * Time.deltaTime);
            transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, 0f, currentTilt);
        }
    }
}

