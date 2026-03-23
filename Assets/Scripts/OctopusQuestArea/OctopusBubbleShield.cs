using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class OctopusBubbleShield : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField] private GameObject bubbleVisual;
    [SerializeField] private float activeDuration = 5f;
    [SerializeField] public float reuseCooldown = 12f;
    [SerializeField] private bool startsUnlocked = false;
    [SerializeField] private PlayerHealth playerHealth;

    private float activeTimer;
    public float cooldownTimer;
    private bool isUnlocked;
    public bool tutorialCompleted;

    public bool IsShieldActive => activeTimer > 0f;
    public bool IsUnlocked => isUnlocked;
    public bool IsTutorialCompleted => tutorialCompleted;

    public event System.Action ShieldActivated;

    [Header("Blink Effect")]
    public SpriteRenderer spriteRenderer;
    public float totalBlinkDuration = 1f;
    public float blinkInterval = 0.1f;
    private bool isBlinking = false;

    private void OnEnable()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        isUnlocked = startsUnlocked;
        tutorialCompleted = false;
        activeTimer = 0f;
        cooldownTimer = 0f;
        RefreshBubbleVisual();
    }

    private void Update()
    {
        bool isGameOverVisible = playerHealth != null && playerHealth.IsGameOverVisible;
        bool canAcceptInput = IsUnlocked
            && !isGameOverVisible
            && !GlobalUIRoot.IsModalInputLocked
            && !DialogueManager.IsDialogueActive;

        if (isGameOverVisible && activeTimer > 0f)
        {
            activeTimer = 0f;
            RefreshBubbleVisual();
        }

        if (canAcceptInput && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && cooldownTimer <= 0f)
        {
            ActivateShield();
            isBlinking = true;
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
        if (activeTimer <= 1 && isBlinking)
        {
            StartCoroutine(BlinkRoutine());
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
        tutorialCompleted = true;
        RefreshBubbleVisual();
        ShieldActivated?.Invoke();
    }

    public void SetAbilityUnlocked(bool isUnlocked)
    {
        this.isUnlocked = isUnlocked;

        if (!isUnlocked)
        {
            activeTimer = 0f;
            cooldownTimer = 0f;
            tutorialCompleted = false;
        }

        RefreshBubbleVisual();
    }

    private void RefreshBubbleVisual()
    {
        if (bubbleVisual != null)
            bubbleVisual.SetActive(IsShieldActive);
    }

    private IEnumerator BlinkRoutine()
    {
        isBlinking = false;
        Debug.Log("Starting Blink Routine");

        float endTime = Time.time + totalBlinkDuration;


        while (Time.time < endTime)
        {

            spriteRenderer.enabled = !spriteRenderer.enabled;


            yield return new WaitForSeconds(blinkInterval);
        }

        

    }
}
