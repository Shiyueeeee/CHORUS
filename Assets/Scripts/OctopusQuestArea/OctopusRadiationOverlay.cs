using UnityEngine;
using UnityEngine.UI;

public class OctopusRadiationOverlay : MonoBehaviour
{
    [Header("Overlay")]
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private Image overlayImage;
    [SerializeField] private float minAlpha = 0.08f;
    [SerializeField] private float maxAlpha = 0.2f;
    [SerializeField] private float pulseSpeed = 2.5f;

    private int activeZoneCount;
    private OctopusBubbleShield bubbleShield;

    private void OnEnable()
    {
        activeZoneCount = 0;
        bubbleShield = GetComponent<OctopusBubbleShield>();
        ApplyVisibility(false);
    }

    private void Update()
    {
        bool shouldShow = activeZoneCount > 0 && (bubbleShield == null || !bubbleShield.IsShieldActive);
        if (!shouldShow)
        {
            ApplyVisibility(false);
            return;
        }

        ApplyVisibility(true);

        if (overlayImage == null)
            return;

        Color color = overlayImage.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        overlayImage.color = color;
    }

    public void EnterRadiationZone()
    {
        activeZoneCount++;
    }

    public void ExitRadiationZone()
    {
        activeZoneCount = Mathf.Max(0, activeZoneCount - 1);
        if (activeZoneCount <= 0)
            ApplyVisibility(false);
    }

    private void ApplyVisibility(bool isVisible)
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(isVisible);

        if (!isVisible || overlayImage == null)
            return;

        Color color = overlayImage.color;
        color.a = minAlpha;
        overlayImage.color = color;
    }
}
