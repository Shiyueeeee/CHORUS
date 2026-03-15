using UnityEngine;

public class OctopusTaskAreaPortal : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(GameProgress.octopusQuestAccepted);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Octopus task area portal entered. Placeholder only.");
    }
}
