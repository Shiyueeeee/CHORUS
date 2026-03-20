using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class U : MonoBehaviour
{
    private Collider2D _collider;
    [SerializeField] private GameObject door;
    
    [SerializeField] private DolphinFollowPositionController dolphinFollowPositionController;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (door.activeSelf)
            {
                dolphinFollowPositionController.isFollowing = false;
                dolphinFollowPositionController.canFollow = false;
            }
        }
    }
}
