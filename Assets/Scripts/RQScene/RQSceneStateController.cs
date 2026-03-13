using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RQSceneStateController : MonoBehaviour
{
    [SerializeField] private GameObject crab;
    [SerializeField] private GameObject mapPortal;

    [Header("Seahorse Intro Dialogue")]
    [SerializeField] private string seahorseSpeakerName = "Seahorse";

    [TextArea(2, 5)]
    [SerializeField] private string[] firstEnterDialogueLines;

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

        List<string> lines = BuildDialogueList(firstEnterDialogueLines);
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("RQScene first-enter dialogue is not configured.");
            yield break;
        }

        DialogueManager.Instance.StartDialogue(
            seahorseSpeakerName,
            lines,
            OnFirstEnterDialogueFinished
        );
    }

    private List<string> BuildDialogueList(string[] sourceLines)
    {
        if (sourceLines == null || sourceLines.Length == 0)
            return null;

        List<string> lines = new List<string>();
        foreach (string line in sourceLines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        return lines.Count > 0 ? lines : null;
    }

    private void OnFirstEnterDialogueFinished()
    {
        GameProgress.rqIntroDialogueCompleted = true;
    }
}
