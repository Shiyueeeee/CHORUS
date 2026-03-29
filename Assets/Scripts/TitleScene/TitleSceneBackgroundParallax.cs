using UnityEngine;
using UnityEngine.UI;

public class TitleSceneBackgroundParallax : MonoBehaviour
{
    [Header("Optional References")]
    [SerializeField] private RectTransform rootRect;
    [SerializeField] private Image sourceBackgroundImage;
    [SerializeField] private RectTransform lightLayer;

    [Header("Motion")]
    [SerializeField] private Vector2 backgroundTravel = new Vector2(36f, 24f);
    [SerializeField] private Vector2 lightTravel = new Vector2(72f, 48f);
    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Runtime Setup")]
    [SerializeField] private bool hideOriginalBackgroundGraphic = true;

    private RectTransform generatedBackgroundRect;
    private Image generatedBackgroundImage;
    private Color originalBackgroundColor = Color.white;
    private bool hasStoredOriginalColor;
    private Vector2 generatedBackgroundBasePosition;
    private Vector2 lightBasePosition;
    private bool initialized;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
        CaptureBasePositions();
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            InitializeIfNeeded();
            if (!initialized)
            {
                return;
            }
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        Vector2 normalizedMouseOffset = GetNormalizedMouseOffset();

        if (generatedBackgroundRect != null)
        {
            Vector2 target = generatedBackgroundBasePosition + Vector2.Scale(normalizedMouseOffset, backgroundTravel);
            generatedBackgroundRect.anchoredPosition = Vector2.Lerp(generatedBackgroundRect.anchoredPosition, target, 1f - Mathf.Exp(-smoothSpeed * deltaTime));
        }

        if (lightLayer != null)
        {
            Vector2 target = lightBasePosition + Vector2.Scale(normalizedMouseOffset, lightTravel);
            lightLayer.anchoredPosition = Vector2.Lerp(lightLayer.anchoredPosition, target, 1f - Mathf.Exp(-smoothSpeed * deltaTime));
        }
    }

    private void InitializeIfNeeded()
    {
        if (initialized)
        {
            return;
        }

        if (rootRect == null)
        {
            rootRect = transform as RectTransform;
        }

        if (rootRect == null)
        {
            enabled = false;
            return;
        }

        ResolveReferences();
        EnsureGeneratedBackground();
        CaptureBasePositions();
        initialized = generatedBackgroundRect != null || lightLayer != null;
    }

    private void ResolveReferences()
    {
        if (sourceBackgroundImage == null)
        {
            Transform backgroundTransform = transform.Find("MainTabPanel");
            if (backgroundTransform != null)
            {
                sourceBackgroundImage = backgroundTransform.GetComponent<Image>();
            }
        }

        if (lightLayer == null)
        {
            Transform lightTransform = transform.Find("Light");
            if (lightTransform != null)
            {
                lightLayer = lightTransform as RectTransform;
            }
        }
    }

    private void EnsureGeneratedBackground()
    {
        if (generatedBackgroundRect != null || sourceBackgroundImage == null)
        {
            return;
        }

        RectTransform sourceRect = sourceBackgroundImage.rectTransform;
        if (sourceRect == null)
        {
            return;
        }

        GameObject generatedObject = new GameObject("ParallaxBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        generatedBackgroundRect = generatedObject.GetComponent<RectTransform>();
        generatedBackgroundRect.SetParent(rootRect, false);
        generatedBackgroundRect.SetSiblingIndex(sourceRect.GetSiblingIndex());
        CopyRectTransform(sourceRect, generatedBackgroundRect);

        generatedBackgroundImage = generatedObject.GetComponent<Image>();
        CopyImage(sourceBackgroundImage, generatedBackgroundImage);
        generatedBackgroundImage.raycastTarget = false;

        if (hideOriginalBackgroundGraphic)
        {
            if (!hasStoredOriginalColor)
            {
                originalBackgroundColor = sourceBackgroundImage.color;
                hasStoredOriginalColor = true;
            }

            Color hiddenColor = originalBackgroundColor;
            hiddenColor.a = 0f;
            sourceBackgroundImage.color = hiddenColor;
        }
    }

    private void CaptureBasePositions()
    {
        if (generatedBackgroundRect != null)
        {
            generatedBackgroundBasePosition = generatedBackgroundRect.anchoredPosition;
        }

        if (lightLayer != null)
        {
            lightBasePosition = lightLayer.anchoredPosition;
        }
    }

    private Vector2 GetNormalizedMouseOffset()
    {
        Vector2 mousePosition = Input.mousePosition;
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return Vector2.zero;
        }

        float normalizedX = Mathf.Clamp01(mousePosition.x / Screen.width) - 0.5f;
        float normalizedY = Mathf.Clamp01(mousePosition.y / Screen.height) - 0.5f;
        return new Vector2(-normalizedX * 2f, -normalizedY * 2f);
    }

    private static void CopyRectTransform(RectTransform source, RectTransform target)
    {
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.pivot = source.pivot;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;
    }

    private static void CopyImage(Image source, Image target)
    {
        target.sprite = source.sprite;
        target.overrideSprite = source.overrideSprite;
        target.type = source.type;
        target.color = source.color;
        target.material = source.material;
        target.preserveAspect = source.preserveAspect;
        target.fillCenter = source.fillCenter;
        target.fillMethod = source.fillMethod;
        target.fillAmount = source.fillAmount;
        target.fillClockwise = source.fillClockwise;
        target.fillOrigin = source.fillOrigin;
        target.useSpriteMesh = source.useSpriteMesh;
        target.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        target.maskable = source.maskable;
    }
}
