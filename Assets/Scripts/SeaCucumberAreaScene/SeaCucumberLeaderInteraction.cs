using System.Collections.Generic;
using UnityEngine;

public class SeaCucumberLeaderInteraction : NpcDialogueInteractionBase
{
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "Sea Cucumber";
    [SerializeField] private DialogueLineData[] acceptQuestDialogueLines;
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };

    private SeaCucumberDialoguePhase activePhase = SeaCucumberDialoguePhase.Repeat;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.seaCucumberQuestAccepted)
        {
            activePhase = SeaCucumberDialoguePhase.AcceptQuest;
            return BuildDialogueList(acceptQuestDialogueLines, speakerName);
        }

        activePhase = SeaCucumberDialoguePhase.Repeat;
        return BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase != SeaCucumberDialoguePhase.AcceptQuest || GameProgress.seaCucumberQuestAccepted)
            return;

        GameProgress.seaCucumberQuestAccepted = true;
        Debug.Log("Sea cucumber quest accepted. Placeholder only.");
    }

    private enum SeaCucumberDialoguePhase
    {
        AcceptQuest,
        Repeat
    }
}
