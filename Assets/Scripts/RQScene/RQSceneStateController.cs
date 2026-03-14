using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RQSceneStateController : MonoBehaviour
{
    [SerializeField] private GameObject crab;
    [SerializeField] private GameObject mapPortal;

    [Header("Seahorse Intro Dialogue")]
    [SerializeField] private string seahorseSpeakerName = "Seahorse";
    [SerializeField] private DialogueLineData[] firstEnterDialogueLines;

    private void Start()
    {
        if (crab != null)
        {
            crab.SetActive(GameProgress.crabCompleted);
        }

        if (mapPortal != null)
        {
            mapPortal.SetActive(GameProgress.whaleIntroDialogueCompleted);
        }

        if (!GameProgress.rqIntroDialogueCompleted)
        {
            StartCoroutine(PlayFirstEnterDialogue());
        }
    }

    private IEnumerator PlayFirstEnterDialogue()
    {
        yield return null;

        if (GameProgress.rqIntroDialogueCompleted)
            yield break;

        if (DialogueManager.Instance == null || DialogueManager.IsDialogueActive)
            yield break;

        List<DialogueLineData> lines = BuildDialogueList(firstEnterDialogueLines);
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("RQScene first-enter dialogue is not configured.");
            yield break;
        }

        DialogueManager.Instance.StartDialogue(
            lines,
            OnFirstEnterDialogueFinished
        );
    }

    private List<DialogueLineData> BuildDialogueList(DialogueLineData[] sourceLines)
    {
        if (sourceLines == null || sourceLines.Length == 0)
            return null;

        List<DialogueLineData> lines = new List<DialogueLineData>();
        foreach (DialogueLineData line in sourceLines)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text))
                continue;

            lines.Add(new DialogueLineData
            {
                speaker = string.IsNullOrWhiteSpace(line.speaker) ? seahorseSpeakerName : line.speaker,
                text = line.text
            });
        }

        return lines.Count > 0 ? lines : null;
    }

    private void OnFirstEnterDialogueFinished()
    {
        GameProgress.rqIntroDialogueCompleted = true;
    }
}
