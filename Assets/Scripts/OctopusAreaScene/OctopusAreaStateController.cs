using UnityEngine;

public class OctopusAreaStateController : MonoBehaviour
{
    [SerializeField] private GameObject unfinishedObjects;
    [SerializeField] private GameObject completedObjects;

    private void Start()
    {
        if (GameProgress.octopusCompleted)
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
