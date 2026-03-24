using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DolphinLeaderInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Dolphin Leader";
    [SerializeField] private string returnSceneName = "RQScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_DolphinReturn";

    private DialoguePhase activePhase = DialoguePhase.Repeat;

    [Header("Repeat Dialogue")]
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    [Header("Finished Dialogue")]
    [SerializeField] private DialogueLineData[] finishedDialogueLines;

    
    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.dolphinCompleted)
        {
            activePhase = DialoguePhase.Finish;
            return BuildDialogueList(finishedDialogueLines, speakerName);
        }

        activePhase = DialoguePhase.Repeat;
        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase == DialoguePhase.Finish && !GameProgress.dolphinCompleted)
            CompleteQuestAndReturn();
    }
    
    private void CompleteQuestAndReturn()
    {
        GameProgress.dolphinCompleted = true;

        SceneSpawnData.returnSpawnPointName = returnSpawnPointName;
        GameSaveSystem.SaveCheckpoint(returnSceneName, returnSpawnPointName);
        SceneTransition.LoadScene(returnSceneName);
    }

    private enum DialoguePhase
    {
        Repeat,
        Finish
    }
}
