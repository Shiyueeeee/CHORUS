using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarratorInteraction : NpcDialogueInteractionBase
{
    [SerializeField] private string speakerName = "Narrator";
    [SerializeField] private DialogueLineData[] dialogueLines;

    [Header("Scene Load")]
    [SerializeField] private bool triggerOnSceneLoad = true;
    [SerializeField] private float triggerDelaySeconds = 0f;

    private bool hasTriggeredOnSceneLoad;

    private void OnValidate()
    {
        triggerDelaySeconds = Mathf.Max(0f, triggerDelaySeconds);
    }

    private void Start()
    {
        if (!triggerOnSceneLoad || hasTriggeredOnSceneLoad)
            return;

        hasTriggeredOnSceneLoad = true;
        StartCoroutine(TriggerDialogueOnSceneLoad());
    }

    private IEnumerator TriggerDialogueOnSceneLoad()
    {
        yield return null;

        if (triggerDelaySeconds > 0f)
            yield return new WaitForSecondsRealtime(triggerDelaySeconds);

        yield return new WaitUntil(() => DialogueManager.Instance != null);
        yield return new WaitUntil(() => !DialogueManager.IsDialogueActive);

        Interact();
    }

    protected override string GetSpeakerName()
    {
        return speakerName;
    }

    protected override List<DialogueLineData> GetDialogueLinesForCurrentState()
    {
        return BuildDialogueList(dialogueLines, GetSpeakerName());
    }

    protected override void OnDialogueFinished()
    {
    }
}
