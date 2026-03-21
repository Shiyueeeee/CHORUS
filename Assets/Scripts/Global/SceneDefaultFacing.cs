using UnityEngine;

public class SceneDefaultFacing : MonoBehaviour
{
    [SerializeField] private bool facingRight = true;

    public bool FacingRight => facingRight;
}
