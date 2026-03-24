using UnityEngine;

public class DebugJumpToPreFinalWhaleDialogueButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "RQScene";
    [SerializeField] private string returnSpawnPointName = "Spawn_SeaCucumberReturn";

    public void JumpToPreFinalWhaleDialogue()
    {
        GameProgress.ResetAll();

        GameProgress.rqIntroDialogueCompleted = true;

        GameProgress.crabCompleted = true;
        GameProgress.dolphinCompleted = true;
        GameProgress.sharkCompleted = true;
        GameProgress.octopusCompleted = true;
        GameProgress.seaCucumberCompleted = true;

        GameProgress.whaleIntroDialogueCompleted = true;
        GameProgress.whalePostCrabDialogueCompleted = true;
        GameProgress.whalePostDolphinDialogueCompleted = true;
        GameProgress.whalePostOctopusDialogueCompleted = true;
        GameProgress.whalePostSeaCucumberDialogueCompleted = true;
        GameProgress.whaleEnsembleAccepted = false;

        SceneSpawnData.returnSpawnPointName = returnSpawnPointName;
        SceneTransition.LoadScene(targetSceneName);
    }
}
