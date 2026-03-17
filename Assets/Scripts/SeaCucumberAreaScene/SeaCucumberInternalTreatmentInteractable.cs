using UnityEngine;

public class SeaCucumberInternalTreatmentInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SeaCucumberInternalTreatmentManager treatmentManager;

    public void Interact()
    {
        if (DialogueManager.IsDialogueActive || treatmentManager == null)
            return;

        treatmentManager.TryOpenPanel();
    }
}
