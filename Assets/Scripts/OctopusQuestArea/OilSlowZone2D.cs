using Seahorse;
using UnityEngine;

public class OilSlowZone2D : MonoBehaviour
{
    [Header("Slow Effect")]
    [SerializeField] [Range(0.1f, 1f)] private float speedMultiplier = 0.5f;

    private SeahorseMotor playerMotorInZone;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerMotorInZone = other.GetComponent<SeahorseMotor>();
        if (playerMotorInZone != null)
            playerMotorInZone.SetSpeedMultiplier(speedMultiplier);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SeahorseMotor exitingMotor = other.GetComponent<SeahorseMotor>();
        if (exitingMotor != null && exitingMotor == playerMotorInZone)
        {
            exitingMotor.SetSpeedMultiplier(1f);
            playerMotorInZone = null;
        }
    }
}
