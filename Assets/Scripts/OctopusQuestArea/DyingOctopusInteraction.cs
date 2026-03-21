using System.Collections.Generic;
using UnityEngine;

public class DyingOctopusInteraction : NpcDialogueInteractionBase
{
    private enum DialoguePhase
    {
        Hint,
        AliveRepeat,
        DeadRepeat
    }

    [SerializeField] private string speakerName = "Octopus";

    [Header("Before Oil Wells Are Closed")]
    [SerializeField] private DialogueLineData[] hintDialogueLines;
    [SerializeField] private DialogueLineData[] aliveRepeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomAliveRepeatDialogueBlocks;

    [Header("After Oil Wells Are Closed")]
    [SerializeField] private DialogueLineData[] deadRepeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };
    [SerializeField] private DialogueBlockData[] randomDeadRepeatDialogueBlocks;

    [Header("Visuals")]
    [SerializeField] private GameObject aliveVisualObject;
    [SerializeField] private GameObject deadVisualObject;

    private DialoguePhase activePhase;
    private bool lastOilWellsClosedState;

    private void Start()
    {
        RefreshVisualState();
    }

    private void Update()
    {
        bool oilWellsClosed = AreOilWellsClosed();
        if (oilWellsClosed == lastOilWellsClosedState)
            return;

        RefreshVisualState();
    }

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        bool oilWellsClosed = AreOilWellsClosed();
        if (oilWellsClosed)
        {
            activePhase = DialoguePhase.DeadRepeat;
            return BuildRandomDialogueList(randomDeadRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(deadRepeatDialogueLines, speakerName);
        }

        if (!GameProgress.octopusDyingNpcHintDialogueCompleted)
        {
            activePhase = DialoguePhase.Hint;
            return BuildDialogueList(hintDialogueLines, speakerName);
        }

        activePhase = DialoguePhase.AliveRepeat;
        return BuildRandomDialogueList(randomAliveRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(aliveRepeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase == DialoguePhase.Hint)
            GameProgress.octopusDyingNpcHintDialogueCompleted = true;
    }

    private bool AreOilWellsClosed()
    {
        return GameProgress.octopusTaskReadyToTurnIn || GameProgress.octopusCompleted;
    }

    private void RefreshVisualState()
    {
        bool oilWellsClosed = AreOilWellsClosed();
        lastOilWellsClosedState = oilWellsClosed;

        if (aliveVisualObject != null)
            aliveVisualObject.SetActive(!oilWellsClosed);

        if (deadVisualObject != null)
            deadVisualObject.SetActive(oilWellsClosed);
    }
}
