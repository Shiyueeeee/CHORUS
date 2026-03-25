using UnityEngine;
using UnityEngine.UI;

public class EndingSceneCreditsRoll : MonoBehaviour
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private float scrollSpeed = 46f;
    [SerializeField] private float topPadding = 72f;
    [SerializeField] private float bottomPadding = 52f;
    [SerializeField] private bool loop;

    private bool isInitialized;
    private bool hasCompleted;
    private float startY;
    private float endY;

    private void OnEnable()
    {
        hasCompleted = false;
        isInitialized = false;
        TryInitialize();
        ResetPosition();
    }

    private void Update()
    {
        if (hasCompleted)
            return;

        if (!TryInitialize() || content == null)
            return;

        Vector2 position = content.anchoredPosition;
        position.y += scrollSpeed * Time.unscaledDeltaTime;

        if (position.y >= endY)
        {
            if (loop)
            {
                position.y = startY;
            }
            else
            {
                position.y = endY;
                hasCompleted = true;
            }
        }

        content.anchoredPosition = position;
    }

    public void Configure(RectTransform viewportRect, RectTransform contentRect, float speed, bool shouldLoop)
    {
        viewport = viewportRect;
        content = contentRect;
        scrollSpeed = speed;
        loop = shouldLoop;
        hasCompleted = false;
        isInitialized = false;
        TryInitialize();
        ResetPosition();
    }

    private bool TryInitialize()
    {
        if (viewport == null || content == null)
            return false;

        Canvas.ForceUpdateCanvases();

        float viewportHeight = viewport.rect.height;
        float contentHeight = Mathf.Max(content.rect.height, LayoutUtility.GetPreferredHeight(content));
        if (contentHeight <= 0f)
            contentHeight = content.sizeDelta.y;

        startY = -(viewportHeight * 0.5f + contentHeight * 0.5f + bottomPadding);
        endY = viewportHeight * 0.5f + contentHeight * 0.5f + topPadding;
        isInitialized = true;
        return true;
    }

    private void ResetPosition()
    {
        if (!isInitialized || content == null)
            return;

        Vector2 position = content.anchoredPosition;
        position.y = startY;
        content.anchoredPosition = position;
    }
}
