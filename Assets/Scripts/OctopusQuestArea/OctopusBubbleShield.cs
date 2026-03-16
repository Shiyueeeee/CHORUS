using UnityEngine;
using UnityEngine.InputSystem;

public class OctopusBubbleShield : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] private GameObject bubbleVisual;
    [SerializeField] private float activeDuration = 5f;
    [SerializeField] private float reuseCooldown = 12f;

    private float activeTimer;
    private float cooldownTimer;

    public bool IsShieldActive => activeTimer > 0f;

    private void OnEnable()
    {
        activeTimer = 0f;
        cooldownTimer = 0f;
        RefreshBubbleVisual();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            ActivateShield();
        }

        if (activeTimer > 0f)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                activeTimer = 0f;
                RefreshBubbleVisual();
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f)
                cooldownTimer = 0f;
        }
    }

    private void ActivateShield()
    {
        activeTimer = activeDuration;
        cooldownTimer = reuseCooldown;
        RefreshBubbleVisual();
    }

    private void RefreshBubbleVisual()
    {
        if (bubbleVisual != null)
            bubbleVisual.SetActive(IsShieldActive);
    }
}
