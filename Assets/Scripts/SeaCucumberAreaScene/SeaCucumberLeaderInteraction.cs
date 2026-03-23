using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeaCucumberLeaderInteraction : NpcDialogueInteractionBase
{
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "Sea Cucumber";
    [SerializeField] private DialogueLineData[] acceptQuestDialogueLines;
    [SerializeField] private DialogueLineData[] turnInQuestDialogueLines;
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;
    [SerializeField] private string returnSceneName = "RQScene";

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

        if (GameProgress.seaCucumberTriageCompleted &&
            GameProgress.seaCucumberInternalTreatmentCompleted &&
            GameProgress.seaCucumberExternalTreatmentCompleted &&
            !GameProgress.seaCucumberCompleted)
        {
            activePhase = SeaCucumberDialoguePhase.TurnInQuest;
            return BuildDialogueList(turnInQuestDialogueLines, speakerName);
        }

        activePhase = SeaCucumberDialoguePhase.Repeat;
        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase == SeaCucumberDialoguePhase.AcceptQuest && !GameProgress.seaCucumberQuestAccepted)
        {
            GameProgress.seaCucumberQuestAccepted = true;
            Debug.Log("Sea cucumber quest accepted. Placeholder only.");
            return;
        }

        if (activePhase != SeaCucumberDialoguePhase.TurnInQuest || GameProgress.seaCucumberCompleted)
            return;

        GameProgress.seaCucumberCompleted = true;
        SceneTransition.LoadScene(returnSceneName);
    }

    private enum SeaCucumberDialoguePhase
    {
        AcceptQuest,
        TurnInQuest,
        Repeat
    }
}
