using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WhaleInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Whale";

    [Header("Post Crab Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postCrabQuestDialogueLines;

    [Header("Repeat Dialogue")]
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    [Header("Post Dolphin Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postDolphinQuestDialogueLines;

    [Header("Post Octopus Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postOctopusQuestDialogueLines;

    [Header("Post Sea Cucumber Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postSeaCucumberQuestDialogueLines;

    [Header("After Sea Cucumber Completed Dialogue")]
    [SerializeField] private DialogueLineData[] afterSeaCucumberCompletedDialogueLines;

    [Header("Ensemble Prompt Dialogue")]
    [SerializeField] private DialogueLineData[] ensemblePromptDialogueLines;
    [SerializeField] private string ensembleYesLabel = "Yes";
    [SerializeField] private string ensembleNoLabel = "No";

    [Header("Ending")]
    [SerializeField] private string endingSceneName = "ChorusScene";
    [SerializeField] private GameObject objectToActivate;

    private DialoguePhase activePhase = DialoguePhase.Repeat;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.whaleIntroDialogueCompleted)
        {
            activePhase = DialoguePhase.PostCrabQuest;
            return BuildDialogueList(postCrabQuestDialogueLines, speakerName);
        }

        if (GameProgress.crabCompleted && !GameProgress.whalePostCrabDialogueCompleted)
        {
            activePhase = DialoguePhase.PostDolphinQuest;
            return BuildDialogueList(postDolphinQuestDialogueLines, speakerName);
        }

        if (GameProgress.dolphinCompleted && !GameProgress.whalePostDolphinDialogueCompleted)
        {
            activePhase = DialoguePhase.PostOctopusQuest;
            return BuildDialogueList(postOctopusQuestDialogueLines, speakerName);
        }

        if (GameProgress.octopusCompleted && !GameProgress.whalePostOctopusDialogueCompleted)
        {
            activePhase = DialoguePhase.PostSeaCucumberQuest;
            return BuildDialogueList(postSeaCucumberQuestDialogueLines, speakerName);
        }

        if (GameProgress.seaCucumberCompleted && !GameProgress.whalePostSeaCucumberDialogueCompleted)
        {
            activePhase = DialoguePhase.AfterSeaCucumberCompleted;
            return BuildDialogueList(afterSeaCucumberCompletedDialogueLines, speakerName);
        }

        if (GameProgress.whalePostSeaCucumberDialogueCompleted && !GameProgress.whaleEnsembleAccepted)
        {
            activePhase = DialoguePhase.EnsemblePrompt;
            return BuildDialogueList(ensemblePromptDialogueLines, speakerName);
        }

        activePhase = DialoguePhase.Repeat;
        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase == DialoguePhase.PostCrabQuest)
        {
            GameProgress.whaleIntroDialogueCompleted = true;

            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        }
        else if (activePhase == DialoguePhase.PostDolphinQuest)
        {
            GameProgress.whalePostCrabDialogueCompleted = true;
        }
        else if (activePhase == DialoguePhase.PostOctopusQuest)
        {
            GameProgress.whalePostDolphinDialogueCompleted = true;
        }
        else if (activePhase == DialoguePhase.PostSeaCucumberQuest)
        {
            GameProgress.whalePostOctopusDialogueCompleted = true;
        }
        else if (activePhase == DialoguePhase.AfterSeaCucumberCompleted)
        {
            GameProgress.whalePostSeaCucumberDialogueCompleted = true;
        }
        else if (activePhase == DialoguePhase.EnsemblePrompt)
        {
            string choiceQuestion = GetLastDialogueText(ensemblePromptDialogueLines, "Are you ready for the ensemble?");
            DialogueManager.Instance.StartYesNoChoice(
                speakerName,
                choiceQuestion,
                StartEnsemble,
                null,
                ensembleYesLabel,
                ensembleNoLabel);
        }
    }

    private void StartEnsemble()
    {
        GameProgress.whaleEnsembleAccepted = true;

        if (!string.IsNullOrWhiteSpace(endingSceneName))
            SceneTransition.LoadScene(endingSceneName);
    }

    private string GetLastDialogueText(DialogueLineData[] sourceLines, string fallbackText)
    {
        if (sourceLines == null || sourceLines.Length == 0)
            return fallbackText;

        for (int i = sourceLines.Length - 1; i >= 0; i--)
        {
            DialogueLineData line = sourceLines[i];
            if (line == null)
                continue;

            string localizedText = line.GetLocalizedText();
            if (!string.IsNullOrWhiteSpace(localizedText))
                return localizedText;
        }

        return fallbackText;
    }

    private enum DialoguePhase
    {
        PostCrabQuest,
        Repeat,
        PostDolphinQuest,
        PostOctopusQuest,
        PostSeaCucumberQuest,
        AfterSeaCucumberCompleted,
        EnsemblePrompt
    }
}
