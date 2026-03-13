using System.Collections.Generic;
using UnityEngine;

public abstract class NpcDialogueInteractionBase : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (DialogueManager.IsDialogueActive)
            return;

        List<string> linesToUse = GetDialogueLinesForCurrentState();
        if (linesToUse == null || linesToUse.Count == 0)
        {
            Debug.LogWarning(GetMissingDialogueWarning());
            return;
        }

        DialogueManager.Instance.StartDialogue(
            GetSpeakerName(),
            linesToUse,
            OnDialogueFinished
        );
    }

    protected List<string> BuildDialogueList(string[] sourceLines)
    {
        if (sourceLines == null || sourceLines.Length == 0)
            return null;

        List<string> lines = new List<string>();
        foreach (string line in sourceLines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        return lines.Count > 0 ? lines : null;
    }

    protected virtual string GetMissingDialogueWarning()
    {
        return GetType().Name + " has no dialogue configured for the current state.";
    }

    protected abstract string GetSpeakerName();
    protected abstract List<string> GetDialogueLinesForCurrentState();
    protected abstract void OnDialogueFinished();
}
