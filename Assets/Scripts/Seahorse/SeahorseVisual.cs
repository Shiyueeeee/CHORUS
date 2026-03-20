using System;
using UnityEngine;

namespace Seahorse
{
    public class SeahorseVisual : MonoBehaviour
    {
        [SerializeField] private SeahorseController seahorseController;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float deadzone;

        [SerializeField] private bool isFacingRight = true;
        [SerializeField] private bool positiveScaleFacesRight = false;

        private void Awake()
        {
            seahorseController = GetComponentInParent<SeahorseController>();
            rb = GetComponentInParent<Rigidbody2D>();
        }

        private void Update()
        {
            if (seahorseController == null) return;
            
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
    }
}

