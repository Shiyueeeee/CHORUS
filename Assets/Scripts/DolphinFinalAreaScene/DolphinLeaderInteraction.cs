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
    [TextArea(2, 5)]
    [SerializeField] private string[] initDialogueLines;

    [Header("Repeat Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] repeatDialogueLines =
    {
        "......"
    };

    [Header("Finished Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] finishedDialogueLines;

    
    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<string> GetDialogueLinesForCurrentState()
    {
        switch (activePhase)
        {
            case DialoguePhase.Init:
                return BuildDialogueList(initDialogueLines);
            case DialoguePhase.Repeat:
                return BuildDialogueList(repeatDialogueLines);
            case DialoguePhase.Finish:
                return BuildDialogueList(finishedDialogueLines);
            default:
                return BuildDialogueList(null);
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
        GameProgress.crabCompleted = true;

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
