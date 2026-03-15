using UnityEngine;

public class SharkTrashObstacle : MonoBehaviour
{
    [SerializeField] private bool disableOnHit = true;

    private bool hasBeenHit;

    public bool RegisterSharkHit()
    {
        if (hasBeenHit)
            return false;

        hasBeenHit = true;
        Debug.Log("Shark collided with trash obstacle: " + gameObject.name);

        if (disableOnHit)
        {
            gameObject.SetActive(false);
        }

        return true;
    }
}
