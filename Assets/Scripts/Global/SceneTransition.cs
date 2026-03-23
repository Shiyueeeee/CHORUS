using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneTransition
{
    private const string SettingsResourcePath = "SceneTransitionSettings";
    private const float DefaultFadeDuration = 1.35f;
    private const int DefaultFadeCanvasSortOrder = short.MaxValue;
    private static readonly Color DefaultFadeColor = Color.black;

    private static bool initialized;
    private static bool pendingFadeIn;
    private static float pendingFadeDuration = DefaultFadeDuration;
    private static SceneTransitionSettings cachedSettings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (initialized)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        initialized = true;
    }

    public static void LoadScene(string sceneName)
    {
        LoadScene(sceneName, GetDefaultFadeDuration());
    }

    public static void LoadScene(string sceneName, float fadeDuration)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("SceneTransition.LoadScene called with an empty scene name.");
            return;
        }

        Initialize();

        pendingFadeIn = true;
        pendingFadeDuration = Mathf.Max(0f, fadeDuration);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!pendingFadeIn)
            return;

        pendingFadeIn = false;

        if (mode != LoadSceneMode.Single || UsesCustomSceneFade(scene))
            return;

        CreateFadeOverlay(pendingFadeDuration);
    }

    private static bool UsesCustomSceneFade(Scene scene)
    {
        SceneTransitionSettings settings = GetSettings();
        return settings != null ? settings.ShouldSkipFadeIn(scene.name) : scene.name == "EndingScene";
    }

    private static void CreateFadeOverlay(float fadeDuration)
    {
        GameObject fadeRoot = new GameObject("SceneFadeInOverlay", typeof(RectTransform), typeof(Canvas), typeof(RawImage));

        Canvas canvas = fadeRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = GetFadeCanvasSortOrder();

        RectTransform rectTransform = fadeRoot.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        RawImage overlay = fadeRoot.GetComponent<RawImage>();
        overlay.texture = Texture2D.whiteTexture;
        overlay.color = GetFadeColor();
        overlay.raycastTarget = false;

        SceneFadeInOverlay fadeIn = fadeRoot.AddComponent<SceneFadeInOverlay>();
        fadeIn.Configure(overlay, fadeDuration);
    }

    private static SceneTransitionSettings GetSettings()
    {
        if (cachedSettings == null)
            cachedSettings = Resources.Load<SceneTransitionSettings>(SettingsResourcePath);

        return cachedSettings;
    }

    private static float GetDefaultFadeDuration()
    {
        SceneTransitionSettings settings = GetSettings();
        return settings != null ? settings.DefaultFadeDuration : DefaultFadeDuration;
    }

    private static int GetFadeCanvasSortOrder()
    {
        SceneTransitionSettings settings = GetSettings();
        return settings != null ? settings.SortingOrder : DefaultFadeCanvasSortOrder;
    }

    private static Color GetFadeColor()
    {
        SceneTransitionSettings settings = GetSettings();
        return settings != null ? settings.FadeColor : DefaultFadeColor;
    }
}
