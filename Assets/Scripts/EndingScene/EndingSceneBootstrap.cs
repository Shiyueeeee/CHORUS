using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class EndingSceneBootstrap : MonoBehaviour
{
    [System.Serializable]
    public class CreditSection
    {
        public string heading = string.Empty;
        public string[] names = new string[0];
    }

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.35f;
    [SerializeField] private float secondsPerSlide = 3f;
    [SerializeField] private float slideFadeDuration = 0.35f;
    [SerializeField] private float creditsScrollSpeed = 46f;

    [Header("Slideshow")]
    [SerializeField] private Sprite[] slideshowSprites = new Sprite[0];

    [Header("Credits")]
    [SerializeField] private string creditsTitle = "Credits";
    [SerializeField] private bool loopCredits = true;
    [SerializeField] private float creditsTitleFontSize = 40f;
    [SerializeField] private float creditsRoleFontSize = 22f;
    [SerializeField] private float creditsNameFontSize = 34f;
    [SerializeField] private string creditsClosingHeading = "CHORUS";
    [SerializeField] private float creditsClosingFontSize = 28f;
    [SerializeField, TextArea(2, 4)] private string creditsClosingText = "Thank you for playing";
    [SerializeField] private CreditSection[] creditSections = new CreditSection[]
    {
        new CreditSection { heading = "DESIGN / ART", names = new[] { "Olivia Wang" } },
        new CreditSection { heading = "DESIGN", names = new[] { "Alex" } },
        new CreditSection { heading = "PROGRAMMING", names = new[] { "Lucia", "Richard" } }
    };

    [Header("Back Button")]
    [SerializeField] private string backButtonText = "Back to Title";
    [SerializeField] private string backButtonTextChinese = "返回标题";
    [SerializeField] private TMP_FontAsset backButtonFontAsset;
    [SerializeField] private Sprite backButtonBackgroundSprite;
    [SerializeField] private float backButtonFontSize = 28f;
    [SerializeField] private Vector2 backButtonSize = new Vector2(260f, 72f);

    private readonly List<Object> runtimeAssets = new List<Object>();

    private TMP_FontAsset fontAsset;
    private Sprite whiteSprite;

    private void Awake()
    {
        fontAsset = ResolveFont();
        whiteSprite = CreateSolidSprite("EndingScene_WhiteSprite", Color.white);

        EnsureCamera();
        BuildScene();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < runtimeAssets.Count; i++)
        {
            Object asset = runtimeAssets[i];
            if (asset != null)
                Destroy(asset);
        }

        runtimeAssets.Clear();
    }

    private void EnsureCamera()
    {
        if (UnityEngine.Camera.main != null)
            return;

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        UnityEngine.Camera cameraComponent = cameraObject.AddComponent<UnityEngine.Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(0.02f, 0.09f, 0.13f, 1f);
        cameraComponent.nearClipPlane = -10f;
        cameraComponent.farClipPlane = 10f;

        cameraObject.AddComponent<AudioListener>();
    }

    private void BuildScene()
    {
        Canvas canvas = CreateCanvas();

        Image background = CreateImage(canvas.transform, "Background", new Color(0.03f, 0.12f, 0.17f, 1f));
        Stretch(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        CreateAccentBubble(canvas.transform, "AccentBubble_TopLeft", new Vector2(0.12f, 0.84f), new Vector2(280f, 280f), new Color(0.26f, 0.72f, 0.95f, 0.12f));
        CreateAccentBubble(canvas.transform, "AccentBubble_BottomLeft", new Vector2(0.22f, 0.14f), new Vector2(180f, 180f), new Color(0.78f, 0.98f, 0.94f, 0.12f));
        CreateAccentBubble(canvas.transform, "AccentBubble_Right", new Vector2(0.87f, 0.76f), new Vector2(340f, 340f), new Color(0.16f, 0.55f, 0.76f, 0.1f));

        RectTransform leftPanel = CreateContainer(canvas.transform, "SlideshowPanel", new Vector2(0.05f, 0.11f), new Vector2(0.49f, 0.89f));
        RectTransform rightPanel = CreatePanel(canvas.transform, "CreditsPanel", new Vector2(0.56f, 0.08f), new Vector2(0.94f, 0.9f), new Color(0.03f, 0.15f, 0.2f, 0.74f));

        BuildSlideshowPanel(leftPanel);
        BuildCreditsPanel(rightPanel);
        Button backButton = BuildBackButton(canvas.transform);
        EnsureEventSystem(backButton.gameObject);

        // TMP_Text footer = CreateText(canvas.transform, "FooterText", "Thank you for playing", 30f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.88f, 0.98f, 0.95f, 0.9f));
        // Stretch(footer.rectTransform, new Vector2(0.28f, 0.02f), new Vector2(0.72f, 0.09f), Vector2.zero, Vector2.zero);

        Image fadeOverlay = CreateImage(canvas.transform, "FadeOverlay", Color.black);
        Stretch(fadeOverlay.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        EndingSceneFadeIn fadeIn = fadeOverlay.gameObject.AddComponent<EndingSceneFadeIn>();
        fadeIn.Configure(fadeOverlay, fadeDuration);
    }

    private Button BuildBackButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("BackToTitleButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.96f, 0.03f);
        buttonRect.anchorMax = new Vector2(0.96f, 0.03f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = backButtonSize;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = backButtonBackgroundSprite != null ? backButtonBackgroundSprite : whiteSprite;
        buttonImage.type = Image.Type.Simple;
        buttonImage.color = new Color(0.17f, 0.59f, 0.87f, 0.96f);
        buttonImage.raycastTarget = true;
        AddOutline(buttonImage, new Color(0.9f, 0.98f, 1f, 0.2f), new Vector2(2f, -2f));

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.17f, 0.59f, 0.87f, 0.96f);
        colors.highlightedColor = new Color(0.24f, 0.68f, 0.95f, 1f);
        colors.pressedColor = new Color(0.12f, 0.46f, 0.72f, 1f);
        colors.selectedColor = new Color(0.24f, 0.68f, 0.95f, 1f);
        colors.disabledColor = new Color(0.17f, 0.59f, 0.87f, 0.45f);
        button.colors = colors;
        button.onClick.AddListener(ReturnToTitleScene);

        TMP_Text label = CreateText(
            buttonObject.transform,
            "BackToTitleLabel",
            LanguageSettingsManager.GetLocalizedText(backButtonText, backButtonTextChinese),
            backButtonFontSize,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.96f, 0.99f, 1f, 1f));
        if (backButtonFontAsset != null)
            label.font = backButtonFontAsset;
        else
        {
            TMP_FontAsset chineseFont = LanguageSettingsManager.GetChineseUiFontOrNull();
            if (LanguageSettingsManager.CurrentLanguage == SupportedLanguage.Chinese && chineseFont != null)
                label.font = chineseFont;
        }
        Stretch(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 10f), new Vector2(-20f, -10f));

        return button;
    }

    private void BuildSlideshowPanel(RectTransform panel)
    {
        Image frame = CreateImage(panel, "SlideshowFrame", new Color(0.02f, 0.12f, 0.16f, 0.96f));
        Stretch(frame.rectTransform, new Vector2(0.02f, 0.03f), new Vector2(0.98f, 0.97f), Vector2.zero, Vector2.zero);
        AddOutline(frame, new Color(0.78f, 0.95f, 0.96f, 0.25f), new Vector2(2f, -2f));
        frame.gameObject.AddComponent<RectMask2D>();

        Image slideImage = CreateImage(frame.transform, "SlideshowImage", Color.white);
        Stretch(slideImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f));

        EndingSceneSlideshow slideshow = panel.gameObject.AddComponent<EndingSceneSlideshow>();
        slideshow.Configure(slideImage, null, null, secondsPerSlide, slideFadeDuration);
        slideshow.SetSlides(CreateSlides());
    }

    private void BuildCreditsPanel(RectTransform panel)
    {
        TMP_Text header = CreateText(panel, "CreditsHeader", creditsTitle, creditsTitleFontSize, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.95f, 1f, 0.98f, 1f));
        Stretch(header.rectTransform, new Vector2(0.08f, 0.89f), new Vector2(0.92f, 0.97f), Vector2.zero, Vector2.zero);

        Image divider = CreateImage(panel, "CreditsDivider", new Color(0.72f, 0.94f, 0.96f, 0.24f));
        Stretch(divider.rectTransform, new Vector2(0.18f, 0.85f), new Vector2(0.82f, 0.855f), Vector2.zero, Vector2.zero);

        Image viewport = CreateImage(panel, "CreditsViewport", new Color(0.02f, 0.1f, 0.14f, 0.82f));
        Stretch(viewport.rectTransform, new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.81f), Vector2.zero, Vector2.zero);
        AddOutline(viewport, new Color(0.75f, 0.96f, 0.97f, 0.2f), new Vector2(2f, -2f));
        viewport.gameObject.AddComponent<RectMask2D>();

        TMP_Text creditsText = CreateText(viewport.transform, "CreditsContent", BuildCreditsText(), 30f, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.92f, 0.98f, 0.98f, 1f));
        creditsText.textWrappingMode = TextWrappingModes.NoWrap;
        creditsText.richText = true;

        RectTransform creditsRect = creditsText.rectTransform;
        creditsRect.anchorMin = new Vector2(0.5f, 0.5f);
        creditsRect.anchorMax = new Vector2(0.5f, 0.5f);
        creditsRect.pivot = new Vector2(0.5f, 0.5f);
        creditsRect.anchoredPosition = Vector2.zero;
        creditsRect.sizeDelta = new Vector2(500f, 900f);

        Canvas.ForceUpdateCanvases();
        creditsText.ForceMeshUpdate();
        creditsRect.sizeDelta = new Vector2(500f, creditsText.preferredHeight + 60f);

        EndingSceneCreditsRoll creditsRoll = viewport.gameObject.AddComponent<EndingSceneCreditsRoll>();
        creditsRoll.Configure(viewport.rectTransform, creditsRect, creditsScrollSpeed, loopCredits);
    }

    private EndingSceneSlideshow.SlideData[] CreateSlides()
    {
        if (slideshowSprites != null && slideshowSprites.Length > 0)
        {
            EndingSceneSlideshow.SlideData[] slides = new EndingSceneSlideshow.SlideData[slideshowSprites.Length];
            for (int i = 0; i < slideshowSprites.Length; i++)
            {
                slides[i] = new EndingSceneSlideshow.SlideData
                {
                    image = slideshowSprites[i],
                    title = string.Empty,
                    subtitle = string.Empty
                };
            }

            return slides;
        }

        Debug.LogWarning("EndingSceneBootstrap has no slideshow sprites assigned. Falling back to generated placeholders.");
        return CreateFallbackSlides();
    }

    private EndingSceneSlideshow.SlideData[] CreateFallbackSlides()
    {
        return new[]
        {
            new EndingSceneSlideshow.SlideData
            {
                image = CreatePlaceholderSprite("EndingSlide_01", new Color(0.07f, 0.28f, 0.38f, 1f), new Color(0.21f, 0.58f, 0.73f, 1f), 1.3f),
                title = string.Empty,
                subtitle = string.Empty
            },
            new EndingSceneSlideshow.SlideData
            {
                image = CreatePlaceholderSprite("EndingSlide_02", new Color(0.08f, 0.23f, 0.27f, 1f), new Color(0.49f, 0.74f, 0.62f, 1f), 2.7f),
                title = string.Empty,
                subtitle = string.Empty
            },
            new EndingSceneSlideshow.SlideData
            {
                image = CreatePlaceholderSprite("EndingSlide_03", new Color(0.09f, 0.16f, 0.3f, 1f), new Color(0.49f, 0.72f, 0.93f, 1f), 3.9f),
                title = string.Empty,
                subtitle = string.Empty
            },
            new EndingSceneSlideshow.SlideData
            {
                image = CreatePlaceholderSprite("EndingSlide_04", new Color(0.12f, 0.2f, 0.19f, 1f), new Color(0.81f, 0.94f, 0.73f, 1f), 5.1f),
                title = string.Empty,
                subtitle = string.Empty
            },
            new EndingSceneSlideshow.SlideData
            {
                image = CreatePlaceholderSprite("EndingSlide_05", new Color(0.06f, 0.18f, 0.25f, 1f), new Color(0.29f, 0.66f, 0.8f, 1f), 6.5f),
                title = string.Empty,
                subtitle = string.Empty
            }
        };
    }

    private string BuildCreditsText()
    {
        StringBuilder builder = new StringBuilder();

        if (creditSections != null)
        {
            for (int i = 0; i < creditSections.Length; i++)
            {
                CreditSection section = creditSections[i];
                if (section == null)
                    continue;

                bool hasHeading = !string.IsNullOrWhiteSpace(section.heading);
                bool hasNames = section.names != null && section.names.Length > 0;
                if (!hasHeading && !hasNames)
                    continue;

                if (hasHeading)
                    builder.Append("<size=").Append(creditsRoleFontSize).Append("><b>").Append(section.heading).Append("</b></size>\n");

                if (hasNames)
                    builder.Append("<size=").Append(creditsNameFontSize).Append(">").Append(string.Join("\n", section.names)).Append("</size>\n\n");
                else
                    builder.Append('\n');
            }
        }

        if (!string.IsNullOrWhiteSpace(creditsClosingHeading))
            builder.Append("<size=").Append(creditsRoleFontSize).Append("><b>").Append(creditsClosingHeading).Append("</b></size>\n");

        if (!string.IsNullOrWhiteSpace(creditsClosingText))
            builder.Append("<size=").Append(creditsClosingFontSize).Append(">").Append(creditsClosingText).Append("</size>");

        return builder.ToString().TrimEnd();
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("EndingCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void EnsureEventSystem(GameObject firstSelected)
    {
        EventSystem existingEventSystem = EventSystem.current;
        if (existingEventSystem != null)
        {
            existingEventSystem.firstSelectedGameObject = firstSelected;
            existingEventSystem.SetSelectedGameObject(firstSelected);
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
        eventSystem.firstSelectedGameObject = firstSelected;
        eventSystem.SetSelectedGameObject(firstSelected);
    }

    private RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        Image panel = CreateImage(parent, name, color);
        Stretch(panel.rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        AddOutline(panel, new Color(0.74f, 0.95f, 0.96f, 0.16f), new Vector2(2f, -2f));
        return panel.rectTransform;
    }

    private RectTransform CreateContainer(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject containerObject = new GameObject(name, typeof(RectTransform));
        containerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = containerObject.GetComponent<RectTransform>();
        Stretch(rectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        return rectTransform;
    }

    private void CreateAccentBubble(Transform parent, string name, Vector2 anchorCenter, Vector2 size, Color color)
    {
        Image bubble = CreateImage(parent, name, color);
        bubble.sprite = CreateSoftCircleSprite(name + "_Sprite");

        RectTransform bubbleRect = bubble.rectTransform;
        bubbleRect.anchorMin = anchorCenter;
        bubbleRect.anchorMax = anchorCenter;
        bubbleRect.pivot = new Vector2(0.5f, 0.5f);
        bubbleRect.anchoredPosition = Vector2.zero;
        bubbleRect.sizeDelta = size;

        bubble.gameObject.AddComponent<UILightPulse>();
    }

    private Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(Transform parent, string name, string value, float fontSize, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = fontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
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

    private void AddOutline(Graphic graphic, Color color, Vector2 distance)
    {
        Outline outline = graphic.gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private void ReturnToTitleScene()
    {
        SceneTransition.LoadScene("TitleScene");
    }

    private TMP_FontAsset ResolveFont()
    {
        TMP_FontAsset resolvedFont = TMP_Settings.defaultFontAsset;
        if (resolvedFont != null)
            return resolvedFont;

        resolvedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (resolvedFont != null)
            return resolvedFont;

        Debug.LogWarning("EndingSceneBootstrap could not resolve a TMP font asset.");
        return null;
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

    private Sprite CreateSoftCircleSprite(string assetName)
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = assetName + "_Texture";

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (distance / radius));
                alpha = alpha * alpha;
                pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = assetName;

        runtimeAssets.Add(texture);
        runtimeAssets.Add(sprite);
        return sprite;
    }

    private Sprite CreatePlaceholderSprite(string assetName, Color primaryColor, Color secondaryColor, float seedOffset)
    {
        const int size = 512;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = assetName + "_Texture";

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            float vertical01 = y / (size - 1f);

            for (int x = 0; x < size; x++)
            {
                float horizontal01 = x / (size - 1f);
                float diagonal = Mathf.Clamp01((horizontal01 * 0.55f) + (vertical01 * 0.9f));
                float wave = Mathf.Sin((horizontal01 * 7.5f) + seedOffset) * 0.04f;
                Color color = Color.Lerp(primaryColor, secondaryColor, Mathf.Clamp01(diagonal + wave));

                float band = Mathf.Abs(vertical01 - (0.28f + Mathf.Sin((horizontal01 * 5f) + seedOffset) * 0.05f));
                if (band < 0.035f)
                    color = Color.Lerp(color, Color.white, 0.12f);

                float bubbleX = 0.7f + Mathf.Sin(seedOffset) * 0.08f;
                float bubbleY = 0.68f + Mathf.Cos(seedOffset * 1.2f) * 0.06f;
                float bubbleRadius = 0.12f;
                float bubbleDistance = Vector2.Distance(new Vector2(horizontal01, vertical01), new Vector2(bubbleX, bubbleY));
                if (bubbleDistance < bubbleRadius)
                {
                    float bubbleStrength = 1f - (bubbleDistance / bubbleRadius);
                    color = Color.Lerp(color, Color.white, bubbleStrength * 0.16f);
                }

                pixels[(y * size) + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = assetName;

        runtimeAssets.Add(texture);
        runtimeAssets.Add(sprite);
        return sprite;
    }
}
