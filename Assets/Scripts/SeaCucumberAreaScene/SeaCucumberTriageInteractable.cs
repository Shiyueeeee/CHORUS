using UnityEngine;
using System.Collections.Generic;

public class SeaCucumberTriageInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private SeaCucumberTriageManager triageManager;
    [SerializeField] private string nurseSpeakerName = "Sea Cucumber Nurse";
    [SerializeField] private DialogueLineData[] guideDialogueLines;

    public void Interact()
    {
        if (DialogueManager.IsDialogueActive || triageManager == null)
            return;

        List<DialogueLineData> guideLines = BuildDialogueList(guideDialogueLines, nurseSpeakerName);
        if (guideLines != null && guideLines.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(guideLines, OnGuideDialogueFinished);
            return;
        }

        triageManager.TryOpenPuzzle();
    }

    private void OnGuideDialogueFinished()
    {
        triageManager.TryOpenPuzzle();
    }

    private List<DialogueLineData> BuildDialogueList(DialogueLineData[] sourceLines, string fallbackSpeaker)
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
                speaker = string.IsNullOrWhiteSpace(line.speaker) ? fallbackSpeaker : line.speaker,
                text = line.text
            });
        }

        return lines.Count > 0 ? lines : null;
    }
}
