using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestGuideObjectiveResolver
{
    private const string SettingsResourcePath = "QuestLocalizationSettings";
    private static QuestLocalizationSettings cachedSettings;

    private static QuestLocalizationSettings Settings
    {
        get
        {
            if (cachedSettings == null)
                cachedSettings = Resources.Load<QuestLocalizationSettings>(SettingsResourcePath);

            return cachedSettings;
        }
    }

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
            objective = QuestGuideObjective.CreateTalk(Settings.talkToWhale.GetText(), QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        if (GameProgress.crabCompleted && !GameProgress.whalePostCrabDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk(Settings.talkToWhale.GetText(), QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        CrabQuestManager crabQuestManager = CrabQuestManager.Instance;
        if (!GameProgress.crabCompleted)
        {
            if (crabQuestManager == null || !crabQuestManager.QuestAccepted)
            {
                objective = QuestGuideObjective.CreateTalk(Settings.talkToCrab.GetText(), QuestGuideAnchorId.Crab, "CrabAreaScene");
                return true;
            }

            if (!crabQuestManager.AllTrashCleared)
            {
                int totalTrash = Mathf.Max(crabQuestManager.InitialTrashCount, 1);
                int collectedTrash = Mathf.Clamp(totalTrash - crabQuestManager.RemainingTrash, 0, totalTrash);
                objective = QuestGuideObjective.CreateNonTalk(Settings.collectTrash.GetFormattedText(collectedTrash, totalTrash));
                return true;
            }

            objective = QuestGuideObjective.CreateTalk(Settings.talkToCrab.GetText(), QuestGuideAnchorId.Crab, "CrabAreaScene");
            return true;
        }

        if (!GameProgress.dolphinCompleted)
        {
            if (GameProgress.dolphinQuestAccepted)
            {
                objective = QuestGuideObjective.CreateNonTalk(Settings.findTheirMom.GetText());
                return true;
            }

            objective = QuestGuideObjective.CreateTalk(Settings.talkToDolphin.GetText(), QuestGuideAnchorId.Dolphin, "DolphinAreaScene");
            return true;
        }

        if (!GameProgress.whalePostDolphinDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk(Settings.talkToWhale.GetText(), QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        SharkEncounterManager sharkEncounterManager = Object.FindFirstObjectByType<SharkEncounterManager>();
        if (sceneName == "SharkScene" && !GameProgress.sharkCompleted)
        {
            if (sharkEncounterManager == null || sharkEncounterManager.CurrentPhase == SharkEncounterPhase.WaitingForInteraction)
            {
                objective = QuestGuideObjective.CreateTalk(Settings.talkToShark.GetText(), QuestGuideAnchorId.Shark, "SharkScene");
                return true;
            }

            if (sharkEncounterManager.CurrentPhase == SharkEncounterPhase.AttackChallenge)
            {
                objective = QuestGuideObjective.CreateNonTalk(Settings.fightTheShark.GetText());
                return true;
            }

            if (sharkEncounterManager.CurrentPhase == SharkEncounterPhase.Quiz)
            {
                objective = QuestGuideObjective.CreateNonTalk(Settings.answerToTheShark.GetText());
                return true;
            }

            if (sharkEncounterManager.CurrentPhase == SharkEncounterPhase.Completed)
            {
                objective = QuestGuideObjective.CreateTalk(Settings.talkToOctopus.GetText(), QuestGuideAnchorId.Octopus, "OctopusAreaScene");
                return true;
            }
        }

        if (!GameProgress.sharkCompleted)
        {
            objective = QuestGuideObjective.CreateTalk(Settings.talkToOctopus.GetText(), QuestGuideAnchorId.Octopus, "OctopusAreaScene");
            return true;
        }

        if (!GameProgress.octopusCompleted)
        {
            if (!GameProgress.octopusQuestAccepted || GameProgress.octopusTaskReadyToTurnIn)
            {
                objective = QuestGuideObjective.CreateTalk(Settings.talkToOctopus.GetText(), QuestGuideAnchorId.Octopus, "OctopusAreaScene");
                return true;
            }

            objective = QuestGuideObjective.CreateNonTalk(Settings.closeTheOilRigs.GetText());
            return true;
        }

        if (!GameProgress.whalePostOctopusDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk(Settings.talkToWhale.GetText(), QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        if (!GameProgress.sharkEscortToSeaCucumberRequested)
        {
            objective = QuestGuideObjective.CreateTalk(Settings.talkToShark.GetText(), QuestGuideAnchorId.Shark, "RQScene");
            return true;
        }

        if (!GameProgress.seaCucumberCompleted)
        {
            if (!GameProgress.seaCucumberQuestAccepted)
            {
                objective = QuestGuideObjective.CreateTalk(Settings.talkToSeaCucumber.GetText(), QuestGuideAnchorId.SeaCucumber, "SeaCucumberAreaScene");
                return true;
            }

            if (!GameProgress.seaCucumberTriageCompleted)
            {
                objective = QuestGuideObjective.CreateNonTalk(Settings.sortThePatients.GetText());
                return true;
            }

            bool needsInternalTreatment = !GameProgress.seaCucumberInternalTreatmentCompleted;
            bool needsExternalTreatment = !GameProgress.seaCucumberExternalTreatmentCompleted;
            if (needsInternalTreatment && needsExternalTreatment)
            {
                objective = QuestGuideObjective.CreateNonTalk(Settings.treatSickAndHurt.GetText());
                return true;
            }

            if (needsInternalTreatment)
            {
                objective = QuestGuideObjective.CreateNonTalk(Settings.treatSick.GetText());
                return true;
            }

            if (needsExternalTreatment)
            {
                objective = QuestGuideObjective.CreateNonTalk(Settings.treatHurt.GetText());
                return true;
            }

            objective = QuestGuideObjective.CreateTalk(Settings.talkToSeaCucumber.GetText(), QuestGuideAnchorId.SeaCucumber, "SeaCucumberAreaScene");
            return true;
        }

        if (!GameProgress.whalePostSeaCucumberDialogueCompleted)
        {
            objective = QuestGuideObjective.CreateTalk(Settings.talkToWhale.GetText(), QuestGuideAnchorId.Whale, "RQScene");
            return true;
        }

        if (!GameProgress.whaleEnsembleAccepted)
        {
            objective = QuestGuideObjective.CreateTalk(Settings.playTheSymphony.GetText(), QuestGuideAnchorId.Whale, "RQScene");
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
