using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DolphinInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Dolphin";

    [Header("Init Dialogue")]
    [SerializeField] private DialogueLineData[] initDialogueLines;

    [Header("Repeat Dialogue")]
    [SerializeField] private DialogueLineData[] repeatDialogueLines =
    {
        new DialogueLineData { text = "......" }
    };

    [Header("Ship section Dialogue")]
    [SerializeField] private DialogueLineData[] shipDialogueLines;
    
    [Header("Rush section Dialogue")]
    [SerializeField] private DialogueLineData[] rushDialogueLines;
    
    [Header("End section Dialogue")]
    [SerializeField] private DialogueLineData[] endDialogueLines;

    [SerializeField] private GameObject objectToActivate;

    private DialoguePhase activePhase = DialoguePhase.Init;
    
    private DolphinFollowPositionController dolphinFollowPositionController;
    private Interactable interactable;

    private void Awake()
    {
        dolphinFollowPositionController = GetComponent<DolphinFollowPositionController>();
        interactable = GetComponent<Interactable>();

        if (SceneManager.GetActiveScene().name == "DolphinAreaScene")
        {
            activePhase = DialoguePhase.Init;
        }
        else if (SceneManager.GetActiveScene().name == "ShipScene")
        {
            activePhase = DialoguePhase.Ship;
        }
        else
        {
            activePhase = DialoguePhase.Repeat;
        }

        if (GameProgress.dolphinQuestAccepted)
        {
            DisableInteraction();
        }
    }

    protected override string GetSpeakerName()
    {
        return  speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        List<DialogueLineData> selectedLines;

        switch (activePhase)
        {
            case DialoguePhase.Repeat:
                selectedLines = BuildDialogueList(repeatDialogueLines, speakerName);
                break;
            case DialoguePhase.Init:
                selectedLines = BuildDialogueList(initDialogueLines, speakerName);
                break;
            case DialoguePhase.Ship:
                selectedLines = BuildDialogueList(shipDialogueLines, speakerName);
                break;
            case DialoguePhase.Rush:
                selectedLines = BuildDialogueList(rushDialogueLines, speakerName);
                break;
            case DialoguePhase.End:
                selectedLines = BuildDialogueList(endDialogueLines, speakerName);
                break;
            default:
                selectedLines = null;
                break;
        }

        if (selectedLines != null && selectedLines.Count > 0)
            return selectedLines;

        return BuildDialogueList(repeatDialogueLines, speakerName);
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase == DialoguePhase.Init)
        {
            GameProgress.dolphinQuestAccepted = true;

            // after init start follow
            dolphinFollowPositionController.canFollow = true;
            dolphinFollowPositionController.isFollowing = true;
            DisableInteraction();
        }
        activePhase = DialoguePhase.Repeat;
    }

    private void DisableInteraction()
    {
        if (interactable != null)
        {
            interactable.SetInteractionEnabled(false);
        }
    }

    private enum DialoguePhase
    {
        Repeat,
        Init, 
        Ship,
        Rush,
        End
    }
}
