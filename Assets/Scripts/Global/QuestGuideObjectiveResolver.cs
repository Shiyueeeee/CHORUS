using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestGuideObjectiveResolver
{
    public static bool TryGetCurrentObjective(out QuestGuideObjective objective)
    {
        objective = QuestGuideObjective.None;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "TitleScene" || sceneName == "MapScene")
            return false;

        if (!GameProgress.rqIntroDialogueCompleted)
            return false;

        if (!GameProgress.whaleIntroDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk("Talk to Whale", QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        if (GameProgress.crabCompleted && !GameProgress.whalePostCrabDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk("Talk to Whale", QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        CrabQuestManager crabQuestManager = CrabQuestManager.Instance;
        if (!GameProgress.crabCompleted)
        {
            if (crabQuestManager == null || !crabQuestManager.QuestAccepted)
            {
                objective = QuestGuideObjective.CreateTalk("Talk to Crab", QuestGuideAnchorId.Crab, "CrabAreaScene");
                return true;
            }

            if (!crabQuestManager.AllTrashCleared)
            {
                int totalTrash = Mathf.Max(crabQuestManager.InitialTrashCount, 1);
                int collectedTrash = Mathf.Clamp(totalTrash - crabQuestManager.RemainingTrash, 0, totalTrash);
                objective = QuestGuideObjective.CreateNonTalk("Collect Trash (" + collectedTrash + "/" + totalTrash + ")");
                return true;
            }

            objective = QuestGuideObjective.CreateTalk("Talk to Crab", QuestGuideAnchorId.Crab, "CrabAreaScene");
            return true;
        }

        if (!GameProgress.dolphinCompleted)
        {
            if (GameProgress.dolphinQuestAccepted)
            {
                objective = QuestGuideObjective.CreateNonTalk("Find Their Mom");
                return true;
            }

            objective = QuestGuideObjective.CreateTalk("Talk to Dolphin", QuestGuideAnchorId.Dolphin, "DolphinAreaScene");
            return true;
        }

        if (!GameProgress.whalePostDolphinDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk("Talk to Whale", QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        SharkEncounterManager sharkEncounterManager = Object.FindFirstObjectByType<SharkEncounterManager>();
        if (sceneName == "SharkScene" && !GameProgress.sharkCompleted)
        {
            if (sharkEncounterManager == null || sharkEncounterManager.CurrentPhase == SharkEncounterPhase.WaitingForInteraction)
            {
                objective = QuestGuideObjective.CreateTalk("Talk to Shark", QuestGuideAnchorId.Shark, "SharkScene");
                return true;
            }

            if (sharkEncounterManager.CurrentPhase == SharkEncounterPhase.AttackChallenge)
            {
                objective = QuestGuideObjective.CreateNonTalk("Fight the Shark");
                return true;
            }

            if (sharkEncounterManager.CurrentPhase == SharkEncounterPhase.Quiz)
            {
                objective = QuestGuideObjective.CreateNonTalk("Answer to the Shark");
                return true;
            }

            if (sharkEncounterManager.CurrentPhase == SharkEncounterPhase.Completed)
            {
                objective = QuestGuideObjective.CreateTalk("Talk to Octopus", QuestGuideAnchorId.Octopus, "OctopusAreaScene");
                return true;
            }
        }

        if (!GameProgress.sharkCompleted)
        {
            objective = QuestGuideObjective.CreateTalk("Talk to Octopus", QuestGuideAnchorId.Octopus, "OctopusAreaScene");
            return true;
        }

        if (!GameProgress.octopusCompleted)
        {
            if (!GameProgress.octopusQuestAccepted || GameProgress.octopusTaskReadyToTurnIn)
            {
                objective = QuestGuideObjective.CreateTalk("Talk to Octopus", QuestGuideAnchorId.Octopus, "OctopusAreaScene");
                return true;
            }

            objective = QuestGuideObjective.CreateNonTalk("Close the Oil Rigs");
            return true;
        }

        if (!GameProgress.whalePostOctopusDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk("Talk to Whale", QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        if (!GameProgress.sharkEscortToSeaCucumberRequested)
        {
            objective = QuestGuideObjective.CreateTalk("Talk to Shark", QuestGuideAnchorId.Shark, "RQScene");
            return true;
        }

        if (!GameProgress.seaCucumberCompleted)
        {
            if (!GameProgress.seaCucumberQuestAccepted)
            {
                objective = QuestGuideObjective.CreateTalk("Talk to Sea Cucumber", QuestGuideAnchorId.SeaCucumber, "SeaCucumberAreaScene");
                return true;
            }

            if (!GameProgress.seaCucumberTriageCompleted)
            {
                objective = QuestGuideObjective.CreateNonTalk("Sort the Patients");
                return true;
            }

            bool needsInternalTreatment = !GameProgress.seaCucumberInternalTreatmentCompleted;
            bool needsExternalTreatment = !GameProgress.seaCucumberExternalTreatmentCompleted;
            if (needsInternalTreatment && needsExternalTreatment)
            {
                objective = QuestGuideObjective.CreateNonTalk("Treat\n-Sick pile\n-Hurt pile");
                return true;
            }

            if (needsInternalTreatment)
            {
                objective = QuestGuideObjective.CreateNonTalk("Treat\n-Sick pile");
                return true;
            }

            if (needsExternalTreatment)
            {
                objective = QuestGuideObjective.CreateNonTalk("Treat\n-Hurt pile");
                return true;
            }

            objective = QuestGuideObjective.CreateTalk("Talk to Sea Cucumber", QuestGuideAnchorId.SeaCucumber, "SeaCucumberAreaScene");
            return true;
        }

        if (!GameProgress.whalePostSeaCucumberDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk("Talk to Whale", QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        if (!GameProgress.whaleEnsembleAccepted)
        {
            objective = QuestGuideObjective.CreateTalk("Play the Symphony", QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        return false;
    }
}

public struct QuestGuideObjective
{
    public string Label;
    public bool IsTalkObjective;
    public QuestGuideAnchorId AnchorId;
    public string TargetSceneName;

    public static QuestGuideObjective None => new QuestGuideObjective
    {
        Label = string.Empty,
        IsTalkObjective = false,
        AnchorId = QuestGuideAnchorId.None,
        TargetSceneName = string.Empty
    };

    public static QuestGuideObjective CreateTalk(string label, QuestGuideAnchorId anchorId, string targetSceneName)
    {
        return new QuestGuideObjective
        {
            Label = label,
            IsTalkObjective = true,
            AnchorId = anchorId,
            TargetSceneName = targetSceneName
        };
    }

    public static QuestGuideObjective CreateNonTalk(string label)
    {
        return new QuestGuideObjective
        {
            Label = label,
            IsTalkObjective = false,
            AnchorId = QuestGuideAnchorId.None,
            TargetSceneName = string.Empty
        };
    }
}
