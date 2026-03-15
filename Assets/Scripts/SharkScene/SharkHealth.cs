using UnityEngine;
using UnityEngine.UI;

public class SharkHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 3;
    [SerializeField] private int currentHP = 3;

    [Header("UI")]
    [SerializeField] private Image hpFillImage;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsDepleted => currentHP <= 0;

    private void OnEnable()
    {
        currentHP = maxHP;
        UpdateHPBar();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDepleted)
            return;

        currentHP -= amount;
        if (currentHP < 0)
            currentHP = 0;

        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
        }
    }
}
