using System.Collections;
using Seahorse;
using UnityEngine;

public class TrashInteraction : MonoBehaviour, IInteractable
{
    private const string DefaultSettingsResourcePath = "CrabTrashCleanupSettings";

    [SerializeField] private CrabTrashCleanupSettings cleanupSettings;

    private bool isCleaning;
    private bool lockedPlayerInput;
    private SeahorseController lockedPlayerController;
    private SeahorseDash lockedPlayerDash;
    private Rigidbody2D lockedPlayerRb;

    public void Interact()
    {
        if (isCleaning)
            return;

        isCleaning = true;
        DisableInteraction();
        LockPlayerForCleanup();

        CrabTrashCleanupSettings settings = ResolveSettings();
        if (settings == null || settings.Frames == null || settings.Frames.Length == 0 || settings.FramesPerSecond <= 0f)
        {
            FinishCleanup();
            return;
        }

        StartCoroutine(PlayCleanupAnimation(settings));
    }

    private CrabTrashCleanupSettings ResolveSettings()
    {
        if (cleanupSettings == null)
            cleanupSettings = Resources.Load<CrabTrashCleanupSettings>(DefaultSettingsResourcePath);

        return cleanupSettings;
    }

    private void DisableInteraction()
    {
        Interactable interactable = GetComponent<Interactable>();
        if (interactable != null)
            interactable.SetInteractionEnabled(false);

        Collider2D[] colliders = GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    private IEnumerator PlayCleanupAnimation(CrabTrashCleanupSettings settings)
    {
        SpriteRenderer sourceRenderer = ResolveBaseRenderer();

        GameObject effectObject = new GameObject("CleanupEffect");
        effectObject.transform.SetParent(transform, false);
        effectObject.transform.localPosition = settings.EffectOffset;
        effectObject.transform.localRotation = Quaternion.identity;
        effectObject.transform.localScale = settings.EffectScale;

        SpriteRenderer effectRenderer = effectObject.AddComponent<SpriteRenderer>();
        ApplyEffectSorting(effectRenderer, sourceRenderer, settings.SortingOrderOffset);

        float frameDuration = 1f / settings.FramesPerSecond;
        for (int i = 0; i < settings.Frames.Length; i++)
        {
            effectRenderer.sprite = settings.Frames[i];
            yield return new WaitForSeconds(frameDuration);
        }

        effectRenderer.enabled = false;
        Destroy(effectObject);
        yield return null;
        FinishCleanup();
    }

    private void ApplyEffectSorting(SpriteRenderer effectRenderer, SpriteRenderer sourceRenderer, int sortingOrderOffset)
    {
        if (sourceRenderer == null)
            return;

        effectRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        effectRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
    }

    private SpriteRenderer ResolveBaseRenderer()
    {
        Transform body = transform.Find("Body");
        if (body != null && body.TryGetComponent(out SpriteRenderer bodyRenderer))
            return bodyRenderer;

        return GetComponentInChildren<SpriteRenderer>();
    }

    private void FinishCleanup()
    {
        UnlockPlayerAfterCleanup();

        if (CrabQuestManager.Instance != null)
            CrabQuestManager.Instance.ClearOneTrash();

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        UnlockPlayerAfterCleanup();
    }

    private void LockPlayerForCleanup()
    {
        if (lockedPlayerInput)
            return;

        lockedPlayerController = FindFirstObjectByType<SeahorseController>();
        if (lockedPlayerController == null)
            return;

        lockedPlayerDash = lockedPlayerController.GetComponent<SeahorseDash>();
        lockedPlayerRb = lockedPlayerController.GetComponent<Rigidbody2D>();

        SeahorseController.SetGameplayInputLocked(true);
        lockedPlayerInput = true;

        if (lockedPlayerDash != null)
            lockedPlayerDash.ForceCancelDash();

        if (lockedPlayerRb != null)
            lockedPlayerRb.linearVelocity = Vector2.zero;
    }

    private void UnlockPlayerAfterCleanup()
    {
        if (!lockedPlayerInput)
            return;

        SeahorseController.SetGameplayInputLocked(false);
        lockedPlayerInput = false;
        lockedPlayerController = null;
        lockedPlayerDash = null;
        lockedPlayerRb = null;
    }
}
