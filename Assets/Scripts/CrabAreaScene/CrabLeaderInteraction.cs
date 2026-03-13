using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrabLeaderInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Crab Leader";
    [SerializeField] private string returnSceneName = "RQScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_CrabReturn";

    [Header("Accept Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] acceptQuestDialogueLines;

    [Header("Quest In Progress Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] questInProgressDialogueLines;

    [Header("Turn In Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] turnInQuestDialogueLines;

    [Header("Invalid Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] invalidDialogueLines =
    {
        "Hi my lovely soldier!"
    };

    private DialoguePhase activePhase = DialoguePhase.Invalid;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<string> GetDialogueLinesForCurrentState()
    {
        if (CrabQuestManager.Instance == null)
        {
            Debug.LogWarning("CrabQuestManager not found.");
            return null;
        }

        if (GameProgress.crabCompleted)
        {
            activePhase = DialoguePhase.Invalid;
            return BuildDialogueList(invalidDialogueLines);
        }

        if (!CrabQuestManager.Instance.QuestAccepted)
        {
            activePhase = DialoguePhase.AcceptQuest;
            return BuildDialogueList(acceptQuestDialogueLines);
        }

        if (!CrabQuestManager.Instance.AllTrashCleared)
        {
            activePhase = DialoguePhase.InProgress;
            return BuildDialogueList(questInProgressDialogueLines);
        }

        activePhase = DialoguePhase.TurnIn;
        return BuildDialogueList(turnInQuestDialogueLines);
    }

    protected override void OnDialogueFinished()
    {
        switch (activePhase)
        {
            case DialoguePhase.AcceptQuest:
                if (CrabQuestManager.Instance != null)
                {
                    CrabQuestManager.Instance.AcceptQuest();
                }
                break;
            case DialoguePhase.TurnIn:
                CompleteQuestAndReturn();
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
        AcceptQuest,
        InProgress,
        TurnIn,
        Invalid
    }
}
