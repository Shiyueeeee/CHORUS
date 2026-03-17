using UnityEngine;

public class SeaCucumberTriageInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SeaCucumberTriageManager triageManager;

    public void Interact()
    {
        if (DialogueManager.IsDialogueActive || triageManager == null)
            return;

        triageManager.TryOpenPuzzle();
    }
}
