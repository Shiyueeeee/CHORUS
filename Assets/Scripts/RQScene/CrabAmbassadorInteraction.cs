using System.Collections.Generic;
using UnityEngine;

public class CrabAmbassadorInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Crab Ambassador";

    [Header("Post Crab Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] postCrabQuestDialogueLines;

    [Header("Post Dolphin Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] postDolphinQuestDialogueLines;

    [Header("Post Shark Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] postSharkQuestDialogueLines;

    [Header("Post Octopus Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] postOctopusQuestDialogueLines;

    [Header("Post Sea Cucumber Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] postSeaCucumberQuestDialogueLines;

    [Header("Repeat Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] repeatDialogueLines =
    {
        "......"
    };

    private DialoguePhase activePhase = DialoguePhase.Repeat;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<string> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.crabAmbassadorIntroDialogueCompleted)
        {
            activePhase = DialoguePhase.PostCrabQuest;
            return BuildDialogueList(postCrabQuestDialogueLines);
        }

        if (GameProgress.dolphinCompleted && !GameProgress.crabAmbassadorPostDolphinDialogueCompleted)
        {
            activePhase = DialoguePhase.PostDolphinQuest;
            return BuildDialogueList(postDolphinQuestDialogueLines);
        }

        if (GameProgress.sharkCompleted && !GameProgress.crabAmbassadorPostSharkDialogueCompleted)
        {
            activePhase = DialoguePhase.PostSharkQuest;
            return BuildDialogueList(postSharkQuestDialogueLines);
        }

        if (GameProgress.octopusCompleted && !GameProgress.crabAmbassadorPostOctopusDialogueCompleted)
        {
            activePhase = DialoguePhase.PostOctopusQuest;
            return BuildDialogueList(postOctopusQuestDialogueLines);
        }

        if (GameProgress.seaCucumberCompleted && !GameProgress.crabAmbassadorPostSeaCucumberDialogueCompleted)
        {
            activePhase = DialoguePhase.PostSeaCucumberQuest;
            return BuildDialogueList(postSeaCucumberQuestDialogueLines);
        }

        activePhase = DialoguePhase.Repeat;
        return BuildDialogueList(repeatDialogueLines);
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
