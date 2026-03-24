using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OctopusLeaderInteraction : NpcDialogueInteractionBase
{
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "Octopus Leader";
    [SerializeField] private string returnSceneName = "RQScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_OctopusReturn";
    [SerializeField] private DialogueLineData[] acceptQuestDialogueLines;
    [SerializeField] private DialogueLineData[] questInProgressDialogueLines;
    [SerializeField] private DialogueLineData[] turnInQuestDialogueLines;
    [SerializeField] private DialogueLineData[] repeatDialogueLines;
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    [Header("Portal")]
    [SerializeField] private GameObject taskAreaPortal;

    private OctopusLeaderDialoguePhase activePhase = OctopusLeaderDialoguePhase.Repeat;

    private void Start()
    {
        RefreshPortalState();
    }

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.octopusQuestAccepted)
        {
            activePhase = OctopusLeaderDialoguePhase.AcceptQuest;
            return BuildDialogueList(acceptQuestDialogueLines, speakerName);
        }

        if (GameProgress.octopusTaskReadyToTurnIn && !GameProgress.octopusCompleted)
        {
            activePhase = OctopusLeaderDialoguePhase.TurnInQuest;
            return BuildDialogueList(turnInQuestDialogueLines, speakerName);
        }

        if (!GameProgress.octopusCompleted)
        {
            activePhase = OctopusLeaderDialoguePhase.QuestInProgress;
            return BuildDialogueList(questInProgressDialogueLines, speakerName);
        }

        activePhase = OctopusLeaderDialoguePhase.Repeat;
        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase == OctopusLeaderDialoguePhase.AcceptQuest && !GameProgress.octopusQuestAccepted)
        {
            GameProgress.octopusQuestAccepted = true;
            RefreshPortalState();
            Debug.Log("Octopus quest accepted. Placeholder only.");
            return;
        }

        if (activePhase == OctopusLeaderDialoguePhase.TurnInQuest && GameProgress.octopusTaskReadyToTurnIn && !GameProgress.octopusCompleted)
        {
            GameProgress.octopusTaskReadyToTurnIn = false;
            GameProgress.octopusCompleted = true;
            SceneSpawnData.returnSpawnPointName = returnSpawnPointName;
            GameSaveSystem.SaveCheckpoint(returnSceneName, returnSpawnPointName);
            SceneTransition.LoadScene(returnSceneName);
        }
    }

    private void RefreshPortalState()
    {
        if (taskAreaPortal != null)
            taskAreaPortal.SetActive(GameProgress.octopusQuestAccepted);
    }

    private enum OctopusLeaderDialoguePhase
    {
        AcceptQuest,
        QuestInProgress,
        TurnInQuest,
        Repeat
    }
}
