using UnityEngine;

public class RadiationZone2D : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float damageTickInterval = 0.25f;

    private PlayerHealth playerHealthInZone;
    private OctopusRadiationOverlay radiationOverlayInZone;
    private float damageTimer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerHealthInZone = other.GetComponent<PlayerHealth>();
        radiationOverlayInZone = other.GetComponent<OctopusRadiationOverlay>();
        if (radiationOverlayInZone != null)
            radiationOverlayInZone.EnterRadiationZone();

        damageTimer = 0f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (other.GetComponent<PlayerHealth>() == playerHealthInZone)
        {
            if (radiationOverlayInZone != null)
                radiationOverlayInZone.ExitRadiationZone();

            playerHealthInZone = null;
            radiationOverlayInZone = null;
            damageTimer = 0f;
        }
    }

    private void Update()
    {
        if (playerHealthInZone == null || damageTickInterval <= 0f || damagePerSecond <= 0f)
            return;

        OctopusBubbleShield bubbleShield = playerHealthInZone.GetComponent<OctopusBubbleShield>();
        if (bubbleShield != null && bubbleShield.IsShieldActive)
        {
            damageTimer = 0f;
            return;
        }

        damageTimer += Time.deltaTime;

        while (damageTimer >= damageTickInterval)
        {
            damageTimer -= damageTickInterval;

            int damageAmount = Mathf.Max(1, Mathf.RoundToInt(damagePerSecond * damageTickInterval));
            playerHealthInZone.TakeDamage(damageAmount);

            if (playerHealthInZone == null)
                break;
        }
    }
}
