using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DolphinLeaderInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Dolphin Leader";
    [SerializeField] private string returnSceneName = "RQScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_DolphinReturn";
    
    private DialoguePhase activePhase = DialoguePhase.Init;
    
    [Header("Init Dialogue")]
    [SerializeField] private DialogueLineData[] initDialogueLines;

    [Header("Repeat Dialogue")]
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };

    [Header("Finished Dialogue")]
    [SerializeField] private DialogueLineData[] finishedDialogueLines;

    
    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        switch (activePhase)
        {
            case DialoguePhase.Init:
                return BuildDialogueList(initDialogueLines, speakerName);
            case DialoguePhase.Repeat:
                return BuildDialogueList(repeatDialogueLines, speakerName);
            case DialoguePhase.Finish:
                return BuildDialogueList(finishedDialogueLines, speakerName);
            default:
                return null;
        }
    }

    protected override void OnDialogueFinished()
    {
        switch (activePhase)
        {
            case DialoguePhase.Init:
                activePhase = DialoguePhase.Finish;
                break;
            case DialoguePhase.Finish:
                activePhase = DialoguePhase.Repeat;
                CompleteQuestAndReturn();
                break;
            default:
                break;
        }
    }
    
    private void CompleteQuestAndReturn()
    {
        GameProgress.dolphinCompleted = true;

        SceneSpawnData.returnSpawnPointName = returnSpawnPointName;
        SceneManager.LoadScene(returnSceneName);
    }

    private enum DialoguePhase
    {
        Init, 
        Repeat,
        Finish
    }
}
