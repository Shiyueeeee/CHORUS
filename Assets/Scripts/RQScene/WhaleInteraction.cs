using System.Collections.Generic;
using UnityEngine;

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

    [Header("Post Dolphin Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postDolphinQuestDialogueLines;

    [Header("Post Shark Quest Dialogue")]
    [SerializeField] private DialogueLineData[] postSharkQuestDialogueLines;

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
            activePhase = DialoguePhase.PostSharkQuest;
            return BuildDialogueList(postSharkQuestDialogueLines, speakerName);
        }

        activePhase = DialoguePhase.Repeat;
        return BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        Debug.Log("Whale dialogue finished");

        if (activePhase == DialoguePhase.PostCrabQuest)
        {
            GameProgress.whaleIntroDialogueCompleted = true;
            Debug.Log("Crab quest accepted. Placeholder only.");

            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        }
        else if (activePhase == DialoguePhase.PostDolphinQuest)
        {
            GameProgress.whalePostCrabDialogueCompleted = true;
            Debug.Log("Dolphin quest accepted. Placeholder only.");
        }
        else if (activePhase == DialoguePhase.PostSharkQuest)
        {
            GameProgress.whalePostDolphinDialogueCompleted = true;
            Debug.Log("Octopus route task accepted. Shark encounter on the way. Placeholder only.");
        }
    }

    private enum DialoguePhase
    {
        PostCrabQuest,
        Repeat,
        PostDolphinQuest,
        PostSharkQuest
    }
}
