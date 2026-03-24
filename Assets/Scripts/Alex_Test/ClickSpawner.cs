using UnityEngine;
using UnityEngine.InputSystem;

public class ClickSpawner : MonoBehaviour
{
    public GameObject ripplePrefab;
    
    private void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();

            Vector3 mouseWorldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(mouseScreenPosition);

            mouseWorldPosition.z = 0f;

            Instantiate(ripplePrefab, mouseWorldPosition, Quaternion.identity);
        }
    }
}