using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ShipTrigger : MonoBehaviour
{
    private Collider2D _collider;

    [SerializeField] private string shipSceneName;
    [SerializeField] private DolphinFollowPositionController followPositionController;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && followPositionController.isFollowing)
        {
            // load in ship scene
            SceneTransition.LoadScene(shipSceneName);
        }
    }
}
