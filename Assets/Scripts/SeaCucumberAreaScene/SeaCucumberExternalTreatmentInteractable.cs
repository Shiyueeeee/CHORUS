using UnityEngine;

public class SeaCucumberExternalTreatmentInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SeaCucumberExternalTreatmentManager treatmentManager;

    public void Interact()
    {
        if (DialogueManager.IsDialogueActive || treatmentManager == null)
            return;

        treatmentManager.TryOpenPanel();
    }
}
