using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Switch : MonoBehaviour
{
    private Collider2D _collider;
    [SerializeField] private GameObject door;
    [SerializeField] private DolphinFollowPositionController dolphinFollowPositionController;
    [SerializeField] private bool isPlayerInRange;
    private Interactable _interactable;
    
    private bool isInteracted = false;
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _interactable = GetComponent<Interactable>();
    }

    private void Update()
    {
        if (isInteracted) return;


        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (door != null) door.SetActive(false);
            _interactable.SetInteractionEnabled(false);
            isInteracted = true;
            if (dolphinFollowPositionController != null) 
                dolphinFollowPositionController.canFollow = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
