using System.Collections.Generic;
using UnityEngine;

public class OctopusCorpseInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Octopus";
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        return BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
    }
}
