using System.Collections.Generic;
using UnityEngine;

public class SharkInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Shark";
    [SerializeField] private SharkEncounterManager encounterManager;

    [Header("Phase 1 Intro Dialogue")]
    [SerializeField] private DialogueLineData[] introDialogueLines;

    private bool playedIntroThisInteraction;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        playedIntroThisInteraction = false;

        if (encounterManager == null)
        {
            Debug.LogWarning("SharkInteraction is missing a SharkEncounterManager reference.");
            return null;
        }

        if (encounterManager.CurrentPhase != SharkEncounterPhase.WaitingForInteraction)
        {
            return null;
        }

        playedIntroThisInteraction = true;
        return BuildDialogueList(introDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (playedIntroThisInteraction && encounterManager != null)
        {
            encounterManager.BeginAttackPhase();
        }
    }
}
