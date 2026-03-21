using System.Collections.Generic;
using UnityEngine;

public class OctopusOilWellSwitchInteraction : NpcDialogueInteractionBase
{
    private enum DialoguePhase
    {
        CompleteOilWells,
        Repeat
    }

    [Header("Dialogue")]
    [SerializeField] private string speakerName = "Switch";
    [SerializeField] private DialogueLineData[] completeOilWellsDialogueLines;
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    [Header("Affected Objects")]
    [SerializeField] private GameObject oilGroupObject;
    [SerializeField] private GameObject radiationGroupObject;
    [SerializeField] private GameObject wallObjectToDisable;

    private DialoguePhase activePhase = DialoguePhase.Repeat;

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.octopusTaskReadyToTurnIn && !GameProgress.octopusCompleted)
        {
            activePhase = DialoguePhase.CompleteOilWells;
            return BuildDialogueList(completeOilWellsDialogueLines, speakerName);
        }

        activePhase = DialoguePhase.Repeat;
        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase != DialoguePhase.CompleteOilWells || GameProgress.octopusTaskReadyToTurnIn || GameProgress.octopusCompleted)
            return;

        GameProgress.octopusTaskReadyToTurnIn = true;

        if (oilGroupObject != null)
            oilGroupObject.SetActive(false);

        if (radiationGroupObject != null)
            radiationGroupObject.SetActive(false);

        if (wallObjectToDisable != null)
            wallObjectToDisable.SetActive(false);
    }
}
