using UnityEngine;
using UnityEngine.SceneManagement;

public class CompleteAnimalQuestButton : MonoBehaviour
{
    [SerializeField] private AnimalQuestType animalQuestType;
    [SerializeField] private string returnSceneName = "RQScene";

    public void CompleteQuestAndReturn()
    {
        switch (animalQuestType)
        {
            case AnimalQuestType.Crab:
                GameProgress.crabCompleted = true;
                break;
            case AnimalQuestType.Dolphin:
                GameProgress.dolphinCompleted = true;
                break;
            case AnimalQuestType.Shark:
                GameProgress.sharkCompleted = true;
                break;
            case AnimalQuestType.Octopus:
                GameProgress.octopusCompleted = true;
                break;
            case AnimalQuestType.SeaCucumber:
                GameProgress.seaCucumberCompleted = true;
                break;
            default:
                Debug.LogWarning("No animal quest type selected for CompleteAnimalQuestButton.");
                return;
        }

        SceneTransition.LoadScene(returnSceneName);
    }
}

public enum AnimalQuestType
{
    Crab,
    Dolphin,
    Shark,
    Octopus,
    SeaCucumber
}
