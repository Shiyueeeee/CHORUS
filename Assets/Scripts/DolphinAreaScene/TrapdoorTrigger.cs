using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrapdoorTrigger : MonoBehaviour
{
    private Collider2D _collider;

    [SerializeField] private GameObject trapdoor;
    [SerializeField] private bool isPlayerInRange = false;
    [SerializeField] private bool isInteracted = false;
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
    }

    private void Update()
    {
        if (isPlayerInRange && !isInteracted)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (trapdoor != null)
                {
                    trapdoor.SetActive(false);
                    isInteracted = true;
                }
            }
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
