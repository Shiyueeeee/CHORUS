using UnityEngine;

public class SpawnPointFacing : MonoBehaviour
{
    [SerializeField] private bool overrideFacing = true;
    [SerializeField] private bool facingRight = true;

    public bool TryGetFacing(out bool right)
    {
        right = facingRight;
        return overrideFacing;
    }
}
