using UnityEngine;

public class SeaCucumberAreaStateController : MonoBehaviour
{
    [SerializeField] private GameObject unfinishedObjects;
    [SerializeField] private GameObject completedObjects;

    private void Start()
    {
        if (GameProgress.seaCucumberCompleted)
        {
            unfinishedObjects.SetActive(false);
            completedObjects.SetActive(true);
        }
        else
        {
            unfinishedObjects.SetActive(true);
            completedObjects.SetActive(false);
        }
    }
}
