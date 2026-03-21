using UnityEngine;

public class UITitleFloat : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform targetRect;

    [Header("Float Motion")]
    [SerializeField] private float verticalAmplitude = 18f;
    [SerializeField] private float horizontalAmplitude = 6f;
    [SerializeField] private float verticalFrequency = 0.8f;
    [SerializeField] private float horizontalFrequency = 0.45f;
    [SerializeField] private float phaseOffset = 0.15f;

    [Header("Tilt")]
    [SerializeField] private float rotationAmplitude = 2.5f;
    [SerializeField] private float rotationFrequency = 0.55f;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector2 baseAnchoredPosition;
    private Vector3 baseEulerAngles;

    private void Awake()
    {
        if (targetRect == null)
        {
            targetRect = transform as RectTransform;
        }

        if (targetRect == null)
        {
            enabled = false;
            return;
        }

        baseAnchoredPosition = targetRect.anchoredPosition;
        baseEulerAngles = targetRect.localEulerAngles;
    }

    private void OnEnable()
    {
        if (targetRect == null)
        {
            return;
        }

        baseAnchoredPosition = targetRect.anchoredPosition;
        baseEulerAngles = targetRect.localEulerAngles;
    }

    private void LateUpdate()
    {
        if (targetRect == null)
        {
            return;
        }

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;

        float verticalOffset = Mathf.Sin((time + phaseOffset) * verticalFrequency * Mathf.PI * 2f) * verticalAmplitude;
        float horizontalOffset = Mathf.Sin((time * horizontalFrequency * Mathf.PI * 2f) + 1.2f) * horizontalAmplitude;
        float tiltOffset = Mathf.Sin((time * rotationFrequency * Mathf.PI * 2f) + 0.6f) * rotationAmplitude;

        targetRect.anchoredPosition = baseAnchoredPosition + new Vector2(horizontalOffset, verticalOffset);
        targetRect.localEulerAngles = baseEulerAngles + new Vector3(0f, 0f, tiltOffset);
    }
}
