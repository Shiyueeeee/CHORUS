using UnityEngine;
using UnityEngine.UI;

public class EndingSceneFadeIn : MonoBehaviour
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private float duration = 1.35f;

    private float elapsed;

    private void Awake()
    {
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        SetAlpha(1f);
    }

    private void OnEnable()
    {
        elapsed = 0f;
        SetAlpha(1f);
    }

    private void Update()
    {
        if (targetGraphic == null)
        {
            enabled = false;
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
        float alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
        SetAlpha(alpha);

        if (t >= 1f)
            Destroy(gameObject);
    }

    public void Configure(Graphic graphicTarget, float fadeDuration)
    {
        targetGraphic = graphicTarget;
        duration = fadeDuration;
        elapsed = 0f;
        SetAlpha(1f);
    }

    private void SetAlpha(float alpha)
    {
        if (targetGraphic == null)
            return;

        Color color = targetGraphic.color;
        color.a = alpha;
        targetGraphic.color = color;
    }
}
