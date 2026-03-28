using UnityEngine;
using UnityEngine.SceneManagement;

public class WebGLCursorController : MonoBehaviour
{
    private const string SettingsResourcePath = "WebGLCursorSettings";

    private static bool initialized;

    private Texture2D sourceTexture;
    private Texture2D scaledCursorTexture;
    private Vector2 hotspot;
    private int webCursorSize = 64;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (initialized)
            return;

        WebGLCursorSettings settings = Resources.Load<WebGLCursorSettings>(SettingsResourcePath);
        if (settings == null || settings.cursorTexture == null)
            return;

        GameObject controllerObject = new GameObject("WebGLCursorController");
        DontDestroyOnLoad(controllerObject);

        WebGLCursorController controller = controllerObject.AddComponent<WebGLCursorController>();
        controller.Configure(settings);
        initialized = true;
#endif
    }

    private void Awake()
    {
        ApplyCursor();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (scaledCursorTexture != null)
            Destroy(scaledCursorTexture);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyCursor();
    }

    private void Configure(WebGLCursorSettings settings)
    {
        sourceTexture = settings.cursorTexture;
        hotspot = settings.hotspot;
        webCursorSize = Mathf.Max(16, settings.webCursorSize);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyCursor();
    }

    private void ApplyCursor()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (sourceTexture == null)
            return;

        EnsureScaledCursorTexture();
        if (scaledCursorTexture == null)
            return;

        Vector2 scaledHotspot = GetScaledHotspot();
        Cursor.SetCursor(scaledCursorTexture, scaledHotspot, CursorMode.Auto);
#endif
    }

    private void EnsureScaledCursorTexture()
    {
        if (sourceTexture == null)
            return;

        if (scaledCursorTexture != null &&
            scaledCursorTexture.width == webCursorSize &&
            scaledCursorTexture.height == webCursorSize)
        {
            return;
        }

        if (scaledCursorTexture != null)
            Destroy(scaledCursorTexture);

        scaledCursorTexture = new Texture2D(webCursorSize, webCursorSize, TextureFormat.RGBA32, false);
        scaledCursorTexture.name = sourceTexture.name + "_WebGLScaled";
        scaledCursorTexture.alphaIsTransparency = true;
        scaledCursorTexture.wrapMode = TextureWrapMode.Clamp;
        scaledCursorTexture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < webCursorSize; y++)
        {
            float v = webCursorSize <= 1 ? 0f : (y + 0.5f) / webCursorSize;
            for (int x = 0; x < webCursorSize; x++)
            {
                float u = webCursorSize <= 1 ? 0f : (x + 0.5f) / webCursorSize;
                scaledCursorTexture.SetPixel(x, y, sourceTexture.GetPixelBilinear(u, v));
            }
        }

        scaledCursorTexture.Apply(false, false);
    }

    private Vector2 GetScaledHotspot()
    {
        if (sourceTexture == null || scaledCursorTexture == null || sourceTexture.width <= 0 || sourceTexture.height <= 0)
            return hotspot;

        float x = hotspot.x * scaledCursorTexture.width / sourceTexture.width;
        float y = hotspot.y * scaledCursorTexture.height / sourceTexture.height;
        return new Vector2(x, y);
    }
}
