using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SharkEncounterManager : MonoBehaviour
{
    public static bool StartFromAttackPhaseOnLoad;

    public SharkEncounterPhase CurrentPhase { get; private set; } = SharkEncounterPhase.WaitingForInteraction;

    [Header("Phase Roots")]
    [SerializeField] private GameObject[] waitingPhaseObjects;
    [SerializeField] private GameObject[] attackPhaseObjects;
    [SerializeField] private GameObject[] quizPhaseObjects;
    [SerializeField] private GameObject[] completedPhaseObjects;

    [Header("Completed Dialogue")]
    [SerializeField] private string completedDialogueSpeakerName = "Shark";
    [SerializeField] private DialogueLineData[] completedDialogueLines;
    [SerializeField] private string nextSceneName = "OctopusPathScene";

    [Header("Scene Intro Dialogue")]
    [SerializeField] private string sceneIntroDialogueSpeakerName = "Seahorse";
    [SerializeField] private DialogueLineData[] sceneIntroDialogueLines;

    private bool isResolvingCompletion;
    private bool shouldPlaySceneIntroDialogue;

    private void Start()
    {
        if (StartFromAttackPhaseOnLoad)
        {
            StartFromAttackPhaseOnLoad = false;
            CurrentPhase = SharkEncounterPhase.AttackChallenge;
        }
        else
        {
            shouldPlaySceneIntroDialogue = true;
        }

        ApplyPhaseState();
        TryPlaySceneIntroDialogue();
    }

    public void BeginAttackPhase()
    {
        if (CurrentPhase != SharkEncounterPhase.WaitingForInteraction)
            return;

        CurrentPhase = SharkEncounterPhase.AttackChallenge;
        ApplyPhaseState();
        Debug.Log("Shark encounter entered Phase 2: Attack Challenge.");
    }

    public void BeginQuizPhase()
    {
        if (CurrentPhase != SharkEncounterPhase.AttackChallenge)
            return;

        CurrentPhase = SharkEncounterPhase.Quiz;
        ApplyPhaseState();
        Debug.Log("Shark encounter entered Phase 3: Quiz.");
    }

    public void CompleteEncounter()
    {
        if (isResolvingCompletion)
            return;

        CurrentPhase = SharkEncounterPhase.Completed;
        ApplyPhaseState();
        Debug.Log("Shark encounter completed.");

        StartCompletionSequence();
    }

    private void ApplyPhaseState()
    {
        SetGroupActive(waitingPhaseObjects, CurrentPhase == SharkEncounterPhase.WaitingForInteraction);
        SetGroupActive(attackPhaseObjects, CurrentPhase == SharkEncounterPhase.AttackChallenge);
        SetGroupActive(quizPhaseObjects, CurrentPhase == SharkEncounterPhase.Quiz);
        SetGroupActive(completedPhaseObjects, CurrentPhase == SharkEncounterPhase.Completed);
    }

    private void SetGroupActive(GameObject[] objects, bool isActive)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);
            }
        }
    }

    private void StartCompletionSequence()
    {
        isResolvingCompletion = true;

        List<DialogueLineData> linesToUse = BuildDialogueList(completedDialogueLines, completedDialogueSpeakerName);
        if (linesToUse.Count > 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(linesToUse, FinalizeEncounterCompletion);
            return;
        }

        FinalizeEncounterCompletion();
    }

    private void TryPlaySceneIntroDialogue()
    {
        if (!shouldPlaySceneIntroDialogue || DialogueManager.Instance == null)
            return;

        List<DialogueLineData> linesToUse = BuildDialogueList(sceneIntroDialogueLines, sceneIntroDialogueSpeakerName);
        if (linesToUse.Count == 0)
            return;

        shouldPlaySceneIntroDialogue = false;
        DialogueManager.Instance.StartDialogue(linesToUse);
    }

    private void FinalizeEncounterCompletion()
    {
        GameProgress.sharkCompleted = true;
        SceneTransition.LoadScene(nextSceneName);
    }

    private static List<DialogueLineData> BuildDialogueList(DialogueLineData[] sourceLines, string fallbackSpeaker)
    {
        List<DialogueLineData> lines = new List<DialogueLineData>();
        if (sourceLines == null)
            return lines;

        foreach (DialogueLineData line in sourceLines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text))
                continue;

            lines.Add(new DialogueLineData
            {
                speaker = string.IsNullOrWhiteSpace(line.speaker) ? fallbackSpeaker : line.speaker,
                text = line.text
            });
        }

        return lines;
    }
}

public enum SharkEncounterPhase
{
    WaitingForInteraction,
    AttackChallenge,
    Quiz,
    Completed
}
