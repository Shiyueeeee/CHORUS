using UnityEngine;

public class Ripple : MonoBehaviour
{
    [Header("Ripple Settings")]
    public float expandSpeed = 3f; // How fast the circle grows
    public float fadeSpeed = 2f;   // How fast the circle becomes invisible

    private SpriteRenderer spriteRenderer;
    private Color rippleColor;

    void Start()
    {
        // Grab the SpriteRenderer and its starting color
        spriteRenderer = GetComponent<SpriteRenderer>();
        rippleColor = spriteRenderer.color;
    }

    void Update()
    {
        // 1. Expand the size (Scale) over time
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // 2. Fade the transparency (Alpha) over time
        rippleColor.a -= fadeSpeed * Time.deltaTime;
        spriteRenderer.color = rippleColor;

        // 3. Destroy the object once it is completely invisible to save memory
        if (rippleColor.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}