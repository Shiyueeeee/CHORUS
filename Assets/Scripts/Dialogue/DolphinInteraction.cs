using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DolphinInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Dolphin";

    [Header("Init Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] initDialogueLines;

    [Header("Repeat Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] repeatDialogueLines =
    {
        "......"
    };

    [Header("Ship section Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] shipDialogueLines;
    
    [Header("Rush section Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] rushDialogueLines;
    
    [Header("End section Dialogue")]
    [TextArea(2, 5)]
    [SerializeField] private string[] endDialogueLines;

    [SerializeField] private GameObject objectToActivate;

    private DialoguePhase activePhase = DialoguePhase.Init;
    
    private DolphinFollowPositionController dolphinFollowPositionController;

    private void Awake()
    {
        dolphinFollowPositionController = GetComponent<DolphinFollowPositionController>();

        if (SceneManager.GetActiveScene().name == "DolphinAreaScene")
        {
            activePhase = DialoguePhase.Init;
        }
        else if (SceneManager.GetActiveScene().name != "ShipScene")
        {
            activePhase = DialoguePhase.Ship;
        }
        else
        {
            activePhase = DialoguePhase.Repeat;
        }
    }

    protected override string GetSpeakerName()
    {
        return  speakerName;
    }

    protected override List<string> GetDialogueLinesForCurrentState()
    {
        switch (activePhase)
        {
            case DialoguePhase.Repeat:
                return BuildDialogueList(repeatDialogueLines);
            case DialoguePhase.Init:
                return BuildDialogueList(initDialogueLines);
            case DialoguePhase.Ship:
                return BuildDialogueList(shipDialogueLines);
            case DialoguePhase.Rush:
                return BuildDialogueList(rushDialogueLines);
            case DialoguePhase.End:
                return BuildDialogueList(endDialogueLines);
            default:
                return null;
        }
        
    }

    protected override void OnDialogueFinished()
    {
        if (activePhase == DialoguePhase.Init)
        {
            // after init start follow
            dolphinFollowPositionController.canFollow = true;
            dolphinFollowPositionController.isFollowing = true;
        }
        activePhase = DialoguePhase.Repeat;
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
