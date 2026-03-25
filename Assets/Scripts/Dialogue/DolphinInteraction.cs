using System.Collections;
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
    [SerializeField] private DialogueBlockData[] randomRepeatDialogueBlocks;

    [Header("Ship section Dialogue")]
    [SerializeField] private DialogueLineData[] shipDialogueLines;
    
    [Header("Rush section Dialogue")]
    [SerializeField] private DialogueLineData[] rushDialogueLines;
    
    [Header("End section Dialogue")]
    [SerializeField] private DialogueLineData[] endDialogueLines;

    [SerializeField] private GameObject objectToActivate;

    [Header("Scene Load")]
    [SerializeField] private bool triggerCurrentPhaseOnSceneLoad;
    [SerializeField] private float sceneLoadTriggerDelaySeconds = 0f;
    [SerializeField] private bool startRushSceneSystemsAfterDialogue = true;

    private DialoguePhase activePhase = DialoguePhase.Init;
    private bool hasTriggeredCurrentPhaseOnSceneLoad;

    private DolphinFollowPositionController dolphinFollowPositionController;
    private Interactable interactable;
    private CrabRushManager rushManager;
    private TrashSpawner rushTrashSpawner;

    private void OnValidate()
    {
        sceneLoadTriggerDelaySeconds = Mathf.Max(0f, sceneLoadTriggerDelaySeconds);
    }

    private void Awake()
    {
        dolphinFollowPositionController = GetComponent<DolphinFollowPositionController>();
        interactable = GetComponent<Interactable>();
        activePhase = ResolveDialoguePhaseForActiveScene();

        if (activePhase == DialoguePhase.Rush)
        {
            rushManager = FindFirstObjectByType<CrabRushManager>();
            rushTrashSpawner = FindFirstObjectByType<TrashSpawner>();
        }

        if (GameProgress.dolphinQuestAccepted)
        {
            DisableInteraction();
        }
    }

    private void Start()
    {
        if (!triggerCurrentPhaseOnSceneLoad || hasTriggeredCurrentPhaseOnSceneLoad)
        {
            return;
        }

        hasTriggeredCurrentPhaseOnSceneLoad = true;

        if (!HasDialogueForCurrentState())
        {
            StartRushSceneSystemsIfNeeded();
            return;
        }

        StartCoroutine(TriggerDialogueOnSceneLoad());
    }

    private IEnumerator TriggerDialogueOnSceneLoad()
    {
        yield return null;

        if (sceneLoadTriggerDelaySeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(sceneLoadTriggerDelaySeconds);
        }

        yield return new WaitUntil(() => DialogueManager.Instance != null);
        yield return new WaitUntil(() => !DialogueManager.IsDialogueActive);

        Interact();
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
                selectedLines = BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
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

        return BuildRandomDialogueList(randomRepeatDialogueBlocks, speakerName) ?? BuildDialogueList(repeatDialogueLines, speakerName);
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

        StartRushSceneSystemsIfNeeded();
        activePhase = DialoguePhase.Repeat;
    }

    private DialoguePhase ResolveDialoguePhaseForActiveScene()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "DolphinAreaScene":
                return DialoguePhase.Init;
            case "ShipScene":
                return DialoguePhase.Ship;
            case "DolphinFinalRushScene":
                return DialoguePhase.Rush;
            case "DolphinFinalAreaScene":
                return DialoguePhase.End;
            default:
                return DialoguePhase.Repeat;
        }
    }

    private bool HasDialogueForCurrentState()
    {
        List<DialogueLineData> lines = GetDialogueLinesForCurrentState();
        return lines != null && lines.Count > 0;
    }

    private void StartRushSceneSystemsIfNeeded()
    {
        if (!startRushSceneSystemsAfterDialogue || activePhase != DialoguePhase.Rush)
        {
            return;
        }

        if (rushManager != null)
        {
            rushManager.BeginRush();
        }

        if (rushTrashSpawner != null)
        {
            rushTrashSpawner.BeginSpawning();
        }
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
