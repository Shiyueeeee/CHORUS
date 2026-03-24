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
    [SerializeField] private DialogueLineData[] completeOilWellsAfterDyingOctopusDialogueLines;
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    [Header("Affected Objects")]
    [SerializeField] private GameObject oilGroupObject;
    [SerializeField] private GameObject radiationGroupObject;
    [SerializeField] private GameObject wallObjectToDisable;

    [Header("Completed Visual")]
    [SerializeField] private SpriteRenderer switchSpriteRenderer;
    [SerializeField] private Sprite completedSprite;

    private Interactable interactable;

    private DialoguePhase activePhase = DialoguePhase.Repeat;

    private void Start()
    {
        interactable = GetComponent<Interactable>();
        RefreshSwitchVisualState();
    }

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        if (!GameProgress.octopusTaskReadyToTurnIn && !GameProgress.octopusCompleted)
        {
            activePhase = DialoguePhase.CompleteOilWells;

            if (GameProgress.octopusDyingNpcHintDialogueCompleted)
                return BuildDialogueList(completeOilWellsAfterDyingOctopusDialogueLines, speakerName) ?? BuildDialogueList(completeOilWellsDialogueLines, speakerName);

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

        RefreshSwitchVisualState();
    }

    private void RefreshSwitchVisualState()
    {
        bool oilWellsClosed = GameProgress.octopusTaskReadyToTurnIn || GameProgress.octopusCompleted;

        if (interactable != null)
            interactable.SetInteractionEnabled(!oilWellsClosed);

        if (oilWellsClosed && switchSpriteRenderer != null && completedSprite != null)
            switchSpriteRenderer.sprite = completedSprite;
    }
}
