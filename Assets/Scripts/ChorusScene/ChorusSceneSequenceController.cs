using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChorusSceneSequenceController : MonoBehaviour
{
    [Header("Slides")]
    [SerializeField] private Sprite[] slideSprites = new Sprite[0];
    [SerializeField] private Sprite titleSprite;

    [Header("Timing")]
    [SerializeField] private float secondsPerSlide = 1.5f;
    [SerializeField] private float fadeToBlackDuration = 0.45f;
    [SerializeField] private float lastSlideExtraHold = 2f;
    [SerializeField] private float finalFadeToBlackDuration = 1.2f;

    [Header("Scene Flow")]
    [SerializeField] private string nextSceneName = "EndingScene";
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Layout")]
    [SerializeField] private Color backgroundColor = Color.black;

    [Header("Title Animation")]
    [SerializeField] private Vector2 titleScreenPosition = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 titleMinSize = new Vector2(220f, 124f);
    [SerializeField] private Vector2 titleMaxSize = new Vector2(3200f, 1800f);
    [SerializeField] private float titleAnimationDuration = 5f;
    [SerializeField] private float titleOpacityAnimationDuration = 1.2f;
    [SerializeField] private float titleMaxSizeHoldDuration = 0f;
    [SerializeField] private float titleFadeDuration = 1.2f;
    [SerializeField] private AnimationCurve titleScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private readonly List<Object> runtimeAssets = new List<Object>();

    private Canvas rootCanvas;
    private Image slideImage;
    private Image titleImage;
    private Image fadeOverlay;
    private Sprite whiteSprite;
    private Coroutine playRoutine;

    private void Awake()
    {
        EnsureMainCamera();
        BuildRuntimeUI();
    }

    private void Start()
    {
        if (playRoutine == null)
            playRoutine = StartCoroutine(PlaySequence());
    }

    private void OnDestroy()
    {
        for (int i = 0; i < runtimeAssets.Count; i++)
        {
            if (runtimeAssets[i] != null)
                Destroy(runtimeAssets[i]);
        }

        runtimeAssets.Clear();
    }

    private void EnsureMainCamera()
    {
        if (UnityEngine.Camera.main != null)
            return;

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        UnityEngine.Camera cameraComponent = cameraObject.AddComponent<UnityEngine.Camera>();
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = backgroundColor;
        cameraComponent.nearClipPlane = -10f;
        cameraComponent.farClipPlane = 10f;

        cameraObject.AddComponent<AudioListener>();
    }

    private void BuildRuntimeUI()
    {
        whiteSprite = CreateSolidSprite("ChorusScene_WhiteSprite", Color.white);

        GameObject canvasObject = new GameObject("ChorusCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        rootCanvas = canvasObject.GetComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.pixelPerfect = false;
        rootCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage(rootCanvas.transform, "Background", backgroundColor);
        Stretch(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        slideImage = CreateImage(rootCanvas.transform, "SlideImage", Color.white);
        Stretch(slideImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        slideImage.preserveAspect = true;

        titleImage = CreateImage(rootCanvas.transform, "TitleImage", Color.white);
        ConfigureTitleRect(titleImage.rectTransform, titleScreenPosition, titleMinSize);
        titleImage.preserveAspect = true;
        SetImageAlpha(titleImage, 0f);

        fadeOverlay = CreateImage(rootCanvas.transform, "FadeOverlay", Color.clear);
        Stretch(fadeOverlay.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        titleImage.rectTransform.SetAsLastSibling();
    }

    private IEnumerator PlaySequence()
    {
        if (slideSprites == null || slideSprites.Length == 0)
        {
            Debug.LogWarning("ChorusSceneSequenceController has no slides assigned.");
            yield break;
        }

        ShowSlide(0);
        yield return Wait(secondsPerSlide);

        for (int i = 1; i < slideSprites.Length; i++)
        {
            yield return FadeOverlay(0f, 1f, fadeToBlackDuration);
            ShowSlide(i);
            yield return FadeOverlay(1f, 0f, fadeToBlackDuration);

            float holdDuration = secondsPerSlide;
            if (i == slideSprites.Length - 1)
                holdDuration += lastSlideExtraHold;

            yield return Wait(holdDuration);
        }

        if (slideSprites.Length == 1)
            yield return Wait(lastSlideExtraHold);

        float finalPhaseDuration = finalFadeToBlackDuration;
        if (titleSprite != null)
        {
            titleImage.sprite = titleSprite;
            titleImage.enabled = true;
            StartCoroutine(PlayTitleAnimation());
            finalPhaseDuration = Mathf.Max(finalPhaseDuration, Mathf.Max(titleAnimationDuration, titleOpacityAnimationDuration) + titleMaxSizeHoldDuration + titleFadeDuration);
        }

        StartCoroutine(FadeOverlay(0f, 1f, finalFadeToBlackDuration));
        yield return Wait(finalPhaseDuration);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneTransition.LoadScene(nextSceneName);
    }

    private void ShowSlide(int index)
    {
        if (slideImage == null || slideSprites == null || index < 0 || index >= slideSprites.Length)
            return;

        slideImage.sprite = slideSprites[index];
        slideImage.enabled = slideSprites[index] != null;
    }

    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        yield return FadeGraphic(fadeOverlay, from, to, duration);
    }

    private IEnumerator FadeGraphic(Graphic graphic, float from, float to, float duration)
    {
        if (graphic == null)
            yield break;

        float elapsed = 0f;
        SetGraphicAlpha(graphic, from);

        if (duration <= 0f)
        {
            SetGraphicAlpha(graphic, to);
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            SetGraphicAlpha(graphic, alpha);
            yield return null;
        }

        SetGraphicAlpha(graphic, to);
    }

    private IEnumerator Wait(float duration)
    {
        float remaining = Mathf.Max(0f, duration);
        while (remaining > 0f)
        {
            remaining -= GetDeltaTime();
            yield return null;
        }
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = whiteSprite;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    private void ConfigureTitleRect(RectTransform rectTransform, Vector2 normalizedPosition, Vector2 size)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = normalizedPosition;
        rectTransform.anchorMax = normalizedPosition;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    private IEnumerator PlayTitleAnimation()
    {
        if (titleImage == null)
            yield break;

        RectTransform titleRect = titleImage.rectTransform;
        ConfigureTitleRect(titleRect, titleScreenPosition, titleMinSize);
        SetImageAlpha(titleImage, 0f);

        float scaleDuration = Mathf.Max(0f, titleAnimationDuration);
        float opacityDuration = Mathf.Max(0f, titleOpacityAnimationDuration);
        float appearanceDuration = Mathf.Max(scaleDuration, opacityDuration);

        if (appearanceDuration <= 0f)
        {
            titleRect.sizeDelta = titleMaxSize;
            SetImageAlpha(titleImage, 1f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < appearanceDuration)
        {
            elapsed += GetDeltaTime();
            float scaleT = scaleDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / scaleDuration);
            float opacityT = opacityDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / opacityDuration);
            float curveT = titleScaleCurve != null ? titleScaleCurve.Evaluate(scaleT) : scaleT;

            titleRect.sizeDelta = Vector2.LerpUnclamped(titleMinSize, titleMaxSize, curveT);
            SetImageAlpha(titleImage, opacityT);
            yield return null;
        }

        titleRect.sizeDelta = titleMaxSize;
        SetImageAlpha(titleImage, 1f);
        if (titleMaxSizeHoldDuration > 0f)
            yield return Wait(titleMaxSizeHoldDuration);

        float fadeDuration = Mathf.Max(0f, titleFadeDuration);
        if (fadeDuration > 0f)
            yield return FadeGraphic(titleImage, 1f, 0f, fadeDuration);
        else
            SetImageAlpha(titleImage, 0f);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void SetGraphicAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null)
            return;

        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private Sprite CreateSolidSprite(string assetName, Color color)
    {
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.name = assetName + "_Texture";
        texture.SetPixels(new[] { color, color, color, color });
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = assetName;

        runtimeAssets.Add(texture);
        runtimeAssets.Add(sprite);
        return sprite;
    }
}
