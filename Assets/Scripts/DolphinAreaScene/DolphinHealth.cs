using UnityEngine;
using UnityEngine.UI;

public class DolphinHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("UI")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private GameObject gameOverPanel;

    private bool isDead = false;

    private void Awake()
    {
        ResolveHPFillImage();
    }

    private void Start()
    {
        ResolveHPFillImage();
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        maxHP = DolphinSceneHealthState.GetStartingDolphinMaxHealth(sceneName, maxHP);
        currentHP = DolphinSceneHealthState.GetStartingDolphinHealth(sceneName, maxHP);
        UpdateHPBar();
        SyncPersistentHealth();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        if (currentHP < 0)
            currentHP = 0;

        UpdateHPBar();
        SyncPersistentHealth();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;

        UpdateHPBar();
        SyncPersistentHealth();
    }

    private void UpdateHPBar()
    {
        ResolveHPFillImage();

        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = (float)currentHP / maxHP;
        }
    }

    private void ResolveHPFillImage()
    {
        if (hpFillImage != null && hpFillImage.type == Image.Type.Filled)
            return;

        Image[] images = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].type == Image.Type.Filled)
            {
                hpFillImage = images[i];
                return;
            }
        }
    }

    private void Die()
    {
        isDead = true;
        SyncPersistentHealth();

        if (gameOverPanel != null)
        {
            if (!gameOverPanel.activeSelf)
            {
                gameOverPanel.SetActive(true);
            }
        }

        Time.timeScale = 0f;
    }

    private void SyncPersistentHealth()
    {
        DolphinSceneHealthState.StoreDolphinHealth(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, currentHP, maxHP);
    }
}
