using System.Collections.Generic;
using UnityEngine;

public class OctopusSharkInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Shark";
    [SerializeField] private DialogueLineData[] waitingDialogueLines;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        return BuildDialogueList(waitingDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
    }
}
