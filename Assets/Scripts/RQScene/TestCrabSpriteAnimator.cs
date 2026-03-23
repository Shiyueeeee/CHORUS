using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TestCrabSpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private bool loop = true;

    private float frameTimer;
    private int currentFrame;

    private void Awake()
    {
        EnsureRenderer();
        ApplyFrame(0);
    }

    private void OnEnable()
    {
        frameTimer = 0f;
        currentFrame = 0;
        ApplyFrame(currentFrame);
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || framesPerSecond <= 0f)
            return;

        float frameDuration = 1f / framesPerSecond;
        frameTimer += Time.deltaTime;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            AdvanceFrame();
        }
    }

    private void OnValidate()
    {
        EnsureRenderer();

        if (!Application.isPlaying)
            ApplyFrame(currentFrame);
    }

    private void EnsureRenderer()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void AdvanceFrame()
    {
        if (frames == null || frames.Length == 0)
            return;

        if (loop)
            currentFrame = (currentFrame + 1) % frames.Length;
        else
            currentFrame = Mathf.Min(currentFrame + 1, frames.Length - 1);

        ApplyFrame(currentFrame);
    }

    private void ApplyFrame(int frameIndex)
    {
        if (targetRenderer == null || frames == null || frames.Length == 0)
            return;

        currentFrame = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
        targetRenderer.sprite = frames[currentFrame];
    }
}
