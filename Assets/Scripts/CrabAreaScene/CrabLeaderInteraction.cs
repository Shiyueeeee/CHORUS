using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrabLeaderInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Crab Leader";
    [SerializeField] private string returnSceneName = "RQScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_CrabReturn";

    [Header("Accept Quest Dialogue")]
    [SerializeField] private DialogueLineData[] acceptQuestDialogueLines;

    [Header("Quest In Progress Dialogue")]
    [SerializeField] private DialogueLineData[] questInProgressDialogueLines;

    [Header("Turn In Quest Dialogue")]
    [SerializeField] private DialogueLineData[] turnInQuestDialogueLines;

    [Header("Invalid Dialogue")]
    [SerializeField] private DialogueLineData[] invalidDialogueLines =
    {
        new DialogueLineData { text = "Hi my lovely soldier!" }
    };

    private DialoguePhase activePhase = DialoguePhase.Invalid;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (CrabQuestManager.Instance == null)
        {
            Debug.LogWarning("CrabQuestManager not found.");
            return null;
        }

        if (GameProgress.crabCompleted)
        {
            activePhase = DialoguePhase.Invalid;
            return BuildDialogueList(invalidDialogueLines, speakerName);
        }

        if (!CrabQuestManager.Instance.QuestAccepted)
        {
            activePhase = DialoguePhase.AcceptQuest;
            return BuildDialogueList(acceptQuestDialogueLines, speakerName);
        }

        if (!CrabQuestManager.Instance.AllTrashCleared)
        {
            activePhase = DialoguePhase.InProgress;
            return BuildDialogueList(questInProgressDialogueLines, speakerName);
        }

        activePhase = DialoguePhase.TurnIn;
        return BuildDialogueList(turnInQuestDialogueLines, speakerName);
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
        SceneTransition.LoadScene(returnSceneName);
    }

    private enum DialoguePhase
    {
        AcceptQuest,
        InProgress,
        TurnIn,
        Invalid
    }
}
