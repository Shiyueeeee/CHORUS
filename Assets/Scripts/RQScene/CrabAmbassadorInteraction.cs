using System.Collections.Generic;
using UnityEngine;

public class CrabAmbassadorInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Crab Ambassador";

    [Header("Post Crab Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postCrabQuestDialogueLines;

    [Header("Post Dolphin Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postDolphinQuestDialogueLines;

    [Header("Post Shark Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postSharkQuestDialogueLines;

    [Header("Post Octopus Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postOctopusQuestDialogueLines;

    [Header("Post Sea Cucumber Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postSeaCucumberQuestDialogueLines;

    [Header("Repeat Dialogue")]
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    private DialoguePhase activePhase = DialoguePhase.Repeat;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.crabAmbassadorIntroDialogueCompleted)
        {
            activePhase = DialoguePhase.PostCrabQuest;
            return BuildDialogueList(postCrabQuestDialogueLines, speakerName);
        }

        if (GameProgress.dolphinCompleted && !GameProgress.crabAmbassadorPostDolphinDialogueCompleted)
        {
            activePhase = DialoguePhase.PostDolphinQuest;
            return BuildDialogueList(postDolphinQuestDialogueLines, speakerName);
        }

        if (GameProgress.sharkCompleted && !GameProgress.crabAmbassadorPostSharkDialogueCompleted)
        {
            activePhase = DialoguePhase.PostSharkQuest;
            return BuildDialogueList(postSharkQuestDialogueLines, speakerName);
        }

        if (GameProgress.octopusCompleted && !GameProgress.crabAmbassadorPostOctopusDialogueCompleted)
        {
            activePhase = DialoguePhase.PostOctopusQuest;
            return BuildDialogueList(postOctopusQuestDialogueLines, speakerName);
        }

        if (GameProgress.seaCucumberCompleted && !GameProgress.crabAmbassadorPostSeaCucumberDialogueCompleted)
        {
            activePhase = DialoguePhase.PostSeaCucumberQuest;
            return BuildDialogueList(postSeaCucumberQuestDialogueLines, speakerName);
        }

        activePhase = DialoguePhase.Repeat;
        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        Debug.Log("Crab Ambassador dialogue finished");

        switch (activePhase)
        {
            case DialoguePhase.PostCrabQuest:
                GameProgress.crabAmbassadorIntroDialogueCompleted = true;
                break;
            case DialoguePhase.PostDolphinQuest:
                GameProgress.crabAmbassadorPostDolphinDialogueCompleted = true;
                break;
            case DialoguePhase.PostSharkQuest:
                GameProgress.crabAmbassadorPostSharkDialogueCompleted = true;
                break;
            case DialoguePhase.PostOctopusQuest:
                GameProgress.crabAmbassadorPostOctopusDialogueCompleted = true;
                break;
            case DialoguePhase.PostSeaCucumberQuest:
                GameProgress.crabAmbassadorPostSeaCucumberDialogueCompleted = true;
                break;
        }
    }

    private enum DialoguePhase
    {
        PostCrabQuest,
        PostDolphinQuest,
        PostSharkQuest,
        PostOctopusQuest,
        PostSeaCucumberQuest,
        Repeat
    }
}
