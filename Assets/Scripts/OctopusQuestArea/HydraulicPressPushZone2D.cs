using UnityEngine;

public class HydraulicPressPushZone2D : MonoBehaviour
{
    [SerializeField] private HydraulicPressMover2D mover;
    [SerializeField] private float pushMultiplier = 1f;

    private void Reset()
    {
        if (mover == null)
            mover = GetComponentInParent<HydraulicPressMover2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (mover == null || !other.CompareTag("Player"))
            return;

        Rigidbody2D playerRigidbody = other.attachedRigidbody;
        if (playerRigidbody == null)
            return;

        Vector2 pushDelta = mover.CurrentVelocity * pushMultiplier * Time.fixedDeltaTime;
        if (pushDelta.sqrMagnitude <= 0f)
            return;

        playerRigidbody.MovePosition(playerRigidbody.position + pushDelta);
    }
}
