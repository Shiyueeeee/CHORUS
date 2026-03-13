using System.Collections.Generic;
using UnityEngine;

public class WhaleInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Whale";

    [Header("Post Crab Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] postCrabQuestDialogueLines;

    [Header("Repeat Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] repeatDialogueLines =
    {
        "......"
    };

    [Header("Post Dolphin Quest Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] postDolphinQuestDialogueLines;

    [SerializeField] private GameObject objectToActivate;

    private DialoguePhase activePhase = DialoguePhase.Repeat;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<string> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.whaleIntroDialogueCompleted)
        {
            activePhase = DialoguePhase.PostCrabQuest;
            return BuildDialogueList(postCrabQuestDialogueLines);
        }

        if (GameProgress.crabCompleted && !GameProgress.whalePostCrabDialogueCompleted)
        {
            activePhase = DialoguePhase.PostDolphinQuest;
            return BuildDialogueList(postDolphinQuestDialogueLines);
        }

        activePhase = DialoguePhase.Repeat;
        return BuildDialogueList(repeatDialogueLines);
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
    }

    private enum DialoguePhase
    {
        PostCrabQuest,
        Repeat,
        PostDolphinQuest
    }
}
