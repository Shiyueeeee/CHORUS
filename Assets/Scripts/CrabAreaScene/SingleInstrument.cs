using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SingleInstrument : MonoBehaviour, IInteractable
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer uiImageRenderer;
    [SerializeField] private Sprite uiImage;
    [SerializeField] private SpriteRenderer ePromptRenderer;
    [SerializeField] private Sprite ePromptSprite;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxClip;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    private void Reset()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (uiImageRenderer == null)
            uiImageRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }

        ApplySprites();
    }

    public void Interact()
    {
        if (audioSource == null || sfxClip == null)
            return;

        AudioSettingsManager.ApplySfxVolume(audioSource, sfxVolume);
        audioSource.PlayOneShot(sfxClip, 1f);
    }

    private void ApplySprites()
    {
        if (uiImageRenderer != null && uiImage != null)
            uiImageRenderer.sprite = uiImage;

        if (ePromptRenderer != null && ePromptSprite != null)
            ePromptRenderer.sprite = ePromptSprite;
    }
}
