using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [SerializeField] private GameObject promptObject;

    private bool playerInRange = false;
    private bool interactionEnabled = true;
    private IInteractable interactAction;

    public bool InteractionEnabled => interactionEnabled;

    private void Start()
    {
        interactAction = GetComponent<IInteractable>();

        if (promptObject != null)
            promptObject.SetActive(false);
    }

    private void Update()
    {
        if (!interactionEnabled || !playerInRange) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (interactAction != null)
            {
                interactAction.Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!interactionEnabled) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HidePrompt();
        }
    }

    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;

        if (!interactionEnabled)
        {
            playerInRange = false;
            HidePrompt();
            return;
        }

        if (playerInRange)
        {
            ShowPrompt();
        }
    }

    private void ShowPrompt()
    {
        if (promptObject != null)
        {
            promptObject.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }
}
