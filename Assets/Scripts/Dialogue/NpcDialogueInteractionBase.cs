using System.Collections.Generic;
using UnityEngine;

public abstract class NpcDialogueInteractionBase : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (DialogueManager.IsDialogueActive)
            return;

        List<DialogueLineData> linesToUse = GetDialogueLinesForCurrentState();
        if (linesToUse == null || linesToUse.Count == 0)
        {
            Debug.LogWarning(GetMissingDialogueWarning());
            return;
        }

        DialogueManager.Instance.StartDialogue(
            linesToUse,
            OnDialogueFinished
        );
    }

    protected List<DialogueLineData> BuildDialogueList(DialogueLineData[] sourceLines, string fallbackSpeaker)
    {
        if (sourceLines == null || sourceLines.Length == 0)
            return null;

        List<DialogueLineData> lines = new List<DialogueLineData>();
        foreach (DialogueLineData line in sourceLines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text))
                continue;

            lines.Add(new DialogueLineData
            {
                speaker = string.IsNullOrWhiteSpace(line.speaker) ? fallbackSpeaker : line.speaker,
                text = line.text
            });
        }

        return lines.Count > 0 ? lines : null;
    }

    protected List<DialogueLineData> BuildRandomDialogueList(DialogueBlockData[] sourceBlocks, string fallbackSpeaker)
    {
        if (sourceBlocks == null || sourceBlocks.Length == 0)
            return null;

        List<DialogueBlockData> validBlocks = new List<DialogueBlockData>();
        foreach (DialogueBlockData block in sourceBlocks)
        {
            if (block == null)
                continue;

            List<DialogueLineData> builtLines = BuildDialogueList(block.Lines, fallbackSpeaker);
            if (builtLines != null && builtLines.Count > 0)
                validBlocks.Add(block);
        }

        if (validBlocks.Count == 0)
            return null;

        DialogueBlockData selectedBlock = validBlocks[Random.Range(0, validBlocks.Count)];
        return BuildDialogueList(selectedBlock.Lines, fallbackSpeaker);
    }

    protected virtual string GetMissingDialogueWarning()
    {
        return GetType().Name + " has no dialogue configured for the current state.";
    }

    protected abstract string GetSpeakerName();
    protected abstract List<DialogueLineData> GetDialogueLinesForCurrentState();
    protected abstract void OnDialogueFinished();
}
