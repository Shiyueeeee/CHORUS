using UnityEngine;
using UnityEngine.SceneManagement;

public class MapUI : MonoBehaviour
{
    [Header("Portal Buttons")]
    [SerializeField] private GameObject crabPortalButton;
    [SerializeField] private GameObject dolphinPortalButton;
    [SerializeField] private GameObject octopusPortalButton;
    [SerializeField] private GameObject seaCucumberPortalButton;

    private void Start()
    {
        RefreshPortalButtons();
    }

    private void RefreshPortalButtons()
    {
        SetPortalActive(crabPortalButton, GameProgress.whaleIntroDialogueCompleted);
        SetPortalActive(dolphinPortalButton, GameProgress.whalePostCrabDialogueCompleted);
        SetPortalActive(octopusPortalButton, GameProgress.whalePostDolphinDialogueCompleted);
        SetPortalActive(seaCucumberPortalButton, GameProgress.whalePostOctopusDialogueCompleted && GameProgress.sharkEscortToSeaCucumberRequested);
    }

    private void SetPortalActive(GameObject portalButton, bool isUnlocked)
    {
        if (portalButton != null)
        {
            portalButton.SetActive(isUnlocked);
        }
    }

    public void OnClickClose()
    {
        SceneTransition.LoadScene("RQScene");
    }

    public void OnClickCrabArea()
    {
        if (GameProgress.crabCompleted)
        {
            SceneTransition.LoadScene("CrabAreaScene");
        }
        else
        {
            PlayerEnterState.movementMode = PlayerMovementMode.VerticalOnly;
            SceneTransition.LoadScene("CrabRushScene");
        }
    }

    public void OnClickDolphinArea()
    {
        if (GameProgress.dolphinCompleted)
        {
            SceneTransition.LoadScene("DolphinFinalAreaScene");
        }
        else
        {
            PlayerEnterState.movementMode = PlayerMovementMode.VerticalOnly;
            SceneTransition.LoadScene("DolphinRushScene");
        }
    }


    public void OnClickOctopusArea()
    {
        if (GameProgress.sharkCompleted)
        {
            SceneTransition.LoadScene("OctopusAreaScene");
            return;
        }

        SceneTransition.LoadScene("SharkScene");
    }

    public void OnClickSeaCucumberArea()
    {
        SceneTransition.LoadScene("SeaCucumberAreaScene");
    }
}
