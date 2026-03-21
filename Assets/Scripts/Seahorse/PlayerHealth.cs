using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("UI")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private GameObject gameOverPanel;

    private bool isDead = false;
    private bool pendingCheckpointRespawn = false;
    private Rigidbody2D playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
        UpdateHPBar();

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
    }

    private void UpdateHPBar()
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = (float)currentHP / maxHP;
        }
    }

    private void Die()
    {
        pendingCheckpointRespawn = HasCheckpointRespawnAvailable();

        isDead = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void Replay()
    {
        Time.timeScale = 1f;

        if (pendingCheckpointRespawn && HasCheckpointRespawnAvailable())
        {
            RespawnAtCheckpoint();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool HasCheckpointRespawnAvailable()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return SceneSpawnData.HasCheckpointForScene(sceneName);
    }

    private void RespawnAtCheckpoint()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Vector3 checkpointPosition = SceneSpawnData.checkpointPosition;
        transform.position = checkpointPosition;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
            playerRigidbody.position = checkpointPosition;
        }

        if (SceneSpawnData.TryGetCheckpointFacing(sceneName, out bool facingRight))
            PlayerFacingUtility.TryApplyFacing(transform, facingRight);

        currentHP = maxHP;
        isDead = false;
        pendingCheckpointRespawn = false;
        UpdateHPBar();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
