using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    private bool hasConfiguredRaycasts;
    private TMP_FontAsset defaultQuestFont;

    private void Start()
    {
        if (questText != null)
            defaultQuestFont = questText.font;

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

        EnsurePanelDoesNotBlockRaycasts();

        bool shouldShow = TryGetCurrentQuestText(out string questLabel);
        if (!hasInitializedVisibility || shouldShow != lastVisibility)
        {
            panelObject.SetActive(shouldShow);
            lastVisibility = shouldShow;
            hasInitializedVisibility = true;
        }

        if (shouldShow && questLabel != lastQuestText)
        {
            ApplyLocalizedFont();
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

    private void EnsurePanelDoesNotBlockRaycasts()
    {
        if (hasConfiguredRaycasts || panelObject == null)
            return;

        Graphic[] graphics = panelObject.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            if (graphic != null)
                graphic.raycastTarget = false;
        }

        CanvasGroup[] canvasGroups = panelObject.GetComponentsInChildren<CanvasGroup>(true);
        foreach (CanvasGroup canvasGroup in canvasGroups)
        {
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
        }

        hasConfiguredRaycasts = true;
    }

    private bool TryGetCurrentQuestText(out string questLabel)
    {
        questLabel = string.Empty;

        string sceneName = SceneManager.GetActiveScene().name;
        if (hiddenSceneNames.Contains(sceneName))
            return false;

        if (!QuestGuideObjectiveResolver.TryGetCurrentObjective(out QuestGuideObjective objective))
            return false;

        questLabel = objective.Label;
        return !string.IsNullOrEmpty(questLabel);
    }

    private void ApplyLocalizedFont()
    {
        if (questText == null)
            return;

        TMP_FontAsset chineseFont = LanguageSettingsManager.GetChineseUiFontOrNull();
        questText.font = LanguageSettingsManager.CurrentLanguage == SupportedLanguage.Chinese && chineseFont != null
            ? chineseFont
            : defaultQuestFont;
    }
}
