using UnityEngine;
using UnityEngine.InputSystem;

public class ClickSpawner : MonoBehaviour
{
    public GameObject ripplePrefab;
    [Tooltip("Assign the Canvas where ripples should be spawned")]
    public Canvas canvas;
    
    private void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, mouseScreenPosition, canvas.worldCamera, out Vector2 localPoint);

            GameObject ripple = Instantiate(ripplePrefab, canvas.transform);
            ripple.GetComponent<RectTransform>().anchoredPosition = localPoint;
            ripple.transform.SetAsLastSibling();
            ripple.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        }
    }
}