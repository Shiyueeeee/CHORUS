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

    public bool IsGameOverVisible => gameOverPanel != null && gameOverPanel.activeSelf;

    private void Awake()
    {
        ResolveHPFillImage();
    }

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        ResolveHPFillImage();
        ResolveGameOverPanel();
        string sceneName = SceneManager.GetActiveScene().name;
        maxHP = DolphinSceneHealthState.GetStartingPlayerMaxHealth(sceneName, maxHP);
        currentHP = DolphinSceneHealthState.GetStartingPlayerHealth(sceneName, maxHP);
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
        if (IsValidFillImage(hpFillImage))
            return;

        if (TryResolveScreenSpaceFillImage(out Image resolvedScreenSpaceFill))
        {
            hpFillImage = resolvedScreenSpaceFill;
            return;
        }

        if (hpFillImage == null)
            return;

        Image[] nearbyImages = hpFillImage.transform.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < nearbyImages.Length; i++)
        {
            if (IsValidFillImage(nearbyImages[i]))
            {
                hpFillImage = nearbyImages[i];
                return;
            }
        }
    }

    private static bool IsValidFillImage(Image image)
    {
        return image != null && image.type == Image.Type.Filled;
    }

    private bool TryResolveScreenSpaceFillImage(out Image resolvedImage)
    {
        Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            Image candidate = images[i];
            if (!IsValidFillImage(candidate))
                continue;

            Canvas candidateCanvas = candidate.canvas;
            if (candidateCanvas == null || candidateCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            resolvedImage = candidate;
            return true;
        }

        resolvedImage = null;
        return false;
    }

    private void ResolveGameOverPanel()
    {
        if (gameOverPanel != null)
            return;

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                continue;

            Transform panelTransform = canvas.transform.Find("GameOverPanel");
            if (panelTransform == null)
                continue;

            gameOverPanel = panelTransform.gameObject;
            return;
        }
    }

    private void Die()
    {
        pendingCheckpointRespawn = HasCheckpointRespawnAvailable();

        isDead = true;
        SyncPersistentHealth();

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

        DolphinSceneHealthState.ResetAll();
        SceneTransition.LoadScene(SceneManager.GetActiveScene().name);
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
        SyncPersistentHealth();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void SyncPersistentHealth()
    {
        DolphinSceneHealthState.StorePlayerHealth(SceneManager.GetActiveScene().name, currentHP, maxHP);
    }
}
