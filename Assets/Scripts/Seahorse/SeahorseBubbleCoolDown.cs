using UnityEngine;
using UnityEngine.UI;

public class SeahorseBubbleCoolDown : MonoBehaviour
{
    public Image cooldownImage;
    public GameObject cooldownVisual;
    private bool isCooldownActive = false;
    public OctopusBubbleShield bubbleShield;

    void Start()
    {
        cooldownVisual.SetActive(false);
        cooldownImage.fillAmount = 0f; 
    }

    void Update()
    {
        if (bubbleShield.tutorialCompleted && !isCooldownActive)
        {
            cooldownVisual.SetActive(true);
            isCooldownActive = true;
        }
        if (bubbleShield.cooldownTimer > 0f)
        {
            float progress = 1f - (bubbleShield.cooldownTimer / bubbleShield.reuseCooldown);
            cooldownImage.fillAmount = Mathf.Clamp01(progress);
        }
    }

}