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
            cooldownImage.fillAmount = bubbleShield.cooldownTimer / bubbleShield.reuseCooldown;
        }
    }

}