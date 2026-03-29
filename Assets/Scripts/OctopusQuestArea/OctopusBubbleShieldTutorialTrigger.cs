using System.Collections.Generic;
using UnityEngine;

public class OctopusBubbleShieldTutorialTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "Octopus";
    [SerializeField] private DialogueLineData[] unlockDialogueLines;

    [Header("References")]
    [SerializeField] private OctopusBubbleShield bubbleShield;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject spacePromptObject;
    [SerializeField] private Collider2D triggerCollider;

    [Header("Behaviour")]
    [SerializeField] private bool lockAbilityUntilDialogue = true;
    [SerializeField] private bool disableTriggerAfterUnlock = true;

    private bool hasStartedDialogue;

    private void Reset()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        if (bubbleShield == null)
            bubbleShield = FindFirstObjectByType<OctopusBubbleShield>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (bubbleShield != null)
            bubbleShield.ShieldActivated += HandleShieldActivated;

        if (lockAbilityUntilDialogue && bubbleShield != null && !bubbleShield.IsUnlocked)
            bubbleShield.SetAbilityUnlocked(false);

        RefreshPromptState();
    }

    private void OnDestroy()
    {
        if (bubbleShield != null)
            bubbleShield.ShieldActivated -= HandleShieldActivated;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasStartedDialogue || (bubbleShield != null && bubbleShield.IsUnlocked))
            return;

        List<DialogueLineData> lines = BuildDialogueList(unlockDialogueLines, speakerName);
        if (lines == null || lines.Count == 0 || DialogueManager.Instance == null)
            return;

        hasStartedDialogue = true;
        DialogueManager.Instance.StartDialogue(lines, OnUnlockDialogueFinished);
    }

    private void OnUnlockDialogueFinished()
    {
        if (bubbleShield != null)
            bubbleShield.SetAbilityUnlocked(true);

        RefreshPromptState();

        if (disableTriggerAfterUnlock && triggerCollider != null)
            triggerCollider.enabled = false;
    }

    private void HandleShieldActivated()
    {
        RefreshPromptState();
    }

    private void Update()
    {
        RefreshPromptState();
    }

    private void RefreshPromptState()
    {
        if (spacePromptObject == null)
            return;

        bool shouldShow = bubbleShield != null
            && bubbleShield.IsUnlocked
            && !bubbleShield.IsTutorialCompleted
            && (playerHealth == null || !playerHealth.IsGameOverVisible);

        spacePromptObject.SetActive(shouldShow);
    }

    private static List<DialogueLineData> BuildDialogueList(DialogueLineData[] sourceLines, string fallbackSpeaker)
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
                translatedSpeaker = line.translatedSpeaker,
                text = line.text,
                translatedText = line.translatedText
            });
        }

        return lines.Count > 0 ? lines : null;
    }
}
