using UnityEngine;
using UnityEngine.UI;

public class UILightPulse : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private CanvasGroup targetCanvasGroup;
    [SerializeField] private Graphic targetGraphic;

    [Header("Alpha")]
    [SerializeField] [Range(0f, 1f)] private float minAlpha = 0.2f;
    [SerializeField] [Range(0f, 1f)] private float maxAlpha = 0.65f;
    [SerializeField] private float pulseFrequency = 0.45f;
    [SerializeField] private float secondaryWaveStrength = 0.15f;

    [Header("Scale")]
    [SerializeField] private bool animateScale = true;
    [SerializeField] private float scalePulseAmount = 0.06f;
    [SerializeField] private float scaleFrequency = 0.35f;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        if (targetRect == null)
        {
            targetRect = transform as RectTransform;
        }

        if (targetCanvasGroup == null)
        {
            targetCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }

        if (targetRect != null)
        {
            baseScale = targetRect.localScale;
        }
    }

    private void OnEnable()
    {
        if (targetRect != null)
        {
            baseScale = targetRect.localScale;
        }
    }

    private void Update()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;

        float mainWave = Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f);
        float secondaryWave = Mathf.Sin((time * pulseFrequency * 1.9f * Mathf.PI * 2f) + 0.7f) * secondaryWaveStrength;
        float pulse01 = Mathf.Clamp01(((mainWave + secondaryWave) + 1f) * 0.5f);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse01);

        ApplyAlpha(alpha);
        ApplyScale(time, pulse01);
    }

    private void ApplyAlpha(float alpha)
    {
        if (targetCanvasGroup != null)
        {
            targetCanvasGroup.alpha = alpha;
            return;
        }

        if (targetGraphic != null)
        {
            Color color = targetGraphic.color;
            color.a = alpha;
            targetGraphic.color = color;
        }
    }

    private void ApplyScale(float time, float pulse01)
    {
        if (!animateScale || targetRect == null)
        {
            return;
        }

        float scaleWave = Mathf.Sin((time * scaleFrequency * Mathf.PI * 2f) + 0.35f) * 0.5f + 0.5f;
        float scaleOffset = Mathf.Lerp(-scalePulseAmount * 0.35f, scalePulseAmount, (scaleWave + pulse01) * 0.5f);
        targetRect.localScale = baseScale * (1f + scaleOffset);
    }
}
