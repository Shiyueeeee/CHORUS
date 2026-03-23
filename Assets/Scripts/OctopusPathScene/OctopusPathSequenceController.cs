using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OctopusPathSequenceController : MonoBehaviour
{
    [Header("Travel Pair")]
    [SerializeField] private Transform travelPairTransform;

    [Header("Targets")]
    [SerializeField] private Transform dialogueTarget;
    [SerializeField] private Transform exitTarget;

    [Header("Movement")]
    [SerializeField] private float entrySpeed = 4f;
    [SerializeField] private float exitSpeed = 5f;
    [SerializeField] private float arrivalThreshold = 0.05f;

    [Header("Dialogue")]
    [SerializeField] private string defaultSpeakerName = "Shark";
    [SerializeField] private DialogueLineData[] travelDialogueLines;
    [SerializeField] private string nextSceneName = "OctopusAreaScene";

    private bool sequenceStarted;

    private void Start()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        yield return MovePairToTarget(dialogueTarget, entrySpeed);

        List<DialogueLineData> dialogueLines = BuildDialogueList(travelDialogueLines, defaultSpeakerName);
        if (dialogueLines.Count > 0 && DialogueManager.Instance != null)
        {
            bool finishedDialogue = false;
            DialogueManager.Instance.StartDialogue(dialogueLines, () => finishedDialogue = true);
            yield return new WaitUntil(() => finishedDialogue);
        }

        yield return MovePairToTarget(exitTarget, exitSpeed);
        SceneTransition.LoadScene(nextSceneName);
    }

    private IEnumerator MovePairToTarget(Transform target, float moveSpeed)
    {
        if (travelPairTransform == null || target == null)
            yield break;

        while (!HasReached(travelPairTransform, target))
        {
            travelPairTransform.position = MoveTowards(travelPairTransform.position, target.position, moveSpeed);
            yield return null;
        }
    }

    private bool HasReached(Transform actor, Transform target)
    {
        return Vector3.Distance(actor.position, target.position) <= arrivalThreshold;
    }

    private Vector3 MoveTowards(Vector3 current, Vector3 target, float moveSpeed)
    {
        return Vector3.MoveTowards(current, target, moveSpeed * Time.deltaTime);
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
