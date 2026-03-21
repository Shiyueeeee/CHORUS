using UnityEngine;

public class SeaweedHideZone2D : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] [Range(0f, 1f)] private float fadedAlpha = 0.4f;
    [SerializeField] [Range(0f, 1f)] private float normalAlpha = 1f;

    private int playerCountInside;

    private void Reset()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        SetAlpha(normalAlpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SpotlightHideTarget2D hideTarget = GetOrCreateHideTarget(other);
        hideTarget.EnterHiddenZone();

        playerCountInside++;
        if (playerCountInside > 0)
            SetAlpha(fadedAlpha);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        SpotlightHideTarget2D hideTarget = GetOrCreateHideTarget(other);
        hideTarget.ExitHiddenZone();

        playerCountInside = Mathf.Max(0, playerCountInside - 1);
        if (playerCountInside == 0)
            SetAlpha(normalAlpha);
    }

    private SpotlightHideTarget2D GetOrCreateHideTarget(Collider2D other)
    {
        Rigidbody2D playerRigidbody = other.attachedRigidbody;
        GameObject targetObject = playerRigidbody != null ? playerRigidbody.gameObject : other.gameObject;

        SpotlightHideTarget2D hideTarget = targetObject.GetComponent<SpotlightHideTarget2D>();
        if (hideTarget == null)
            hideTarget = targetObject.AddComponent<SpotlightHideTarget2D>();

        return hideTarget;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderers == null)
            return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            if (spriteRenderer == null)
                continue;

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
