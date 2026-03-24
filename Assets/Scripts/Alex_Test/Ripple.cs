using UnityEngine;
using UnityEngine.UI;

public class Ripple : MonoBehaviour
{
    [Header("Ripple Settings")]
    public float expandSpeed = 3f;
    public float fadeSpeed = 2f;

    private RectTransform rectTransform;
    private Image image;
    private Color rippleColor;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        rippleColor = image.color;
    }

    void Update()
    {
        rectTransform.localScale += Vector3.one * expandSpeed * Time.unscaledDeltaTime;

        rippleColor.a -= fadeSpeed * Time.unscaledDeltaTime;
        image.color = rippleColor;

        if (rippleColor.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}