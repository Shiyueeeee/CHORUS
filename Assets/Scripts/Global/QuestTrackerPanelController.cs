using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestTrackerPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private TMP_Text questText;

    [Header("Scene Visibility")]
    [SerializeField] private List<string> hiddenSceneNames = new List<string>
    {
        "TitleScene",
        "MapScene"
    };

    private string lastQuestText = "";
    private bool lastVisibility;
    private bool hasInitializedVisibility;
    private bool hasLoggedMissingReferences;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (!HasRequiredReferences())
            return;

        bool shouldShow = TryGetCurrentQuestText(out string questLabel);
        if (!hasInitializedVisibility || shouldShow != lastVisibility)
        {
            panelObject.SetActive(shouldShow);
            lastVisibility = shouldShow;
            hasInitializedVisibility = true;
        }

        if (shouldShow && questLabel != lastQuestText)
        {
            questText.text = questLabel;
            lastQuestText = questLabel;
        }
        else if (!shouldShow)
        {
            lastQuestText = "";
        }
    }

    private bool HasRequiredReferences()
    {
        if (panelObject != null && questText != null)
            return true;

        if (!hasLoggedMissingReferences)
        {
            Debug.LogWarning("QuestTrackerPanelController is missing UI references. Assign Panel Object and Quest Text in the Inspector.");
            hasLoggedMissingReferences = true;
        }

        return false;
    }

    private bool TryGetCurrentQuestText(out string questLabel)
    {
        questLabel = string.Empty;

        string sceneName = SceneManager.GetActiveScene().name;
        if (hiddenSceneNames.Contains(sceneName))
            return false;

        if (!GameProgress.rqIntroDialogueCompleted)
            return false;

        if (!GameProgress.whaleIntroDialogueCompleted)
        {
            questLabel = "Talk to Whale";
            return true;
        }

        if (GameProgress.crabCompleted && !GameProgress.whalePostCrabDialogueCompleted)
        {
            questLabel = "Talk to Whale";
            return true;
        }

        CrabQuestManager crabQuestManager = CrabQuestManager.Instance;
        if (!GameProgress.crabCompleted)
        {
            if (crabQuestManager == null || !crabQuestManager.QuestAccepted)
            {
                questLabel = "Talk to Crab";
                return true;
            }

            if (!crabQuestManager.AllTrashCleared)
            {
                int totalTrash = Mathf.Max(crabQuestManager.InitialTrashCount, 1);
                int collectedTrash = Mathf.Clamp(totalTrash - crabQuestManager.RemainingTrash, 0, totalTrash);
                questLabel = "Collect Trash (" + collectedTrash + "/" + totalTrash + ")";
                return true;
            }

            questLabel = "Talk to Crab";
            return true;
        }

        if (!GameProgress.dolphinCompleted)
        {
            questLabel = "Talk to Dolphin";
            return true;
        }

        if (!GameProgress.sharkCompleted)
        {
            questLabel = "Talk to Shark";
            return true;
        }

        if (!GameProgress.octopusCompleted)
        {
            questLabel = "Talk to Octopus";
            return true;
        }

        if (!GameProgress.seaCucumberCompleted)
        {
            questLabel = "Talk to Sea Cucumber";
            return true;
        }

        return false;
    }
}
